namespace Yumiko.Datatypes
{
    public class PollOption
    {
        public string Name { get; set; } = null!;
        public List<ulong> Voters { get; set; } = new();
        public int VotesCount => Voters.Count;
    }
}
