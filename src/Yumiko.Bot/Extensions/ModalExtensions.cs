using DSharpPlus.EventArgs;

namespace Yumiko.Bot.Extensions;

public static class ModalExtensions
{
    /// <summary>
    /// Valor de un campo de texto del modal. <c>Values</c> es un diccionario de submissions de
    /// cualquier tipo de componente, así que hay que filtrar por el de texto.
    /// </summary>
    public static string TextOf(this ModalSubmittedEventArgs args, string customId) =>
        args.Values.TryGetValue(customId, out IModalSubmission? submission) && submission is TextInputModalSubmission text
            ? text.Value ?? string.Empty
            : string.Empty;
}
