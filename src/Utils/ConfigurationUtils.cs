namespace Yumiko.Utils
{
    public static class ConfigurationUtils
    {
        public static T GetConfiguration<T>(IConfigurationRoot config, Configurations type)
        {
            return type switch
            {
                Configurations.Website => config.GetValue<T>("website") ?? throw new NullReferenceException("config.json: Missing website"),
                Configurations.InviteUrl => config.GetValue<T>("invite_url") ?? throw new NullReferenceException("config.json: Missing bot invite url"),
                Configurations.TokenDiscordProduction => config.GetValue<T>("tokens:discord:production") ?? throw new NullReferenceException("config.json: Missing production bot token"),
                Configurations.TokenDiscordTesting => config.GetValue<T>("tokens:discord:testing") ?? throw new NullReferenceException("config.json: Missing test bot token"),
                Configurations.TokenTopgg => config.GetValue<T>("tokens:topgg") ?? throw new NullReferenceException("config.json: Missing topgg token"),
                Configurations.TokenOpenWeatherMap => config.GetValue<T>("tokens:openweathermap") ?? throw new NullReferenceException("config.json: Missing openweathermap token"),
                Configurations.TokenTheCatApi => config.GetValue<T>("tokens:thecatapi") ?? throw new NullReferenceException("config.json: Missing thecatapi token"),
                Configurations.TokenTheDogApi => config.GetValue<T>("tokens:thedogapi") ?? throw new NullReferenceException("config.json: Missing therdogapi token"),
                Configurations.LogginGuildId => config.GetValue<T>("loggin:guild_id") ?? throw new NullReferenceException("config.json: Missing guild id for loggin"),
                Configurations.LogginProductionApplicationCommands => config.GetValue<T>("loggin:production:application_commands") ?? throw new NullReferenceException("config.json: Missing production application commands channel id for loggin"),
                Configurations.LogginProductionGuilds => config.GetValue<T>("loggin:production:guilds") ?? throw new NullReferenceException("config.json: Missing production guilds channel id for loggin"),
                Configurations.LogginProductionErrors => config.GetValue<T>("loggin:production:errors") ?? throw new NullReferenceException("config.json: Missing production errors channel id for loggin"),
                Configurations.LogginTestingApplicationCommands => config.GetValue<T>("loggin:testing:application_commands") ?? throw new NullReferenceException("config.json: Missing testing application commands channel id for loggin"),
                Configurations.LogginTestingGuilds => config.GetValue<T>("loggin:testing:guilds") ?? throw new NullReferenceException("config.json: Missing testing guilds channel id for loggin"),
                Configurations.LogginTestingErrors => config.GetValue<T>("loggin:testing:errors") ?? throw new NullReferenceException("config.json: Missing testing errors channel id for loggin"),
                Configurations.FirebaseDatabaseName => config.GetValue<T>("firebase_database_name") ?? throw new NullReferenceException("config.json: Missing firebase database name"),
                Configurations.TopggEnabled => config.GetValue<T>("topgg_enabled") ?? throw new NullReferenceException("config.json: Missing topgg enabled"),
                Configurations.TimeoutGeneral => config.GetValue<T>("timeouts:general") ?? throw new NullReferenceException("config.json: Missing general timeout"),
                Configurations.TimeoutGames => config.GetValue<T>("timeouts:games") ?? throw new NullReferenceException("config.json: Missing games timeout"),
                _ => throw new ArgumentException("Invalid Configuration type"),
            };
        }
    }
}