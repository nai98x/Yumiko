using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.EventHandling;
using Yumiko.Application.Fun;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Events.Handlers;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Games;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Bot.Services.State;

namespace Yumiko.Bot.Commands.Slash;

[TestCommand]
public sealed class Interact(
    ILocalizer localizer,
    DiscordBotService discordBotService,
    PollState pollState,
    InteractivityExtension interactivity,
    IHttpClientFactory httpClientFactory)
{
    private const int MaxPollOptions = 25;

    private static string ImagePath(string fileName) => Path.Join(AppContext.BaseDirectory, "Images", fileName);

    [Command("say")]
    [Description("Replicates a text")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [RequirePermissions(DiscordPermission.SendMessages, DiscordPermission.SendThreadMessages, DiscordPermission.ViewChannel)]
    public async Task SayAsync(
        SlashCommandContext ctx,
        [Parameter("message")] [Description("The text you want to replicate")] string text)
    {
        await ctx.DeferResponseAsync();
        await ctx.DeleteResponseAsync();
        await ctx.Channel.SendMessageAsync(text);
    }

    [Command("question")]
    [Description("Responds with yes or no")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task QuestionAsync(
        SlashCommandContext ctx,
        [Parameter("question")] [Description("The question you want to ask")] string text)
    {
        Loc loc = ctx.Loc(localizer);
        bool isYes = RandomHelper.GetRandomNumber(0, 1) == 1;
        string answer = (isYes ? loc[Keys.yes] : loc[Keys.no]).ToUpper(loc.Culture);

        await ctx.RespondAsync(new DiscordEmbedBuilder
        {
            Color = isYes ? DiscordColor.Green : DiscordColor.Red,
            Title = loc[Keys.yes_or_no],
            Description = $"{Formatter.Bold($"{loc[Keys.question]}:")} {text}\n{Formatter.Bold($"{loc[Keys.answer]}:")} {answer}",
        });
    }

    [Command("choose")]
    [Description("Choose from multiple options separated by commas")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task ChooseAsync(
        SlashCommandContext ctx,
        [Parameter("question")] [Description("The question you want to ask")] string question,
        [Parameter("options")] [Description("Comma separated options")] string optionsInput)
    {
        Loc loc = ctx.Loc(localizer);

        List<string> options = [.. optionsInput.Split(',')];
        int chosenIndex = RandomHelper.GetRandomNumber(0, options.Count - 1);

        string optionsList = Formatter.Bold($"{loc[Keys.options]}:")
                         + string.Concat(options.Select(o => $"\n- {o}"));

        await ctx.RespondAsync(new DiscordEmbedBuilder
        {
            Color = YumikoColors.Primary,
            Title = loc[Keys.question],
            Description = $"{Formatter.Bold(question)}\n\n{optionsList}\n\n{Formatter.Bold($"{loc[Keys.answer]} :")} {options[chosenIndex]}".NormalizeDescription(),
        });
    }

    [Command("poll")]
    [Description("Do a poll in the server")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [RequireGuild]
    public async Task PollAsync(
        SlashCommandContext ctx,
        [Parameter("limit")] [Description("Limit to end the poll (in minutes)")] [MinMaxValue(1, 10)] long timeout,
        [Parameter("anonymous")] [Description("If you want the poll to be anonymous")] bool anonymous)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        string pollId = $"{ctx.Interaction.Id}";
        string modalId = $"poll-{pollId}";

        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordModalBuilder()
            .WithCustomId(modalId)
            .WithTitle(loc[Keys.poll])
            .AddTextInput(
                new DiscordTextInputComponent("poll_title", placeholder: loc[Keys.poll_title_placeholder], style: DiscordTextInputStyle.Short, max_length: 200),
                loc[Keys.title])
            .AddTextInput(
                new DiscordTextInputComponent("poll_options", placeholder: loc[Keys.poll_options_placeholder], style: DiscordTextInputStyle.Paragraph),
                loc[Keys.options]));

        InteractivityResult<ModalSubmittedEventArgs> result = await interactivity.WaitForModalAsync(modalId, TimeSpan.FromMinutes(5));

        if (result.TimedOut)
        {
            return;
        }

        DiscordInteraction interaction = result.Result.Interaction;

        await interaction.CreateResponseAsync(
            DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.success],
                Description = loc[Keys.creating_poll],
                Color = DiscordColor.Green,
            }).AsEphemeral());

        string title = result.Result.TextOf("poll_title");
        List<string> options =
        [
            .. result.Result.TextOf("poll_options")
                .Trim()
                .Split(',')
                .Select(o => o.Trim().NormalizeSelectMenuOption())
                .Where(o => o.Length > 0)
                .Distinct(),
        ];

        if (options.Count <= 1)
        {
            await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(ErrorEmbed.Create(loc[Keys.error], loc[Keys.error_more_than_one_option])));
            return;
        }

        if (options.Count > MaxPollOptions)
        {
            await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(ErrorEmbed.Create(loc[Keys.error], loc[Keys.error_max_options_limit])));
            return;
        }

        Poll poll = new() { Id = pollId, Title = title, Options = options };
        pollState.Add(poll);

        DiscordMessage pollMsg = await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{loc[Keys.poll]}: {title}",
                Description =
                    $"{Formatter.Bold(loc[Keys.anonymous_poll])}: {loc[anonymous ? Keys.yes : Keys.no]}\n" +
                    $"{Formatter.Bold(loc[Keys.time_to_vote])}: {timeout} {loc[Keys.minute].ToLower(loc.Culture)}(s)\n" +
                    $"\n{Formatter.Bold(loc[Keys.poll_description])}",
                Color = YumikoColors.Primary,
            })
            .AddActionRowComponent(new DiscordSelectComponent(
                $"{ComponentInteractionHandler.PollSelectPrefix}{pollId}",
                loc[Keys.select_an_option],
                options.Select(o => new DiscordSelectComponentOption(o, o)))));

        await Task.Delay(TimeSpan.FromMinutes(timeout));

        pollState.Remove(pollId);
        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(PollEmbeds.Results(poll, anonymous, loc)));
        await ctx.DeleteFollowupAsync(pollMsg.Id);
    }

    [Command("emote")]
    [Description("Shows information about an emote")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task EmoteAsync(
        SlashCommandContext ctx,
        [Parameter("emote")] [Description("The emote")] string emoji)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync();

        if (DiscordEmoji.TryFromName(ctx.Client, $":{emoji}:", out DiscordEmoji? byName))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(byName));
            return;
        }

        if (EmojiHelper.ParseCustom(emoji) is { } custom)
        {
            await ctx.EditResponseAsync(new DiscordEmbedBuilder
            {
                Color = YumikoColors.Primary,
                Title = "Emote",
                ImageUrl = custom.Url,
            }
            .AddField("Id", $"{custom.Id}", true)
            .AddField(loc[Keys.name], custom.Name, true)
            .AddField(loc[Keys.animated], loc[custom.Animated ? Keys.yes : Keys.no], true)
            .AddField(loc[Keys.creation_date], Formatter.Timestamp(custom.CreationTimestamp, TimestampFormat.LongDate)));
            return;
        }

        if (DiscordEmoji.TryFromUnicode(emoji.Trim(), out DiscordEmoji? unicode))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(unicode));
            return;
        }

        await ctx.EditResponseAsync(new DiscordEmbedBuilder
        {
            Color = YumikoColors.Primary,
            Title = "Emote",
            Description = loc.Format(Keys.emote_not_found, emoji),
        });
    }

    [Command("avatar")]
    [Description("Shows an user's avatar")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [RequireGuild]
    public async Task AvatarAsync(
        SlashCommandContext ctx,
        [Parameter("user")] [Description("The user's avatar to retrieve")] DiscordUser? user = null,
        [Parameter("server")] [Description("Get server avatar (for nitro users)")] bool serverAvatar = false,
        [Parameter("secret")] [Description("If you want to see only you the command")] bool ephemeral = true)
    {
        Loc loc = ctx.Loc(localizer);

        DiscordMember member = await ctx.Guild!.GetMemberAsync((user ?? ctx.User).Id, true);

        await ctx.RespondAsync(new DiscordInteractionResponseBuilder
        {
            IsEphemeral = ephemeral,
        }.AddEmbed(new DiscordEmbedBuilder
        {
            Title = loc.Format(Keys.member_avatar, member.DisplayName),
            ImageUrl = serverAvatar ? member.PreferredAvatarUrl() : member.AvatarUrl,
        }));
    }

    [Command("user")]
    [Description("Shows information about an user")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [RequireGuild]
    public async Task UserAsync(
        SlashCommandContext ctx,
        [Parameter("user")] [Description("The user you want to retrieve information")] DiscordUser user,
        [Parameter("secret")] [Description("If you want to see only you the command")] bool ephemeral = false)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.RespondAsync(new DiscordInteractionResponseBuilder
        {
            IsEphemeral = ephemeral,
        }.AddEmbed(new DiscordEmbedBuilder
        {
            Title = loc[Keys.processing],
            Description = $"{loc[Keys.processing_desc]}..",
        }));

        // Sin forzar la recarga el banner y los flags llegan vacíos: no vienen en el objeto cacheado.
        user = await ctx.Client.GetUserAsync(user.Id, true);
        DiscordMember member = await ctx.Guild!.GetMemberAsync(user.Id);

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Title = $"{member.DisplayName} ({user.Username})",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = user.AvatarUrl },
            Color = member.Color.PrimaryColor,
        }
        .AddField(loc[Keys.registered], $"{Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.LongDate)} ({Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime)})", true)
        .AddField(loc[Keys.joined_date], $"{Formatter.Timestamp(member.JoinedAt, TimestampFormat.LongDate)} ({Formatter.Timestamp(member.JoinedAt, TimestampFormat.RelativeTime)})", true)
        .AddField("Bot", loc[user.IsBot ? Keys.yes : Keys.no], true);

        if (user.Flags is not null)
        {
            embed.AddField(loc[Keys.badges], $"{user.Flags}".NormalizeField());
        }

        string roles = string.Join(" ", member.Roles.OrderByDescending(r => r.Position).Select(r => r.Mention));
        if (!string.IsNullOrEmpty(roles))
        {
            embed.AddField(loc[Keys.roles], roles.NormalizeField());
        }

        if (user.BannerUrl is not null)
        {
            embed.WithImageUrl(user.BannerUrl);
        }

        await ctx.EditResponseAsync(embed);
    }

    [Command("waifu")]
    [Description("My love level to a user")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [RequireGuild]
    public async Task WaifuAsync(
        SlashCommandContext ctx,
        [Parameter("user")] [Description("User whose waifu level you want to know")] DiscordUser? user = null,
        [Parameter("real")] [Description("Shows the real percentage, it doesn't change")] bool real = false)
    {
        Loc loc = ctx.Loc(localizer);

        DiscordMember member = await ctx.Guild!.GetMemberAsync((user ?? ctx.User).Id);

        // El "real" siembra con el id del miembro: siempre da el mismo número para la misma persona.
        int level = real ? new Random((int)member.Id).Next(0, 101) : RandomHelper.GetRandomNumber(0, 100);

        (DiscordColor color, string messageKey, string image) = level switch
        {
            < 25 => (DiscordColor.Red, Keys.waifu_level_25, "https://i.imgur.com/BOxbruw.png"),
            < 50 => (DiscordColor.Orange, Keys.waifu_level_50, "https://i.imgur.com/ys2HoiL.jpg"),
            < 75 => (DiscordColor.Yellow, Keys.waifu_level_75, "https://i.imgur.com/h7Ic2rk.jpg"),
            < 100 => (DiscordColor.Green, Keys.waifu_level_99, "https://i.imgur.com/dhXR8mV.png"),
            _ => (DiscordColor.Blue, Keys.waifu_level_100, "https://i.imgur.com/Vk6JMJi.jpg"),
        };

        await ctx.RespondAsync(new DiscordEmbedBuilder
        {
            Color = color,
            Title = real ? "Waifu (REAL)" : "Waifu",
            Description = $"{loc.Format(Keys.my_love_to_user_is, member.DisplayName, level)}\n{loc[messageKey]}",
            ImageUrl = image,
        });
    }

    [Command("love")]
    [Description("Love percentage between two users")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [RequireGuild]
    public async Task LoveAsync(
        SlashCommandContext ctx,
        [Parameter("user1")] [Description("First user")] DiscordUser? user1 = null,
        [Parameter("user2")] [Description("Second user")] DiscordUser? user2 = null,
        [Parameter("real")] [Description("Shows the real percentage, it doesn't change")] bool real = false)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync();

        // Con un solo usuario se compara al invocante contra él; con ninguno, al invocante consigo mismo.
        DiscordUser first = user1 ?? ctx.User;
        DiscordUser? second = user2;

        if (second is null && first.Id != ctx.User.Id)
        {
            second = first;
            first = ctx.User;
        }

        bool selfLove = second is null || first.Id == second.Id;

        DiscordMember member1 = await ctx.Guild!.GetMemberAsync(first.Id);
        DiscordWebhookBuilder builder = new();
        string title;
        string imageUrl;

        if (selfLove)
        {
            title = loc.Format(Keys.user_self_love, member1.DisplayName);
            imageUrl = first.GetAvatarUrl(MediaFormat.Png, 128);
        }
        else
        {
            DiscordMember member2 = await ctx.Guild.GetMemberAsync(second!.Id);
            title = loc.Format(Keys.love_between, member1.DisplayName, member2.DisplayName);

            byte[] image = await BuildLoveImageAsync(
                first.GetAvatarUrl(MediaFormat.Png, 512),
                second.GetAvatarUrl(MediaFormat.Png, 512));

            imageUrl = Formatter.AttachedImageUrl("imageLove.png");
            builder.AddFile("imageLove.png", image.ToMemoryStream());
        }

        // El auto-amor siembra sumando el id consigo mismo, distinto de la semilla que usa /waifu.
        int percentage = real
            ? LoveMeter.RealPercentage(first.Id, second?.Id ?? first.Id)
            : RandomHelper.GetRandomNumber(0, 100);

        string description = $"{Formatter.Bold($"{percentage}%")} [{LoveMeter.Bar(percentage)}]\n\n";

        if (!selfLove)
        {
            description += loc[LoveMessageKey(percentage)];
        }

        builder.AddEmbed(new DiscordEmbedBuilder
        {
            Title = real ? $"{title} (REAL)" : title,
            Description = description,
            ImageUrl = imageUrl,
            Color = YumikoColors.Primary,
        });

        await ctx.EditResponseAsync(builder);
    }

    private static string LoveMessageKey(int percentage) => percentage switch
    {
        0 => Keys.love_0,
        <= 10 => Keys.love_10,
        <= 25 => Keys.love_25,
        <= 50 => Keys.love_50,
        <= 75 => Keys.love_75,
        <= 90 => Keys.love_90,
        < 100 => Keys.love_99,
        _ => Keys.love_100,
    };

    private async Task<byte[]> BuildLoveImageAsync(string avatar1, string avatar2)
    {
        HttpClient client = httpClientFactory.CreateClient();
        byte[] bytes1 = await client.GetByteArrayAsync(avatar1);
        byte[] bytes2 = await client.GetByteArrayAsync(avatar2);

        byte[] merged = ImageHelper.MergeImage(bytes1, bytes2, 1024, 512);
        return ImageHelper.OverlapImage(merged, await File.ReadAllBytesAsync(ImagePath("frame-love.png")), 1024, 512);
    }
}
