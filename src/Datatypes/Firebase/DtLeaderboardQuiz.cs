namespace Yumiko.Datatypes.Firebase
{
    using Google.Cloud.Firestore;
    using System.Diagnostics.CodeAnalysis;

    [FirestoreData]
    public class DtLeaderboardQuiz
    {
        [FirestoreProperty]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declared attribute in Firebase")]
        public long user_id { get; set; }

        [FirestoreProperty]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declared attribute in Firebase")]
        public int partidasJugadas { get; set; }

        [FirestoreProperty]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declared attribute in Firebase")]
        public int rondasAcertadas { get; set; }

        [FirestoreProperty]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declared attribute in Firebase")]
        public int rondasTotales { get; set; }

        [FirestoreProperty]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declared attribute in Firebase")]
        public int porcentajeAciertos { get; set; }
    }
}
