﻿using DSharpPlus;
using DSharpPlus.Entities;
using System;

namespace Discord_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
