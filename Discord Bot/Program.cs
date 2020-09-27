using DSharpPlus;
using DSharpPlus.Entities;
using System;

namespace Discord_Bot
{
    class Program
    {
        static void Main()
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
