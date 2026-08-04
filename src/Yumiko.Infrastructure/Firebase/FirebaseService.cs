using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace Yumiko.Infrastructure.Firebase;

public class FirebaseService
{
    private const string CredentialFile = "firebase-yumiko.json";

    private readonly string _credentialsDir;
    private readonly Lazy<FirestoreDb> _db;

    public FirebaseService(string credentialsDir)
    {
        _credentialsDir = credentialsDir;
        _db = new Lazy<FirestoreDb>(Create);
    }

    public FirestoreDb GetDb() => _db.Value;

    private FirestoreDb Create()
    {
        string path = Path.Combine(_credentialsDir, CredentialFile);
        string jsonCredentials = File.ReadAllText(path);

        FirestoreDbBuilder builder = new()
        {
            ProjectId = ProjectId(jsonCredentials, path),
            GoogleCredential = CredentialFactory.FromJson<ServiceAccountCredential>(jsonCredentials).ToGoogleCredential(),
        };

        return builder.Build();
    }

    private static string ProjectId(string jsonCredentials, string path) =>
        JsonDocument.Parse(jsonCredentials).RootElement.GetProperty("project_id").GetString()
        ?? throw new InvalidOperationException($"'project_id' no encontrado en {path}");
}
