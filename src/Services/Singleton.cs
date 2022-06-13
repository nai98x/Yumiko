namespace Yumiko.Services
{
    using System.Collections.Generic;

    public class Singleton
    {
        private static readonly object syncLock = new();
        private static Singleton? instance;

        private Singleton()
        {
            CurrentTrivias = new();
        }

        public List<Quiz> CurrentTrivias { get; set; }

        public static Singleton GetInstance()
        {
            if (instance == null)
            {
                lock (syncLock)
                {
                    if (instance == null)
                    {
                        instance = new Singleton();
                    }
                }
            }

            return instance;
        }

        public Quiz? GetCurrentTrivia(ulong guildId, ulong channelId)
        {
            Quiz? trivia = CurrentTrivias.Find(x => x.GuildId == guildId && x.ChannelId == channelId);
            return trivia;
        }

        public void RemoveCurrentTrivia(ulong guildId, ulong channelId)
        {
            Quiz? trivia = CurrentTrivias.Find(x => x.GuildId == guildId && x.ChannelId == channelId);
            if (trivia != null)
            {
                CurrentTrivias.Remove(trivia);
            }
        }

        public void UpdateCurrentRoundTrivia(ulong guildId, ulong channelId, QuizRound updatedRound)
        {
            Quiz? trivia = CurrentTrivias.Find(x => x.GuildId == guildId && x.ChannelId == channelId);
            if (trivia != null)
            {
                trivia.CurrentRound = updatedRound;
            }
        }
    }
}
