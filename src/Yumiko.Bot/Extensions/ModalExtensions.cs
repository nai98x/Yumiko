using DSharpPlus.EventArgs;

namespace Yumiko.Bot.Extensions;

public static class ModalExtensions
{
    /// <summary>
    /// Value of a text field of the modal. <c>Values</c> is a dictionary of submissions of
    /// any component type, so it has to be filtered by the text one.
    /// </summary>
    public static string TextOf(this ModalSubmittedEventArgs args, string customId) =>
        args.Values.TryGetValue(customId, out IModalSubmission? submission) && submission is TextInputModalSubmission text
            ? text.Value ?? string.Empty
            : string.Empty;
}
