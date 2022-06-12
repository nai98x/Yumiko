namespace Yumiko.Datatypes.Firebase
{
    using Google.Cloud.Firestore;
    using System.Diagnostics.CodeAnalysis;

    [FirestoreData]
    public class DtLeaderboardHoL
    {
        [FirestoreProperty]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declared attribute in Firebase")]
        public long user_id { get; set; }

        [FirestoreProperty]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declared attribute in Firebase")]
        public int puntuacion { get; set; }
    }
}
