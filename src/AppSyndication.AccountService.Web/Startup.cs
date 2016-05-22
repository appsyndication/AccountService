using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using AppSyndication.AccountService.Web.Models;
using AppSyndication.AccountService.Web.Services;
using BrockAllen.MembershipReboot;
using FireGiant.MembershipReboot.AzureStorage;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSyndication.AccountService.Web
{
    public class Startup
    {
        public Startup()
        {
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings_dev.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = this.Configuration["AtsUserAccountConfig:ConnectionString"];

            services.Configure<OpenIdConnectConfig>(this.Configuration.GetSection("OpenIdConnect"));

            services.AddSingleton<AtsUserService>()
                .AddSingleton<AtsUserRepository>()
                .AddSingleton(_ =>
            {
                var appInfo = new RelativePathApplicationInformation(this.Configuration["PublicOrigin"])
                {
                    ApplicationName = "AppSyndication Account",
                    RelativeCancelVerificationUrl = "/account/update/cancel/",
                    RelativeConfirmChangeEmailUrl = "/account/update/email/",
                    RelativeConfirmPasswordResetUrl = "/account/update/password/",
                    RelativeLoginUrl = "/account/"
                };

                var mailConfig = this.Configuration.Get<MailConfig>("Mail");

                var config = new AtsUserServiceConfig(connectionString, tenant: "appsyndication");
                config.AddEventHandler(new EmailAccountEventsHandler<AtsUser>(new EmailFormatter(appInfo), new ConfigurableMessageDelivery(mailConfig)));

                return config;
            });

            services.AddAuthentication(sharedOptions =>
                sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.ConfigureRouting(options =>
            {
                options.AppendTrailingSlash = true;
                options.LowercaseUrls = true;
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<OpenIdConnectConfig> oidcConfig)
        {
            var sourceSwitch = new SourceSwitch("AppSyndicationAccountTraceSource") { Level = SourceLevels.Warning };
            loggerFactory.AddTraceSource(sourceSwitch, new AzureApplicationLogTraceListener());
            loggerFactory.AddTraceSource(sourceSwitch, new EventLogTraceListener("Application"));

            //loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                //app.UseBrowserLink();
                loggerFactory.AddDebug();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/account/error");
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseIISPlatformHandler();

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.AutomaticChallenge = true;

                options.Authority = oidcConfig.Value.Authority;
                options.ClientId = oidcConfig.Value.ClientId;
                options.ClientSecret = oidcConfig.Value.ClientSecret;
                options.CallbackPath = "/account/signin-oidc";

                options.ResponseType = OpenIdConnectResponseTypes.Code;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Add("email");
                options.Scope.Add("roles");
                options.TokenValidationParameters.NameClaimType = "sub";

#if DEBUG
                options.RequireHttpsMetadata = false;
#endif
                //options.PostLogoutRedirectUri = "https://www.appsyndication.com/";
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
