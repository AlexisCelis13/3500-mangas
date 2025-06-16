using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using MiMangaBot.Domain.Models;
using MiMangaBot.Domain.Repositories;

namespace MiMangaBot.Infrastructure.Repositories;

public class FirestoreMangaRepository : IMangaRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<FirestoreMangaRepository> _logger;
    private const string COLLECTION_NAME = "mangas";

    public FirestoreMangaRepository(FirestoreDb firestoreDb, ILogger<FirestoreMangaRepository> logger)
    {
        _firestoreDb = firestoreDb;
        _logger = logger;
    }

    public async Task<List<Manga>> GetAllAsync()
    {
        var mangas = new List<Manga>();
        var snapshot = await _firestoreDb.Collection(COLLECTION_NAME).GetSnapshotAsync();
        
        foreach (var doc in snapshot.Documents)
        {
            var manga = doc.ConvertTo<Manga>();
            mangas.Add(manga);
        }

        _logger.LogInformation($"Recuperados {mangas.Count} mangas de Firestore.");
        return mangas;
    }

    public async Task<Manga?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists)
        {
            return null;
        }

        return snapshot.ConvertTo<Manga>();
    }

    public async Task<Manga> AddAsync(Manga manga)
    {
        var docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(manga.Id);
        await docRef.SetAsync(manga);
        _logger.LogInformation($"Manga agregado exitosamente con ID: {manga.Id}");
        return manga;
    }

    public async Task<bool> UpdateAsync(Manga manga)
    {
        try
        {
            var docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(manga.Id);
            await docRef.SetAsync(manga, SetOptions.MergeAll);
            _logger.LogInformation($"Manga actualizado exitosamente con ID: {manga.Id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar manga con ID: {manga.Id}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(id);
            await docRef.DeleteAsync();
            _logger.LogInformation($"Manga eliminado exitosamente con ID: {id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar manga con ID: {id}");
            return false;
        }
    }

    public async Task<bool> ExistsByTitleAsync(string title)
    {
        var normalizedTitle = title.ToLower().Trim();
        var query = _firestoreDb.Collection(COLLECTION_NAME)
            .WhereEqualTo("TituloNormalizado", normalizedTitle);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Any();
    }

    public async Task<Dictionary<string, List<Manga>>> GetDuplicatesAsync()
    {
        var allMangas = await GetAllAsync();
        return allMangas
            .GroupBy(m => m.TituloNormalizado)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<List<Manga>> AddRangeAsync(IEnumerable<Manga> mangas)
    {
        var batch = _firestoreDb.StartBatch();
        var addedMangas = new List<Manga>();
        var collection = _firestoreDb.Collection(COLLECTION_NAME);
        int count = 0;

        foreach (var manga in mangas)
        {
            batch.Set(collection.Document(manga.Id), manga);
            addedMangas.Add(manga);
            count++;

            // Commit cada 500 mangas para evitar lotes demasiado grandes
            if (count % 500 == 0)
            {
                await batch.CommitAsync();
                batch = _firestoreDb.StartBatch();
                _logger.LogInformation($"Agregados {count} mangas al lote");
            }
        }

        // Commit final si quedan mangas pendientes
        if (count % 500 != 0)
        {
            await batch.CommitAsync();
        }

        _logger.LogInformation($"Total de mangas agregados: {count}");
        return addedMangas;
    }
} 