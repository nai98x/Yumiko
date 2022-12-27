using System.ComponentModel;
using System.Globalization;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Extensions.Formatting;
using Remora.Results;
using Yumiko.Shared.Resources.Translations;

namespace Yumiko.Bot.Commands;

public class Interact : CommandGroup
{
    private readonly IInteractionContext _context;
    private readonly IDiscordRestChannelAPI _chennels;
    private readonly IDiscordRestInteractionAPI _interactions;

    public Interact(IInteractionContext context, IDiscordRestChannelAPI chennels, IDiscordRestInteractionAPI interactions)
    {
        _context = context;
        _chennels = chennels;
        _interactions = interactions;
    }

    [Command("Say")]
    [Description("Replicates a text")]
    public async Task<IResult> SayAsync([Description("The text you want to replicate")] string text)
    {
        //await _interactions.DeferAsync(_context);
        await _interactions.DeleteOriginalInteractionResponseAsync(_context.Interaction.ApplicationID, _context.Interaction.Token);

        return await _chennels.CreateMessageAsync(_context.Interaction.ChannelID.Value, content: text);
    }

    [Command("question")]
    [Description("Responds with yes or no")]
    public async Task<IResult> QuestionAsync([Description("The question you want to ask")] string question)
    {
        Random rnd = new();
        int random = rnd.Next(2);
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(_context.Interaction.Locale.Value);

        Embed embed = random switch
        {
            0 => new Embed
            {
                Colour = System.Drawing.Color.Red,
                Title = translations.yes_or_no,
                Description = $"{Markdown.Bold(translations.question)}: {question}\n{Markdown.Bold(translations.answer)}: {translations.no.ToUpper()}",
            },
            _ => new Embed
            {
                Colour = System.Drawing.Color.Green,
                Title = translations.yes_or_no,
                Description = $"{Markdown.Bold(translations.question)}: {question}\n{Markdown.Bold(translations.answer)}: {translations.yes.ToUpper()}",
            },
        };

        return await _interactions.EditOriginalInteractionResponseAsync(
            _context.Interaction.ApplicationID, 
            _context.Interaction.Token, 
            embeds: new[] { embed }
        ); 
    }

    [Command("choose")]
    [Description("Choose from multiple options separated by commas")]
    public async Task<IResult> ChooseAsync([Description("The question you want to ask")] string question, [Description("Comma Separated Options")] string options)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(_context.Interaction.Locale.Value);

        List<string> opciones = options.Split(',').ToList();
        int random = Shared.Common.GetRandomNumber(0, opciones.Count - 1);
        string optionsR = $"{Markdown.Bold(translations.options)}:";
        foreach (string msj in opciones)
        {
            optionsR += "\n   - " + msj;
        }

        var embed = new Embed
        {
            Colour = Shared.Constants.YumikoColor,
            Title = translations.question,
            Description = $"{Markdown.Bold(question)}\n\n{optionsR}\n\n{Markdown.Bold(translations.answer)}: {opciones[random]}",
        };

        return await _interactions.EditOriginalInteractionResponseAsync(
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            embeds: new[] { embed }
        );

    }
}
