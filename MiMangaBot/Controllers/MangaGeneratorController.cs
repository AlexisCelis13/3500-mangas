using Microsoft.AspNetCore.Mvc;
using MiMangaBot.Services;

namespace MiMangaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MangaGeneratorController : ControllerBase
{
    private readonly MangaGeneratorService _mangaGeneratorService;
    private readonly ILogger<MangaGeneratorController> _logger;

    public MangaGeneratorController(
        MangaGeneratorService mangaGeneratorService,
        ILogger<MangaGeneratorController> logger)
    {
        _mangaGeneratorService = mangaGeneratorService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateMangas([FromQuery] int count = 3500)
    {
        try
        {
            if (count <= 0 || count > 10000)
            {
                return BadRequest("El número de mangas debe estar entre 1 y 10000");
            }

            _logger.LogInformation($"Iniciando generación de {count} mangas");
            var mangas = await _mangaGeneratorService.GenerateAndStoreMangasAsync(count);
            
            return Ok(new
            {
                message = $"Se generaron {mangas.Count} mangas exitosamente",
                count = mangas.Count,
                mangas // Devuelve todos los mangas generados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar mangas");
            return StatusCode(500, "Error al generar los mangas: " + ex.Message);
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetGenerationStatus()
    {
        try
        {
            _logger.LogInformation("Solicitud para obtener el estado de generación de mangas.");
            var allMangas = await _mangaGeneratorService.GetAllMangasAsync();
            var duplicates = await _mangaGeneratorService.GetDuplicateMangasAsync();

            return Ok(new 
            { 
                message = "Servicio de generación de mangas activo",
                totalMangas = allMangas.Count,
                mangasDuplicados = duplicates.Count,
                ultimaActualizacion = allMangas.Max(m => m.FechaActualizacion ?? m.FechaCreacion),
                estado = new
                {
                    totalGeneros = allMangas.Select(m => m.Genero).Distinct().Count(),
                    totalEditoriales = allMangas.Select(m => m.Editorial).Distinct().Count(),
                    mangasEnPublicacion = allMangas.Count(m => m.EnPublicacion),
                    mangasFinalizados = allMangas.Count(m => m.Estado == "Finalizado")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el estado de generación");
            return StatusCode(500, "Error al obtener el estado: " + ex.Message);
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllMangas()
    {
        try
        {
            _logger.LogInformation("Solicitud para obtener todos los mangas.");
            var mangas = await _mangaGeneratorService.GetAllMangasAsync();

            return Ok(new
            {
                count = mangas.Count,
                mangas = mangas // Devuelve todos los mangas
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los mangas.");
            return StatusCode(500, "Error al obtener todos los mangas: " + ex.Message);
        }
    }

    [HttpGet("duplicates")]
    public async Task<IActionResult> GetDuplicateMangas()
    {
        try
        {
            _logger.LogInformation("Solicitud para obtener mangas duplicados.");
            var duplicateMangas = await _mangaGeneratorService.GetDuplicateMangasAsync();
            
            return Ok(new
            {
                count = duplicateMangas.Count,
                duplicates = duplicateMangas
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mangas duplicados.");
            return StatusCode(500, "Error al obtener mangas duplicados: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteManga(string id)
    {
        try
        {
            _logger.LogInformation($"Solicitud para eliminar manga con ID: {id}");
            var result = await _mangaGeneratorService.DeleteMangaAsync(id);

            if (result)
            {
                return Ok($"Manga con ID {id} eliminado exitosamente.");
            }
            return NotFound($"Manga con ID {id} no encontrado o no se pudo eliminar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar manga con ID: {id}");
            return StatusCode(500, "Error al eliminar el manga: " + ex.Message);
        }
    }
} 