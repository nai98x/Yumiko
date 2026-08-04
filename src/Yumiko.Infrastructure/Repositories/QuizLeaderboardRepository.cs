using Google.Cloud.Firestore;
using Yumiko.Application.Games;
using Yumiko.Infrastructure.Firebase;
using Yumiko.Infrastructure.Firebase.Documents;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class QuizLeaderboardRepository(FirebaseService firebase) : IQuizLeaderboardRepository
{
    private CollectionReference Difficulties(ulong guildId, string game) =>
        firebase.GetDb()
            .Collection("Estadisticas").Document($"{guildId}")
            .Collection("Juegos").Document(game)
            .Collection("Dificultad");

    private CollectionReference Users(ulong guildId, string game, string difficulty) =>
        Difficulties(guildId, game).Document(difficulty).Collection("Usuarios");

    public Task<List<GameStats>> GetLeaderboardAsync(ulong guildId, Gamemode gamemode, Difficulty difficulty, int limit) =>
        LeaderboardAsync(guildId, gamemode.ToSpanish(), difficulty.ToSpanish(), limit);

    public Task<List<GameStats>> GetGenreLeaderboardAsync(ulong guildId, string genre, int limit) =>
        LeaderboardAsync(guildId, Gamemode.Genres.ToSpanish(), genre, limit);

    private async Task<List<GameStats>> LeaderboardAsync(ulong guildId, string game, string difficulty, int limit)
    {
        Query query = Users(guildId, game, difficulty)
            .OrderByDescending("porcentajeAciertos")
            .OrderByDescending("rondasTotales")
            .Limit(limit);

        QuerySnapshot snap = await query.GetSnapshotAsync();

        return [.. snap.Documents.Select(d => Map(d.ConvertTo<QuizStatsDocument>(), difficultyName: null))];
    }

    public async Task AddResultAsync(ulong guildId, ulong userId, Gamemode gamemode, Difficulty difficulty, int correctRounds, int totalRounds)
    {
        DocumentReference doc = Users(guildId, gamemode.ToSpanish(), difficulty.ToSpanish())
            .Document($"{userId}");

        DocumentSnapshot snap = await doc.GetSnapshotAsync();

        if (snap.Exists)
        {
            QuizStatsDocument record = snap.ConvertTo<QuizStatsDocument>();
            record.partidasJugadas++;
            record.rondasAcertadas += correctRounds;
            record.rondasTotales += totalRounds;

            await doc.UpdateAsync(new Dictionary<string, object>
            {
                { "user_id", record.user_id },
                { "partidasJugadas", record.partidasJugadas },
                { "rondasAcertadas", record.rondasAcertadas },
                { "rondasTotales", record.rondasTotales },
                // División entera a propósito: es el valor con el que está guardado todo y define el orden
        // del leaderboard.
                { "porcentajeAciertos", record.rondasAcertadas * 100 / record.rondasTotales },
            });

            return;
        }

        await doc.SetAsync(new Dictionary<string, object>
        {
            { "user_id", (long)userId },
            { "partidasJugadas", 1 },
            { "rondasAcertadas", correctRounds },
            { "rondasTotales", totalRounds },
            { "porcentajeAciertos", correctRounds * 100 / totalRounds },
        });
    }

    public async Task DeleteStatsAsync(ulong guildId, ulong userId, Gamemode gamemode)
    {
        string game = gamemode.ToSpanish();

        foreach (Difficulty difficulty in System.Enum.GetValues<Difficulty>())
        {
            DocumentReference doc = Users(guildId, game, difficulty.ToSpanish()).Document($"{userId}");
            DocumentSnapshot snap = await doc.GetSnapshotAsync();

            if (snap.Exists)
            {
                await doc.DeleteAsync();
            }
        }
    }

    public async Task<List<GameStatsUser>> GetStatsUserAsync(ulong guildId, ulong userId)
    {
        List<GameStatsUser> ret = [];

        foreach (Gamemode gamemode in System.Enum.GetValues<Gamemode>())
        {
            string game = gamemode.ToSpanish();
            GameStatsUser gameStats = new() { Gamemode = gamemode };

            foreach (Difficulty difficulty in System.Enum.GetValues<Difficulty>())
            {
                DocumentReference doc = Users(guildId, game, difficulty.ToSpanish()).Document($"{userId}");
                DocumentSnapshot snap = await doc.GetSnapshotAsync();

                if (snap.Exists)
                {
                    gameStats.Stats.Add(Map(snap.ConvertTo<QuizStatsDocument>(), System.Enum.GetName(difficulty)));
                }
            }

            ret.Add(gameStats);
        }

        return ret;
    }

    public async Task<List<GameStats>> GetGenreStatsUserAsync(ulong guildId, ulong userId)
    {
        List<GameStats> ret = [];
        string game = Gamemode.Genres.ToSpanish();

        // En el modo géneros los documentos de "Dificultad" son nombres de género, así que hay que
        // enumerarlos en vez de recorrer el enum.
        await foreach (DocumentReference genre in Difficulties(guildId, game).ListDocumentsAsync())
        {
            DocumentSnapshot snap = await Users(guildId, game, genre.Id).Document($"{userId}").GetSnapshotAsync();

            if (snap.Exists)
            {
                ret.Add(Map(snap.ConvertTo<QuizStatsDocument>(), genre.Id));
            }
        }

        return ret;
    }

    private static GameStats Map(QuizStatsDocument doc, string? difficultyName) => new()
    {
        UserId = doc.user_id,
        GamesPlayed = doc.partidasJugadas,
        TotalRounds = doc.rondasTotales,
        CorrectRounds = doc.rondasAcertadas,
        AccuracyPercentage = doc.porcentajeAciertos,
        DifficultyName = difficultyName,
    };
}
