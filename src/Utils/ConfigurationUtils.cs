namespace Yumiko.Utils
{
    public static class ConfigurationUtils
    {
        public static T GetConfiguration<T>(IConfigurationRoot config, Configurations type)
        {
            T? configuration = GetConfigurationFromJson<T>(config, type);

            if (configuration != null)
            {
                return configuration;
            }
            else
            {
                throw new NullReferenceException($"config.json: Missing {Enum.GetName(type)}");
            }
        }

        private static T GetConfigurationFromJson<T>(IConfigurationRoot config, Configurations type)
        {
            return type switch
            {
                Configurations.Website => config.GetValue<T>("website"),
                Configurations.TokenDiscordProduction => config.GetValue<T>("tokens:discord:production"),
                Configurations.TokenDiscordTesting => config.GetValue<T>("tokens:discord:testing"),
                Configurations.TokenTopgg => config.GetValue<T>("tokens:topgg"),
                Configurations.TokenOpenWeatherMap => config.GetValue<T>("tokens:openweathermap"),
                Configurations.TokenTheCatApi => config.GetValue<T>("tokens:thecatapi"),
                Configurations.TokenTheDogApi => config.GetValue<T>("tokens:thedogapi"),
                Configurations.LogginGuildId => config.GetValue<T>("loggin:guild_id"),
                Configurations.LogginProductionGuilds => config.GetValue<T>("loggin:production:guilds"),
                Configurations.LogginProductionErrors => config.GetValue<T>("loggin:production:errors"),
                Configurations.LogginTestingGuilds => config.GetValue<T>("loggin:testing:guilds"),
                Configurations.LogginTestingErrors => config.GetValue<T>("loggin:testing:errors"),
                Configurations.TopggEnabled => config.GetValue<T>("topgg_enabled"),
                Configurations.TimeoutGeneral => config.GetValue<T>("timeouts:general"),
                Configurations.TimeoutGames => config.GetValue<T>("timeouts:games"),
                _ => throw new ArgumentException("Invalid Configuration type"),
            };
        }
    }
}