using Chat.Data;
using Chat.Hub;
using Chat.Interfaces;
using Chat.Services;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.Mongo.Migration.Strategies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BackgroundService = Chat.Services.BackgroundService;
using Chat.Cache;

var builder = WebApplication.CreateBuilder(args);

// Render için port ayarý
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configuration ayarlarý
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// MongoDB baðlantý stringini environment variable'dan al
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
    ?? builder.Configuration["MongoDB:ConnectionString"];

builder.Services.AddHangfire(config =>
{
    config.UseMongoStorage(mongoConnectionString, "HangfireDb", new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        }
    });
});

builder.Services.AddHangfireServer();

// Performance iyileþtirmeleri
builder.Services.AddResponseCompression();
builder.Services.AddMemoryCache();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbService>();

// CORS politikasý güncelleme (Render için)
builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"],
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"]))
    };
});

// Servis kayýtlarý
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRelationShipService>();
builder.Services.AddScoped<UserRelationRequestsService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<BackgroundService>();

var app = builder.Build();

// Production ortamý için güvenlik ayarlarý
if (app.Environment.IsProduction())
{
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseResponseCompression();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire job setup
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var backgroundService = scope.ServiceProvider.GetRequiredService<BackgroundService>();

    recurringJobManager.AddOrUpdate(
        "background-service-job",
        () => backgroundService.TakeBackup(),
        "* * * * *"
    );
}

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();
