namespace Yumiko.Datatypes
{
    public class Poll
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public List<PollOption> Options { get; set; } = null!;
    }
}
