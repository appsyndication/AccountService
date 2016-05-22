namespace AppSyndication.AccountService.Web.Models
{
    public class MailConfig
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DefaultFrom { get; set; }

        public int Timeout { get; set; } = 5000;
    }
}
