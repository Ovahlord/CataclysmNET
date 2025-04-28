using Microsoft.Extensions.Configuration;

namespace Database.Configuration
{
    public static class ConfigurationManager
    {
        private static readonly IConfigurationRoot _configuration;
        private static readonly Settings _settings;

        static ConfigurationManager()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("databaseconfig.json", optional: false, reloadOnChange: false)
                .Build();

            _settings = _configuration.GetSection("Settings").Get<Settings>()
                ?? throw new InvalidOperationException("Error: Configuration could not be loaded!");
        }

        public static Settings Settings => _settings;
    }
}
