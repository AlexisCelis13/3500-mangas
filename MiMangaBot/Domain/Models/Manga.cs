using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace MiMangaBot.Domain.Models;

[FirestoreData]
public class Manga
{
    [FirestoreDocumentId]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El título debe tener entre 1 y 200 caracteres")]
    [FirestoreProperty]
    public string Titulo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El autor es requerido")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El autor debe tener entre 1 y 100 caracteres")]
    [FirestoreProperty]
    public string Autor { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El género es requerido")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El género debe tener entre 1 y 50 caracteres")]
    [FirestoreProperty]
    public string Genero { get; set; } = string.Empty;
    
    [Range(1900, 2100, ErrorMessage = "El año de publicación debe estar entre 1900 y 2100")]
    [FirestoreProperty]
    public int AnioPublicacion { get; set; }
    
    [Range(1, 1000, ErrorMessage = "El número de volúmenes debe estar entre 1 y 1000")]
    [FirestoreProperty]
    public int Volumenes { get; set; }
    
    [FirestoreProperty]
    public bool EnPublicacion { get; set; }
    
    [Required(ErrorMessage = "La sinopsis es requerida")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "La sinopsis debe tener entre 10 y 2000 caracteres")]
    [FirestoreProperty]
    public string Sinopsis { get; set; } = string.Empty;
    
    [Range(0.0, 10.0, ErrorMessage = "La calificación debe estar entre 0 y 10")]
    [FirestoreProperty]
    public double Calificacion { get; set; }
    
    [Range(1, 10000, ErrorMessage = "El número de capítulos debe estar entre 1 y 10000")]
    [FirestoreProperty]
    public int NumeroCapitulos { get; set; }
    
    [Required(ErrorMessage = "La editorial es requerida")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "La editorial debe tener entre 1 y 100 caracteres")]
    [FirestoreProperty]
    public string Editorial { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El estado es requerido")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El estado debe tener entre 1 y 50 caracteres")]
    [FirestoreProperty]
    public string Estado { get; set; } = string.Empty; // En emisión, Finalizado, Pausado
    
    [FirestoreProperty]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    [FirestoreProperty]
    public DateTime? FechaActualizacion { get; set; }
    
    // Propiedad para verificar unicidad
    public string TituloNormalizado => Titulo.ToLower().Trim();
} 