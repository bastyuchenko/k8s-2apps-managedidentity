using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using ProfileAPI.Models;
using Microsoft.EntityFrameworkCore;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// KeyVault
var keyVaultEndpoint = new Uri(builder.Configuration["AzureKeyVault:Uri"]);

SecretClient secretClient;
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    secretClient = new SecretClient(
                        keyVaultEndpoint,
                        new ClientSecretCredential(
                            builder.Configuration["AzureKeyVault:TenantId"],
                            builder.Configuration["AzureKeyVault:ClientId"],
                            builder.Configuration["AzureKeyVault:ClientSecret"]));

    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
}
else
{

    secretClient = new SecretClient(
                        keyVaultEndpoint,
                        new DefaultAzureCredential(new DefaultAzureCredentialOptions{ ManagedIdentityClientId = "a0b5a2d2-83a7-4bdb-b8de-a73ebe62b414"}));

    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
}

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("ProfileAPIAzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();

builder.Services.AddDbContext<ProfileContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ProfileContext")));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                        builder =>
                        builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod());
            });



var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAllOrigins");

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

