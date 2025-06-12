using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace MiMangaBot.Domain.Models;

[FirestoreData]
public class Manga
{
    [FirestoreDocumentId]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [FirestoreProperty]
    public string Titulo { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string Autor { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string Genero { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public int AnioPublicacion { get; set; }
    
    [FirestoreProperty]
    public int Volumenes { get; set; }
    
    [FirestoreProperty]
    public bool EnPublicacion { get; set; }
    
    [FirestoreProperty]
    public string Sinopsis { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public double Calificacion { get; set; }
    
    [FirestoreProperty]
    public int NumeroCapitulos { get; set; }
    
    [FirestoreProperty]
    public string Editorial { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string Estado { get; set; } = string.Empty; // En emisión, Finalizado, Pausado
    
    [FirestoreProperty]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    [FirestoreProperty]
    public DateTime? FechaActualizacion { get; set; }
    
    // Propiedad para verificar unicidad
    public string TituloNormalizado => Titulo.ToLower().Trim();
} 