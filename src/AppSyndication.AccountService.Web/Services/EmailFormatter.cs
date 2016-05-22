using System.Collections.Generic;
using BrockAllen.MembershipReboot;
using FireGiant.MembershipReboot.AzureStorage;

namespace AppSyndication.AccountService.Web.Services
{
    public class EmailFormatter : EmailMessageFormatter<AtsUser>
    {
        public EmailFormatter(ApplicationInformation appInfo)
            : base(appInfo)
        {
        }

        protected override string GetBody(UserAccountEvent<AtsUser> evt, IDictionary<string, string> values)
        {
            if (evt is PasswordChangedEvent<AtsUser> && !evt.Account.IsAccountVerified)
            {
                return null;
            }

            return base.GetBody(evt, values);
        }
    }
}
