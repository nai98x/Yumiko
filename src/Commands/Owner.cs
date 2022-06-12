namespace Yumiko.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;
    using DSharpPlus.SlashCommands;
    using DSharpPlus.SlashCommands.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Yumiko.Utils;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Not with D#+ Command classes")]
    [SlashCommandGroup("owner", "Comandos solo disponibles para el owner de Yumiko")]
    [SlashRequireOwner]
    public class Owner : ApplicationCommandModule
    {
        [SlashCommand("test", "Testing command")]
        public async Task Test(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Nothing to test"));
        }

        [SlashCommand("guild", "Information about a guild")]
        public async Task Guild(InteractionContext ctx, [Option("guild_id", "Guild Id to see details")] string id)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            bool validId = ulong.TryParse(id, out ulong guildId);
            if (validId)
            {
                bool validGuild = ctx.Client.Guilds.TryGetValue(guildId, out DiscordGuild? guild);
                if (validGuild)
                {
                    string desc =
                        $"  - {Formatter.Bold("Id")}: {guild?.Id}\n" +
                        $"  - {Formatter.Bold("Joined date")}: {guild?.JoinedAt}\n" +
                        $"  - {Formatter.Bold("Member count")}: {guild?.MemberCount}\n\n";

                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = guild?.Name,
                        Description = desc,
                        Color = DiscordColor.Green,
                    }));
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"Guild with id {Formatter.InlineCode(id)} not found.",
                        Color = DiscordColor.Red,
                    }));
                }
            }
            else
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = $"Id {Formatter.InlineCode(id)} not valid",
                    Color = DiscordColor.Red,
                }));
            }
        }

        [SlashCommand("guilds", "See Yumiko's guilds")]
        public async Task Servers(InteractionContext ctx, [Option("Guilds", "See guild details")] bool mostrarServers = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var interactivity = ctx.Client.GetInteractivity();
            List<Page> pages = new();
            var guildsdesordenadas = ctx.Client.Guilds.Values;
            var lista = guildsdesordenadas.ToList();
            lista.Sort((x, y) => x.JoinedAt.CompareTo(y.JoinedAt));
            string servers = string.Empty;
            int cont = 1;
            int usuarios = 0;
            int miembros;
            foreach (var guild in lista)
            {
                if (cont >= 10)
                {
                    pages.Add(new Page()
                    {
                        Embed = new DiscordEmbedBuilder
                        {
                            Title = $"{ctx.Client.CurrentUser.Username}'s guilds",
                            Description = servers,
                            Color = Constants.YumikoColor,
                        },
                    });
                    cont = 1;
                    servers = string.Empty;
                }

                miembros = guild.MemberCount - 1;
                servers +=
                    $"{Formatter.Bold(guild.Name)}\n" +
                    $"  - {Formatter.Bold("Id")}: {guild.Id}\n" +
                    $"  - {Formatter.Bold("Joined date")}: {guild.JoinedAt}\n" +
                    $"  - {Formatter.Bold("Member count")}: {guild.MemberCount}\n\n";
                usuarios += miembros;
                cont++;
            }

            if (cont != 1)
            {
                pages.Add(new Page()
                {
                    Embed = new DiscordEmbedBuilder
                    {
                        Title = $"{ctx.Client.CurrentUser.Username}'s guilds",
                        Description = servers,
                        Color = Constants.YumikoColor,
                    },
                });
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{ctx.Client.CurrentUser.Username}'s guilds",
                Description = $"Total member count: {ctx.Client.Guilds.Count}\nTotal users: {usuarios}",
            }));

            if (mostrarServers)
            {
                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, token: new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
            }
        }

        [SlashCommand("deleteguild", "Yumiko leaves a guild")]
        public async Task EliminarServer(InteractionContext ctx, [Option("Id", "Guild Id to exit")] string idStr)
        {
            try
            {
                bool ok = ulong.TryParse(idStr, out ulong id);
                if (ok)
                {
                    var guild = await ctx.Client.GetGuildAsync(id);
                    string nombre = guild.Name;
                    await guild.LeaveAsync();
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"I have left the {Formatter.InlineCode(nombre)} guild ({id})"));
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Id {Formatter.InlineCode(idStr)} not valid"));
                }
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"There was an error getting the guild with the Id: {Formatter.InlineCode(idStr)}"));
            }
        }

        [SlashCommand("poweroff", "Turn off the bot")]
        public async Task Shutdown(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Turning off...").AsEphemeral(true));
            Environment.Exit(0);
        }
    }
}
