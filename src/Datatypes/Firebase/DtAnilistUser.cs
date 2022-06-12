namespace Yumiko.Datatypes.Firebase
{
    using Google.Cloud.Firestore;

    [FirestoreData]
    public class DtAnilistUser
    {
        [FirestoreProperty]
        public int AnilistId { get; set; }

        [FirestoreProperty]
        public long UserId { get; set; }
    }
}
