using System.Diagnostics.CodeAnalysis;
using Google.Cloud.Firestore;

namespace Yumiko.Infrastructure.Firebase.Documents;

// Los nombres de campo replican exactamente lo que ya está guardado en Firestore.
[FirestoreData]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nombres de campo de Firestore")]
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
