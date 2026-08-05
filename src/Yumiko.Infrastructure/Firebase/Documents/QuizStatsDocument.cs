using System.Diagnostics.CodeAnalysis;
using Google.Cloud.Firestore;

namespace Yumiko.Infrastructure.Firebase.Documents;

// The field names replicate exactly what is already stored in Firestore.
[FirestoreData]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Firestore field names")]
internal class QuizStatsDocument
{
    [FirestoreProperty]
    public long user_id { get; set; }

    [FirestoreProperty]
    public int partidasJugadas { get; set; }

    [FirestoreProperty]
    public int rondasAcertadas { get; set; }

    [FirestoreProperty]
    public int rondasTotales { get; set; }

    [FirestoreProperty]
    public int porcentajeAciertos { get; set; }
}
