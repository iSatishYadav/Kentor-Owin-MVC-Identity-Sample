using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SamlApp.Startup))]
namespace SamlApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
