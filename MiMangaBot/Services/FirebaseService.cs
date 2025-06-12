using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace MiMangaBot.Services;

public class FirebaseService
{
    private readonly FirebaseApp _app;
    private readonly IConfiguration _configuration;

    public FirebaseService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        if (FirebaseApp.DefaultInstance == null)
        {
            var firebaseConfig = new
            {
                type = "service_account",
                project_id = _configuration["Firebase:ProjectId"],
                private_key_id = _configuration["Firebase:PrivateKeyId"],
                private_key = _configuration["Firebase:PrivateKey"]?.Replace("\\n", "\n"),
                client_email = _configuration["Firebase:ClientEmail"],
                client_id = _configuration["Firebase:ClientId"],
                auth_uri = _configuration["Firebase:AuthUri"],
                token_uri = _configuration["Firebase:TokenUri"],
                auth_provider_x509_cert_url = _configuration["Firebase:AuthProviderX509CertUrl"],
                client_x509_cert_url = _configuration["Firebase:ClientX509CertUrl"]
            };

            _app = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(System.Text.Json.JsonSerializer.Serialize(firebaseConfig))
            });
        }
        else
        {
            _app = FirebaseApp.DefaultInstance;
        }
    }

    public async Task<string> CreateUserAsync(string email, string password)
    {
        var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
        {
            Email = email,
            Password = password,
            EmailVerified = false,
            Disabled = false
        });

        return userRecord.Uid;
    }

    public async Task<UserRecord> GetUserAsync(string uid)
    {
        return await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
    }

    public async Task DeleteUserAsync(string uid)
    {
        await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
    }
} 