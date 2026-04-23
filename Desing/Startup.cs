using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Desing.Startup))]
namespace Desing
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
