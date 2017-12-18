using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using EECCORP.Models;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace EECCORP
{
    public partial class Startup
    {

        private ApplicationDbContext _Db;
        private ApplicationDbContext Db { get { if (_Db == null) _Db = new ApplicationDbContext(); return _Db; } }

        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            GoogleOAuth2AuthenticationOptions googleOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["Authentication:Google:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["Authentication:Google:Secret"]
            };

            googleOptions.Provider = new GoogleOAuth2AuthenticationProvider()
            {
                // [START read_google_profile_image_url]
                // After OAuth authentication completes successfully,
                // read user's profile image URL from the profile
                // response data and add it to the current user identity
                OnAuthenticated = context =>
                {
                    var profileUrl = context.User["image"]["url"].ToString();
                    var displayName = context.User["displayName"].ToString();
                    context.Identity.AddClaim(new Claim(ClaimTypes.Uri, profileUrl));
                    context.Identity.AddClaim(new Claim(ClaimTypes.Name, displayName));

                    return Task.FromResult(0);
                }
                // [END read_google_profile_image_url]
            };

            app.UseGoogleAuthentication(googleOptions);
            CreateRolesAndUser();
        }

        private void CreateRolesAndUser()
        {            
            RoleManager<IdentityRole> roleManager = 
                new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(Db));
            UserManager<ApplicationUser> userManager = 
                new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(Db));

            string adminRoleName = "Admin";

            if (!roleManager.RoleExists(adminRoleName))
            {
                IdentityRole adminRole = new IdentityRole();
                adminRole.Name = adminRoleName;
                roleManager.Create(adminRole);
            }

           
        }
    }
}