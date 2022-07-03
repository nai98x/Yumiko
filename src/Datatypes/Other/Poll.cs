namespace Yumiko.Datatypes
{
    public class Poll
    {
        public string Id { get; set; }
        public List<PollOption> Options { get; set; } = null!;
    }
}
