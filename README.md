# VPNremoteSignup
Windows Tool for triggering a signup with FortiClient VPN from remote via Email. (Experimental!)

# Why VPNremoteSignup?
- After the major Anydesk hack in early 2024, we sought a completely independent remote access solution.
- We lacked trust in Teamviewer either and found RustDesk to require excessive maintenance efforts.
- Additionally, we opted to avoid permanent VPN connections on client laptops due to increased security risks and continuous consumption of network resources. We preferred on-demand VPN signups with 2FA for maximum control.
- Unattended access: Nevertheless we needed a tool that enables admins to remotely initiate a VPN connection on a user's PC, even without the user present. Once the VPN connection is established, the admin can easily utilize Windows RDP to access the PC.

# How does it work?
- The program runs in background and regularly checks a mail server for new emails and fetches new mails via IMAP
- It checks if email subject matches with a predefined value (e.g. "start_SSL_VPN_")
- If subject is correct, FortiClient VPN (command line version) will be opened
- Now predefined user credentials will be loaded into FortiClient VPN ( e.g. username "myVPNuser", password "myVPNPassword123#") and the connection process will be started.
- The program was built for SSL VPN connections with enabled 2FA. FortiClient VPN will now show another input field that asks for a 2FA-Token.
- The program will look for another Email with the correct subject, e.g. "2FA_". This Email will contain the token. (e.g. "123456")
- It will now extract the token from the email body, paste it into the FortiClient token input field and hit connect.
- Once the connection is established, the program will send an email with the following message:
"Client COMPUTERNAME is connected now!" The message will also contain information about all public and local IP addresses that the device is currently using.
- If an email with subject "end_SSL_VPN_" is received, the program will terminate the SSL connection and close FortiClient.

# Important Notes: 
- The program executable is called VPNremoteSignup.exe. It will run in the background, without any visible GUI.
- If you want to terminate it, use windows Task manager, search for VPNremoteSignup.exe and end the process.
- All relevant settings can be made in appsetings.json
- The program is considered experimental. Use at your own risk. No warranty at all!

# Do I have to buy a license to use this code / software?
USE OF THIS CODE AND SOFTWARE IS FREE FOR NON-COMMERICAL USE ONLY!
READ LICENSE FILE FOR DETAILS!

To get a commercial license for use on one computer, simply send 60€ or 65 USD (onetime fee, lifetime use) to the following paypal address:
pay@ar-action.com
We depend on your fairness to support our development efforts by paying the license fee.
The Paypal Receipt will act as a license document. You will not receive any emails or documents from us. A license key is not needed.
Please note, that you buy a license for experimental code, not a final product. We do NOT provide any warranty or support.

If you buy more than 5 licenses at the same time, you are allowed to reduce the price for each license to 45€ / 50 USD.
If you buy more than 10 licenses at the same time, you are allowed to reduce the price for each license to 35€ / 40 USD.
For a bigger amount of commercial license or use of our code in a commercial product, you can contact us:
https://www.ar-action.com/impressum/

# Prerequisites
- On your Fortigate Firewall you must have set up a working SSL VPN full tunnel with 2FA token authentication.
- You must have FortiClient VPN installed on your machine and it must be running. (It can be running in background, check for tray icon.)
Use the online Installer under \FortiClientTools\ or Download it from https://www.fortinet.com/support/product-downloads#vpn
- You must have FortiClient Commandline tool on your hard disk. You can downlaod it from https://support.fortinet.com/Download/FirmwareImages.aspx Read this file for more Info: https://github.com/ar-ml/VPNremoteSignup/blob/main/FortiClientTools/SSLVPNcmdline/Readme_FortiClientToolsUsedVersion.txt
- In appsetings.json you must set the correct "ClientPath" to FortiClient Commandline tool.
- Firewall settings: The app communicates via IMAP and SMTP. Make sure, your firewall allows to do that.
- Additionally, for certain functions, the app attempts to determine the machine's public IP address and will send it to you via email. This process is carried out using HTTPS, through the service provided by ifconfig.co. Depending on your firewall settings, you might receive a notification prompting you to allow this action. Depending on your configuration, you may need to manually configure your firewall to permit this.

Note: Our program does not transmit any data to servers other than the email server you have configured. However, it's important to note that ifconfig.co may retain logs of requests made to their service, including public IP addresses. Should you decide to block communication with ifconfig.co, the majority of the program's features should continue to function normally.

# Can I encrypt my passwords?
- Yes, it is possible.
- But by default encryption is disabled, so you have to store your login credentials in plaintext in the appsetings.json
- We have implemented a basic credentials encryption feature. It increases security a bit but be aware, if the computer is contaminated, or someone has physical access to it, it might be possible to get around password encryption!
- Read Readme.txt in folder /src/EncryptText for more information on how to use credentials encryption!

# How does the option "AppendPCnameToPromts" work?
In appsettings.json you can set this option like this:
```
  "EmailPromts": {
    "AppendPCnameToPromts": true,
    "MailSubjectStartVPN": "start_",
    "MailSubjectEndVPN": "end_",
    "MailSubjectToken": "token_",
```
If AppendPCnameToPromts is set to true, the programm automatically appends the COMPUTERNAME to all three email prompts.

