using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YumikoBot.Modulos
{
    public class SlashCommands : SlashCommandModule
    {
        [SlashCommand("avatar", "Get someone's avatar")]
        public async Task Av(InteractionContext ctx, [Option("user", "The user to get it for")] DiscordUser user = null)
        {
            user ??= ctx.Member;
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Avatar",
                ImageUrl = user.AvatarUrl
            }.
            WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl).
            WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        }

        //Attribute choices
        [SlashCommand("phrase", "Sends a certain phrase in the chat!")]
        public async Task Phrase(InteractionContext ctx,
            [Choice("phrase1", "all's well that ends well")]
            [Choice("phrase2", "be happy!")]
            [Option("phrase", "the phrase to respond with")] string phrase)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(phrase));
        }

        //Enum choices
        public enum MyEnum
        {
            [ChoiceName("Option 1")]
            option1,
            [ChoiceName("Option 2")]
            option2,
            [ChoiceName("Option 3")]
            option3
        }

        [SlashCommand("enum", "Test enum")]
        public async Task EnumCommand(InteractionContext ctx, [Option("enum", "enum option")] MyEnum myEnum = MyEnum.option1)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(myEnum.GetName()));
        }

        //ChoiceProvider choices
        public class TestChoiceProvider : IChoiceProvider
        {
            public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            {
                return new DiscordApplicationCommandOptionChoice[]
                {
                    new DiscordApplicationCommandOptionChoice("testing", "testing"),
                    new DiscordApplicationCommandOptionChoice("testing2", "test option 2")
                };
            }
        }

        [SlashCommand("choiceprovider", "test")]
        public async Task ChoiceProviderCommand(InteractionContext ctx,
            [ChoiceProvider(typeof(TestChoiceProvider))]
            [Option("option", "option")] string option)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(option));
        }
    }
}
