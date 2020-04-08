using TCAdmin.SDK.Mail;

namespace TCAdminModule.Configurations
{
    public class LoginNotificationSettings
    {
        public string FromEmail { get; set; } = new MailConfig().DefaultFromEmail;
        public string FromName { get; set; } = new MailConfig().DefaultFromName;
        public bool SendEmail { get; set; } = true;
        public bool ForceToViewOnPanel { get; set; } = true;

        public string Subject { get; set; } = "Discord Access";
        public string MessageContents { get; set; } =
            "<h3>Hello {ThisUser.FullName},</h3><br /><br />" +
            "[This is an automated message from Nexus]<br />We are letting you know that your {CompanyInfo.CompanyName} account was used to authenticate through discord." +
            "Please find details below of the request.<br />" +
            "Discord Account: <b>{Member.Username}#{Member.Discriminator} ({Member.Id})</b><br />" +
            "Discord Server: <b>{Guild.Name} ({Guild.Id})</b><br />" +
            "Time Requested: <b>{DateTime}</b><br /><br />" +
            "If you do not recognise this request, please immediately raise a support ticket!<br />" +
            "{CompanyInfo.SignatureHtml}";
    }
}