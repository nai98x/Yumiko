namespace Yumiko.Datatypes
{
    using System;
    using System.Collections.Generic;

    public class QuizRound
    {
        public string Match { get; set; } = string.Empty;

        public double TimeoutCurrent { get; set; } = 0;

        public bool Guessed { get; set; } = false;

        public DiscordUser? Guesser { get; set; }

        public DateTimeOffset GuessTime { get; set; }
    }
}
