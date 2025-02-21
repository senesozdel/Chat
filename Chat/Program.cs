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
using DotNetEnv;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "4000";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var mongoConnectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration["MongoDB:ConnectionString"]
    : Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");

builder.Services.AddHangfire(config =>
{
    var options = new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        }
    };

    if (builder.Environment.IsDevelopment())
    {
        options.CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection;
    }

    config.UseMongoStorage(mongoConnectionString, "HangfireDb", options);
});

builder.Services.AddHangfireServer();

builder.Services.AddResponseCompression();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbService>();

builder.Services.AddCors(opt =>
    opt.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins(builder.Configuration["CorsOrigin"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
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
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("JWT_KEY")))
    };
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRelationShipService>();
builder.Services.AddScoped<UserRelationRequestsService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<BackgroundService>();
builder.Services.AddScoped<IFileStorageService,BackblazeService>();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
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
