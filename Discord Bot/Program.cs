using DSharpPlus;
using DSharpPlus.Entities;
using System;

namespace Discord_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            //string strCmdText;
            //strCmdText = "/C java -jar Lavalink.jar";
            //System.Diagnostics.Process.Start("CMD.exe", strCmdText);

            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
