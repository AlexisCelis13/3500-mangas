using Bogus;
using MiMangaBot.Domain.Models;
using MiMangaBot.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MiMangaBot.Services;

public class MangaGeneratorService
{
    private const int MAX_ATTEMPTS_MULTIPLIER = 2;
    private readonly IMangaRepository _mangaRepository;
    private readonly ILogger<MangaGeneratorService> _logger;
    private readonly Faker<Manga> _mangaFaker;
    private readonly HashSet<string> _generatedTitles = new();

    public MangaGeneratorService(IMangaRepository mangaRepository, ILogger<MangaGeneratorService> logger)
    {
        _mangaRepository = mangaRepository;
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
        var existingTitles = await GetExistingTitlesAsync();

        int generated = 0;
        int attempts = 0;
        int maxAttempts = count * MAX_ATTEMPTS_MULTIPLIER;

        while (generated < count && attempts < maxAttempts)
        {
            attempts++;
            var manga = _mangaFaker.Generate();

            if (await _mangaRepository.ExistsByTitleAsync(manga.Titulo))
            {
                _logger.LogWarning($"Título duplicado encontrado: {manga.Titulo}");
                continue;
            }

            mangas.Add(manga);
            generated++;

            // Agregar en lotes de 500 para mejor rendimiento
            if (mangas.Count >= 500)
            {
                await _mangaRepository.AddRangeAsync(mangas);
                mangas.Clear();
                _logger.LogInformation($"Generados y almacenados {generated} mangas");
            }
        }

        // Agregar los mangas restantes
        if (mangas.Any())
        {
            await _mangaRepository.AddRangeAsync(mangas);
        }

        _logger.LogInformation($"Generación completada. Total de mangas generados: {generated} de {count} solicitados");
        return mangas;
    }

    private async Task<HashSet<string>> GetExistingTitlesAsync()
    {
        var allMangas = await _mangaRepository.GetAllAsync();
        return new HashSet<string>(allMangas.Select(m => m.TituloNormalizado));
    }

    public async Task<List<Manga>> GetAllMangasAsync()
    {
        return await _mangaRepository.GetAllAsync();
    }

    public async Task<Dictionary<string, List<Manga>>> GetDuplicateMangasAsync()
    {
        return await _mangaRepository.GetDuplicatesAsync();
    }

    public async Task<bool> DeleteMangaAsync(string id)
    {
        return await _mangaRepository.DeleteAsync(id);
    }
} 