This means for the example configuration above:
If my Computer name is "CHARLESPC", the programm would listen to:

```
start_CHARLESPC
end_CHARLESPC
token_CHARLESPC
```

In other words:
If AppendPCnameToPromts is set to true, it will not react to an email with subject "start_".
Only to subject "start_CHARLESPC".

# How does the integrated Ping function work?
You can use this function to "ping" the PC via Email, even if you do not know its current public IP address.
The program listens for another email subject, e.g. "pingtest_PC1" (can be configured in json file).
If an Email with this subject is being received, it sends an Email to the configured reply address.
The reply will look something like this:
```
Email subject: PCNAME is currently online.
Email body:
PCNAME public IP:
xxx.xxx.xxx.xxx

PCNAME private IPs:
list of all private ips
```
  
# How to build an executable application from the code?
Note: This section is relevant for developers only. If you're not a developer, you can just download a ready to use install package from the releases section of this githup repo:  <br/>https://github.com/ar-ml/VPNremoteSignup/releases

- Make sure .NET Framework 4.6.2 Developer Pack is installed on your computer
you can download it here: https://dotnet.microsoft.com/en-us/download/visual-studio-sdks?cid=getdotnetsdk
- Make sure Visual Studio 2022 is installed on your computer
you can download it here: https://visualstudio.microsoft.com/de/downloads/
- Clone the whole repo to your PC.
You can use github Desktop for it: https://desktop.github.com/
- Open the root folder in visual studio.
- Open VPNremoteSignup.sln in visual studio
- Configure the right values (like IMAP and SMTP credentials, VPN credentials and Email subjects) in appsetings.json
- In visual Studio hit F5 button to make a build
- Test the application.
- After quitting Visual Studio you can find the build under yourLocalGitRepo\src\VPNremoteSignup\bin\Debug and there you can manually execute VPNremoteSignup.exe
- Also you can modify appsetings.json in this path again, if needed


# How to install the application on windows machines (with autostart)?
You can use the install package from the releases section of this githup repo and install it on your machines: <br/>
https://github.com/ar-ml/VPNremoteSignup/releases <br/>
In this case, don't forget to configure your settings in C:\VPNremoteSignup\appsetings.json after each installation. A restart might be needed afterwards.

Alternatively, you can create a fresh install package from sourcecode, that will already contain all your settings and can easily be distributed on several machines:
- First install **Inno Setup** from https://jrsoftware.org/isdl.php (version 6.2.2)
- Make sure that in appsetings.json all settings are set and the following path is configured:<br>
```
"ClientPath": "C:\\VPNremoteSignup\\FortiClient\\FortiSSLVPNclient.exe",
```
- In Visual Studio build project in **Release** mode
- Make sure you have placed the FortiClientTools into the right folder (\gitrepo\FortiClientTools\SSLVPNcmdline\x86)
- Run **CreateSetup.bat** in root folder
- There is **VPNremoteSignupSetup.exe** in the **Output** folder.
- You can now use this install package to install it on any Windows 10 or 11 computer.


# How to uninstall the application?
- First of all use windows Task manager, search for VPNremoteSignup.exe and end the process.
-  Then you can just execute C:\VPNremoteSignup\unins000.exe
OR:
- Delete the folder C:\VPNremoteSignup
- Remove VPNremoteSignup.exe from autostart by deleting it here: C:\Users\YourUsername\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup


# What's in the (sourcecode) folders of this repository?

\FortiClientTools\
- Description: Contains FortiClient VPN Online Installer and FortiClient Commandline tool.
- Source: https://fortinet.com

\RuntimeEnvironment\
- Description: Contains VC_redist.x64 and vc_redist.x86.exe that came with FortiClient.
- Source: https://fortinet.com

\src\VPNremoteSignup\App.config
- Description: auto-generated file
- Source: own code

\src\VPNremoteSignup\appsettings.json
- Description: json file that stores all settings for the app to work. You have to enter your data here.
- Source: own code

\src\VPNremoteSignup\packages.config
- Description: list of Nuget packages
- Source: own code

\src\VPNremoteSignup\Program.cs
- Description: main code
- Source: own code

\src\VPNremoteSignup\VPNremoteSignup.csproj
- Description: project file
- Source: own code

\src\VPNremoteSignup\Properties
- Description: folder with AssemblyInfo.cs file
- Source: own code

\src\VPNremoteSignup\Settings
- Description: folder with settings classes
- Source: own code

\src\EncryptText\
- Description: simple commandline tool for encrypting text strings (like usernames / passwords) with an 256-bit AES key
- read Readme.txt in that subfoler for more information!

\src\AesLib\
- Description: Library for using 256-bit AES encrypted usernames and passwords
- read Readme.txt in \src\EncryptText\ for more information how to use encryption

\CreateSetup.cmd
- Description: Script that runs Inno Setup to create an install package
- Source: https://jrsoftware.org/isdl.php, modified

\setup.iss
- Description: Settings for Inno Setup
- Source: https://jrsoftware.org/isdl.php, modified
