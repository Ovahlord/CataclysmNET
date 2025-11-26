using Microsoft.Extensions.Configuration;

namespace CataclysmNET.Core.Utils
{
    public static class AppSettings
    {
        public static IConfigurationRoot Configuration { get; private set; }

        static AppSettings()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }
    }
}
