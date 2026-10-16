
namespace Yumiko.Model.Entities.Anilist
{
    public class MediaUserList
    {
        public string Name { get; set; } = null!;

        public List<MediaUserEntry>? Entries { get; set; }
    }
}
