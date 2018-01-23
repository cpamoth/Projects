using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Hello_Web_Pages.Startup))]
namespace Hello_Web_Pages
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
