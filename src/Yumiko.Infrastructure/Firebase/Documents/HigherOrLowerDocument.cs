using System.Diagnostics.CodeAnalysis;
using Google.Cloud.Firestore;

namespace Yumiko.Infrastructure.Firebase.Documents;

[FirestoreData]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Firestore field names")]
internal class HigherOrLowerDocument
{
    [FirestoreProperty]
    public long user_id { get; set; }

    [FirestoreProperty]
    public int puntuacion { get; set; }
}
