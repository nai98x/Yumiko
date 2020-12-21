using System;

namespace Discord_Bot
{
    public class UserCumple
    {
        public long Id { get; set; }
        public long guild_id { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime BirthdayActual { get; set; }
        public bool? MostrarYear { get; set; }
    }
}
