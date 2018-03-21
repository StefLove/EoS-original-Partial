using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EoS.Startup))]
namespace EoS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
