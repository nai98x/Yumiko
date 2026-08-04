using Google.Cloud.Firestore;

namespace Yumiko.Infrastructure.Firebase.Documents;

[FirestoreData]
internal class AnilistUserDocument
{
    [FirestoreProperty]
    public int AnilistId { get; set; }

    [FirestoreProperty]
    public long UserId { get; set; }
}
