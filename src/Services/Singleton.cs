namespace Yumiko.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Yumiko.Datatypes;

    public class Singleton
    {
        private static readonly object syncLock = new();
        private static Singleton? instance;

        private Singleton()
        {
            CurrentTrivias = new();
            CurrentPolls = new();
            CommandsUsed = new();
        }

        private List<Trivia> CurrentTrivias { get; set; }
        private List<Poll> CurrentPolls { get; set; }
        private List<CommandUse> CommandsUsed { get; set; }
        private List<Anime> MediaList { get; set; } = new();
        private bool BotReady { get; set; } = false;

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

        public void SetBotReady()
        {
            BotReady = true;
        }

        public bool IsBotReady() => BotReady;

        public void AddTrivia(Trivia trivia)
        {
            CurrentTrivias.Add(trivia);
        }

        public Trivia? GetCurrentTrivia(ulong guildId, ulong channelId)
        {
            Trivia? trivia = CurrentTrivias.Find(x => x.GuildId == guildId && x.ChannelId == channelId);
            return trivia;
        }

        public void RemoveCurrentTrivia(ulong guildId, ulong channelId)
        {
            Trivia? trivia = CurrentTrivias.Find(x => x.GuildId == guildId && x.ChannelId == channelId);
            if (trivia != null)
            {
                CurrentTrivias.Remove(trivia);
            }
        }

        public void UpdateCurrentRoundTrivia(ulong guildId, ulong channelId, QuizRound updatedRound)
        {
            Trivia? trivia = CurrentTrivias.Find(x => x.GuildId == guildId && x.ChannelId == channelId);
            if (trivia != null)
            {
                trivia.CurrentRound = updatedRound;
            }
        }

        public void AddPoll(Poll poll)
        {
            CurrentPolls.Add(poll);
        }

        public Poll? GetCurrentPoll(string id)
        {
            Poll? poll = CurrentPolls.Find(x => x.Id == id);
            return poll;
        }

        public void RemoveCurrentPoll(string id)
        {
            Poll? poll = CurrentPolls.Find(x => x.Id == id);
            if (poll != null)
            {
                CurrentPolls.Remove(poll);
            }
        }

        public List<CommandUse> GetUsedCommands()
        {
            return CommandsUsed;
        }

        public void UpdateCommandUsed(string commandName)
        {
            CommandUse? commandUsed = CommandsUsed.Find(x => x.CommandName == commandName);
            if (commandUsed != null)
            {
                commandUsed.Uses++;
            }
            else
            {
                CommandsUsed.Add(new()
                {
                    CommandName = commandName,
                    Uses = 1
                });
            }
        }

        public List<Anime> GetCachedMedia()
        {
            return MediaList;
        }

        public async Task UpdateCachedMediaAsync()
        {
            MediaList.Clear();

            var settings = new GameSettings
            {
                IterIni = 1,
                IterFin = 36,
            };

            MediaList = await GameServices.GetMediaForCacheAsync(MediaType.ANIME, settings);
        }
    }
}
