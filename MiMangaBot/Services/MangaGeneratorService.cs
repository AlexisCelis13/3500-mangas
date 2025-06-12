using Bogus;
using Google.Cloud.Firestore;
using MiMangaBot.Domain.Models;
using Microsoft.Extensions.Logging;

namespace MiMangaBot.Services;

public class MangaGeneratorService
{
    private const int MAX_ATTEMPTS_MULTIPLIER = 2;
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<MangaGeneratorService> _logger;
    private readonly Faker<Manga> _mangaFaker;
    private readonly HashSet<string> _generatedTitles = new();

    public MangaGeneratorService(FirestoreDb firestoreDb, ILogger<MangaGeneratorService> logger)
    {
        _firestoreDb = firestoreDb;
        _logger = logger;

        var generos = new[] { "Shonen", "Seinen", "Shojo", "Josei", "Mecha", "Fantasy", "Romance", "Comedy", "Drama", "Action", "Horror", "Misterio", "Deportes", "Musical", "Histórico" };
        var editoriales = new[] { "Shueisha", "Kodansha", "Shogakukan", "Square Enix", "Kadokawa", "Hakusensha", "Akita Shoten", "Futabasha", "Lezhin Comics", "Yen Press" };
        var estados = new[] { "En emisión", "Finalizado", "Pausado", "Cancelado" };

        _mangaFaker = new Faker<Manga>("es")
            .RuleFor(m => m.Titulo, f => GenerateUniqueTitle(f))
            .RuleFor(m => m.Autor, f => f.Name.FullName())
            .RuleFor(m => m.Genero, f => f.PickRandom(generos))
            .RuleFor(m => m.AnioPublicacion, f => f.Random.Int(1960, 2024))
            .RuleFor(m => m.Volumenes, f => f.Random.Int(1, 100))
            .RuleFor(m => m.EnPublicacion, f => f.Random.Bool())
            .RuleFor(m => m.Sinopsis, f => f.Lorem.Paragraphs(2))
            .RuleFor(m => m.Calificacion, f => Math.Round(f.Random.Double(1, 10), 1))
            .RuleFor(m => m.NumeroCapitulos, f => f.Random.Int(1, 500))
            .RuleFor(m => m.Editorial, f => f.PickRandom(editoriales))
            .RuleFor(m => m.Estado, f => f.PickRandom(estados))
            .RuleFor(m => m.FechaCreacion, f => f.Date.Past().ToUniversalTime())
            .RuleFor(m => m.FechaActualizacion, f => f.Date.Recent().ToUniversalTime());
    }

    private string GenerateUniqueTitle(Faker f)
    {
        string title;
        do
        {
            var words = f.Lorem.Words(f.Random.Int(2, 4));
            title = string.Join(" ", words);
        } while (!_generatedTitles.Add(title.ToLower().Trim()));

        return title;
    }

    public async Task<List<Manga>> GenerateAndStoreMangasAsync(int count)
    {
        var mangas = new List<Manga>();
        var batch = _firestoreDb.StartBatch();
        var collection = _firestoreDb.Collection("mangas");
        var existingTitles = await GetExistingTitlesAsync();

        int generated = 0;
        int attempts = 0;
        int maxAttempts = count * MAX_ATTEMPTS_MULTIPLIER; // Permitimos algunos intentos extra para manejar colisiones

        while (generated < count && attempts < maxAttempts)
        {
            attempts++;
            var manga = _mangaFaker.Generate();

            // Verificar si el título ya existe en la base de datos
            if (existingTitles.Contains(manga.TituloNormalizado))
            {
                _logger.LogWarning($"Título duplicado encontrado: {manga.Titulo}");
                continue;
            }

            mangas.Add(manga);
            batch.Set(collection.Document(manga.Id), manga);
            generated++;

            // Commit cada 500 mangas para evitar lotes demasiado grandes
            if (generated % 500 == 0)
            {
                await batch.CommitAsync();
                batch = _firestoreDb.StartBatch();
                _logger.LogInformation($"Generados y almacenados {generated} mangas");
            }
        }

        // Commit final si quedan mangas pendientes
        if (generated % 500 != 0)
        {
            await batch.CommitAsync();
        }

        _logger.LogInformation($"Generación completada. Total de mangas generados: {generated} de {count} solicitados");
        return mangas;
    }

    private async Task<HashSet<string>> GetExistingTitlesAsync()
    {
        var titles = new HashSet<string>();
        var snapshot = await _firestoreDb.Collection("mangas").GetSnapshotAsync();
        
        foreach (var doc in snapshot.Documents)
        {
            var manga = doc.ConvertTo<Manga>();
            titles.Add(manga.TituloNormalizado);
        }

        return titles;
    }

    public async Task<List<Manga>> GetAllMangasAsync()
    {
        var mangas = new List<Manga>();
        var snapshot = await _firestoreDb.Collection("mangas").GetSnapshotAsync();
        
        foreach (var doc in snapshot.Documents)
        {
            var manga = doc.ConvertTo<Manga>();
            mangas.Add(manga);
        }

        _logger.LogInformation($"Recuperados {mangas.Count} mangas de Firestore.");
        return mangas;
    }

    public async Task<Dictionary<string, List<Manga>>> GetDuplicateMangasAsync()
    {
        _logger.LogInformation("Buscando mangas duplicados.");
        var allMangas = await GetAllMangasAsync();

        var duplicates = allMangas
            .GroupBy(m => m.TituloNormalizado)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key!, g => g.ToList());

        _logger.LogInformation($"Encontrados {duplicates.Count} grupos de mangas duplicados.");
        return duplicates;
    }

    public async Task<bool> DeleteMangaAsync(string id)
    {
        try
        {
            var docRef = _firestoreDb.Collection("mangas").Document(id);
            await docRef.DeleteAsync();
            _logger.LogInformation($"Manga con ID {id} eliminado exitosamente de Firestore.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar manga con ID {id} de Firestore.");
            return false;
        }
    }
} 