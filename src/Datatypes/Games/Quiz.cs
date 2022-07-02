namespace Yumiko.Datatypes
{
    public class Quiz
    {
        public string? Title { get; set; }

        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }

        public double TimeoutTotal { get; set; }

        public QuizRound CurrentRound { get; set; } = new QuizRound();

        public DiscordUser? CreatedBy { get; set; }

        public bool Canceled { get; set; } = false;
    }
}
