using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Yumiko.Application.Games;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Games;

public sealed class TicTacToeGameRunner(InteractivityExtension interactivity)
{
    private static readonly TimeSpan TurnTimeout = TimeSpan.FromSeconds(30);

    public async Task PlayAsync(SlashCommandContext ctx, DiscordUser player2, Loc loc)
    {
        DiscordUser player1 = ctx.User;
        bool firstPlayerTurn = Random.Shared.NextDouble() >= 0.5;

        List<DiscordButtonComponent> buttons = TicTacToeBoard.Initial();

        DiscordEmbedBuilder embed = new()
        {
            Title = loc[Keys.tictactoe],
            Color = firstPlayerTurn ? DiscordColor.Green : DiscordColor.Red,
            Description = loc.Format(Keys.player_turn, (firstPlayerTurn ? player1 : player2).Mention),
        };

        DiscordMessage message = await ctx.FollowupAsync(Builder(buttons).AddEmbed(embed));

        while (true)
        {
            DiscordUser currentPlayer = firstPlayerTurn ? player1 : player2;

            InteractivityResult<ComponentInteractionCreatedEventArgs> move =
                await interactivity.WaitForButtonAsync(message, currentPlayer, TurnTimeout);

            if (move.TimedOut)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc[Keys.game_cancelled],
                    Description = loc[Keys.no_click_button],
                    Color = DiscordColor.Red,
                }));
                return;
            }

            buttons = TicTacToeBoard.Mark(buttons, move.Result.Id, firstPlayerTurn);
            (bool finished, TicTacToeCell winningCell) = TicTacToe.Result(TicTacToeBoard.Read(buttons));

            if (!finished)
            {
                firstPlayerTurn = !firstPlayerTurn;
                embed.Description = loc.Format(Keys.player_turn, (firstPlayerTurn ? player1 : player2).Mention);
                embed.Color = firstPlayerTurn ? DiscordColor.Green : DiscordColor.Red;
                message = await ctx.EditFollowupAsync(message.Id, EditBuilder(buttons).AddEmbed(embed));
                continue;
            }

            DiscordUser? winner = winningCell switch
            {
                TicTacToeCell.Player1 => player1,
                TicTacToeCell.Player2 => player2,
                _ => null,
            };

            if (winner is null)
            {
                embed.Title = loc[Keys.tie];
                embed.Description = loc[Keys.tie_desc];
                embed.Color = YumikoColors.Primary;
            }
            else
            {
                embed.Title = loc[Keys.we_have_a_winner];
                embed.Description = loc.Format(Keys.user_won_the_game, winner.Mention);
                embed.Color = firstPlayerTurn ? DiscordColor.Green : DiscordColor.Red;
            }

            await ctx.EditFollowupAsync(message.Id, EditBuilder(TicTacToeBoard.DisableAll(buttons)).AddEmbed(embed));
            return;
        }
    }

    private static DiscordFollowupMessageBuilder Builder(List<DiscordButtonComponent> buttons)
    {
        DiscordFollowupMessageBuilder builder = new();

        foreach (DiscordButtonComponent[] row in buttons.Chunk(3))
        {
            builder.AddActionRowComponent(row);
        }

        return builder;
    }

    private static DiscordWebhookBuilder EditBuilder(List<DiscordButtonComponent> buttons)
    {
        DiscordWebhookBuilder builder = new();

        foreach (DiscordButtonComponent[] row in buttons.Chunk(3))
        {
            builder.AddActionRowComponent(row);
        }

        return builder;
    }
}
