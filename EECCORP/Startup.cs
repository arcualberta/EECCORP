using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EECCORP.Startup))]
namespace EECCORP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
