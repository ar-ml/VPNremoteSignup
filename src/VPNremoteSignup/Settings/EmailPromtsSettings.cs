namespace ARaction.VPNremoteSignup.Settings
{
    public sealed class EmailPromptsSettings
    {
        public bool AppendPcNameToPrompts { get; set; }
        public string MailSubjectStartVPN { get; set; }
        public string MailSubjectEndVPN { get; set; }
        public string MailSubjectToken { get; set; }
        public string MailSubjectPing { get; set; }
        public string ReplyMailSubject { get; set; }
        public string ReplyEmailAdress { get; set; }
    }
}
