using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RecipeParser.Startup))]
namespace RecipeParser
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
