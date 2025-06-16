// JaveragesLibrary/Program.cs

using Microsoft.EntityFrameworkCore;
using MiMangaBot.Data;
using MiMangaBot.Services;
using MiMangaBot.Domain.Repositories;
using MiMangaBot.Infrastructure.Repositories;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.OpenApi.Models;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar Firebase
if (FirebaseApp.DefaultInstance == null)
{
    // Leer configuración de Firebase
    var firebaseSection = builder.Configuration.GetSection("Firebase");
    var firebaseConfig = new
    {
        type = "service_account",
        project_id = firebaseSection["ProjectId"],
        private_key_id = firebaseSection["PrivateKeyId"],
        private_key = firebaseSection["PrivateKey"],
        client_email = firebaseSection["ClientEmail"],
        client_id = firebaseSection["ClientId"],
        auth_uri = firebaseSection["AuthUri"],
        token_uri = firebaseSection["TokenUri"],
        auth_provider_x509_cert_url = firebaseSection["AuthProviderX509CertUrl"],
        client_x509_cert_url = firebaseSection["ClientX509CertUrl"]
    };

    var googleCredential = GoogleCredential.FromJson(JsonSerializer.Serialize(firebaseConfig));
    var builderFirestore = new FirestoreDbBuilder
    {
        ProjectId = firebaseSection["ProjectId"],
        Credential = googleCredential
    };
    var firestoreDb = builderFirestore.Build();
    builder.Services.AddSingleton(firestoreDb);
}

// Registrar servicios
builder.Services.AddScoped<IMangaRepository, FirestoreMangaRepository>();
builder.Services.AddScoped<MangaGeneratorService>();

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Manga Generator API", 
        Version = "v1",
        Description = "API para generar datos de mangas únicos y almacenarlos en Firebase"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();