using Google.Cloud.Firestore;

namespace YumikoBot.DAL
{
    [FirestoreData]
    public class LeaderboardFirebase
    {
        [FirestoreProperty]
        public long user_id { get; set; }

        [FirestoreProperty]
        public int partidasJugadas { get; set; }

        [FirestoreProperty]
        public int rondasAcertadas { get; set; }

        [FirestoreProperty]
        public int rondasTotales { get; set; }
    }
}
