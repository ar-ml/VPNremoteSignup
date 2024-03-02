using ARaction.AesLib;
using ARaction.VPNremoteSignup.Settings;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ARaction.VPNremoteSignup
{
    internal class Program
    {
        static ApplicationSettings applicationSettings;
        static EmailPromptsSettings emailPromptsSettings;
        static ImapSettings imapSettings;
        static SmtpSettings smtpSettings;
        static VpnSettings vpnSettings;

        static Application application;

        static readonly HttpClient client = new HttpClient();

        private const int SW_RESTORE = 9;

        static string Key => "doqya80QeTBft210Vvp0uDGvSgd76Dfn";

        static async Task Main()
        {
            await RunService();
        }

        public static async Task RunService()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            applicationSettings = configuration.GetRequiredSection("ApplicationSettings").Get<ApplicationSettings>();
            emailPromptsSettings = configuration.GetRequiredSection("EmailPrompts").Get<EmailPromptsSettings>();
            imapSettings = configuration.GetRequiredSection("ImapSettings").Get<ImapSettings>();
            smtpSettings = configuration.GetRequiredSection("SmtpSettings").Get<SmtpSettings>();
            vpnSettings = configuration.GetRequiredSection("VpnSettings").Get<VpnSettings>();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            if (applicationSettings.UseEncryption)
            {
                imapSettings.Username = imapSettings.Username.Decrypt(Key);
                imapSettings.Password = imapSettings.Password.Decrypt(Key);
                smtpSettings.Username = smtpSettings.Username.Decrypt(Key);
                smtpSettings.Password = smtpSettings.Password.Decrypt(Key);
                vpnSettings.Username = vpnSettings.Username.Decrypt(Key);
                vpnSettings.Password = vpnSettings.Password.Decrypt(Key);
            }

            while (true)
            {
                try
                {
                    await ReceiveAndProcessMail();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                finally
                {
                    Thread.Sleep(TimeSpan.FromSeconds(imapSettings.EmailCheckTimeInSeconds));
                }
            }
        }

        static async Task ReceiveAndProcessMail()
        {
            Log.Information("ReceiveAndProcessMail");
            using (var imapClient = new ImapClient())
            {
                try
                {
                    await imapClient.ConnectAsync(imapSettings.Host, imapSettings.Port, true);
                    await imapClient.AuthenticateAsync(imapSettings.Username, imapSettings.Password);
                    await imapClient.Inbox.OpenAsync(FolderAccess.ReadWrite);

                    var uids = await imapClient.Inbox.SearchAsync(SearchQuery.NotSeen);
                    foreach (var uid in uids)
                    {
                        var mimeMessage = await imapClient.Inbox.GetMessageAsync(uid);
                        await ProcessMail(mimeMessage, uid, imapClient);
                    }
                }
                finally
                {
                    await imapClient.DisconnectAsync(true);
                }
            }
        }

        static async Task ProcessMail(MimeMessage message, UniqueId uid, ImapClient imapClient)
        {
            if (IsMessageForStartClient(message))
            {
                imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, true);
                StartFortiClient();
            }
            else if (IsMessageForStopClient(message))
            {
                imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, true);
                StopFortiClient();
            }
            else if (IsMessageForEnterTokenClient(message))
            {
                imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, true);
                Enter2FaTokenFortiClient(message.TextBody.Trim());
                await SendConnectedResponse();
            }
            else if (IsMessageForPing(message))
            {
                imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, true);
                await SendPingResponse();
            }
        }

        static bool IsMessageForStartClient(MimeMessage message) => message.Subject.StartsWith(FormatEmailPrompt(emailPromptsSettings.MailSubjectStartVPN));

        static bool IsMessageForStopClient(MimeMessage message) => message.Subject.StartsWith(FormatEmailPrompt(emailPromptsSettings.MailSubjectEndVPN));

        static bool IsMessageForEnterTokenClient(MimeMessage message) => message.Subject.StartsWith(FormatEmailPrompt(emailPromptsSettings.MailSubjectToken));

        static bool IsMessageForPing(MimeMessage message) => message.Subject.StartsWith(FormatEmailPrompt(emailPromptsSettings.MailSubjectPing));

        static string FormatEmailPrompt(string text) => emailPromptsSettings.AppendPcNameToPrompts ? text + Dns.GetHostName() : text;

        static void StartFortiClient()
        {
            application = null;
            foreach (Process process in Process.GetProcessesByName("FortiSSLVPNclient"))
            {
                process.Kill();
            }

            Log.Information("StartFortiClient");
            var arguments = $"connect -h {vpnSettings.Host} -u {vpnSettings.Username}:{vpnSettings.Password} -i -q -m";
            application = Application.Launch(vpnSettings.ClientPath, arguments);
        }

        static void Enter2FaTokenFortiClient(string token)
        {
            Log.Information("Enter2FaTokenFortiClient");
            if (application == null)
            {
                throw new Exception("The application is not running");
            }

            using (var automation = new UIA3Automation())
            {
                var window = application.GetMainWindow(automation);
                var tokenEdit = Retry.WhileNull(() => window.FindFirstByXPath("//Edit[@Name='FortiToken Code:']"),
                    TimeSpan.FromSeconds(30), null, true, false, "No input for token").Result;

                ShowWindow(application.MainWindowHandle, SW_RESTORE);

                tokenEdit.AsTextBox().Text = token;
                var login = window.FindFirstByXPath("//Button[@Name='Login']");
                login.Click();

                Retry.WhileNull(() => window.FindFirstByXPath("//Text[@Name='Connected']"),
                    TimeSpan.FromSeconds(30), null, true, false, "Not connected");
            }
        }

        static void StopFortiClient()
        {
            Log.Information("StopFortiClient");
            var processStartInfo = new ProcessStartInfo(vpnSettings.ClientPath, "disconnect");
            Process.Start(processStartInfo);

            application = null;
        }

        static async Task SendConnectedResponse()
        {
            Log.Information("SendConnectedResponse");

            var hostName = Dns.GetHostName();
            var addressesString = string.Join("\n", await GetPrivateIps());

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(smtpSettings.From, smtpSettings.From));
            email.To.Add(new MailboxAddress(emailPromptsSettings.ReplyEmailAdress, emailPromptsSettings.ReplyEmailAdress));
            email.Subject = emailPromptsSettings.ReplyMailSubject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = $"Client {hostName} is connected now!\n"
                        + $"{hostName} public IP:\n"
                        + $"{await GetPublicIp()}\n"
                        + $"{hostName} private IPs:\n"
                        + $"{addressesString}",
            };

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(smtpSettings.Host, smtpSettings.Port, true);
                await smtp.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password);
                var result = await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
        }

        static async Task SendPingResponse()
        {
            Log.Information("SendPingResponse");

            var hostName = Dns.GetHostName();
            var addressesString = string.Join("\n", await GetPrivateIps());

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(smtpSettings.From, smtpSettings.From));
            email.To.Add(new MailboxAddress(emailPromptsSettings.ReplyEmailAdress, emailPromptsSettings.ReplyEmailAdress));
            email.Subject = $"{hostName} is currently online";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = $"{hostName} public IP:\n"
                        + $"{await GetPublicIp()}\n"
                        + $"{hostName} private IPs:\n"
                        + $"{addressesString}",
            };

            using (var smtp = new SmtpClient())
            {
                try
                {
                    await smtp.ConnectAsync(smtpSettings.Host, smtpSettings.Port, true);
                    await smtp.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password);
                    await smtp.SendAsync(email);
                }
                finally
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }

        static async Task<List<string>> GetPrivateIps()
        {
            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(string.Empty);
                return hostEntry.AddressList.Select(x => x.ToString()).ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        static async Task<string> GetPublicIp()
        {
            try
            {
                var result = await client.GetStringAsync("https://ifconfig.co/ip");
                return result.Trim();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWind, int nCmdShow);
    }
}
