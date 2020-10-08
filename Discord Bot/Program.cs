namespace Discord_Bot
{
    class Program
    {
        static void Main()
        {
            var yumiko = new Yumiko();
            yumiko.RunAsync().GetAwaiter().GetResult();
        }
    }
}