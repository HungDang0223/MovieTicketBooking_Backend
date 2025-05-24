using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.RepositoryImpl;
using MovieTicket_Backend.RepositoryInpl;
using MovieTicket_Backend.Services;
using StackExchange.Redis;
using System;
using System.Text;
using VNPAY.NET;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseMySql(
       builder.Configuration.GetConnectionString("DefaultConnection"),
       ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
   ));

builder.Services.AddFirebaseNotifications(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DbConnectionFactory>();

// Đăng ký các servie
builder.Services.AddSingleton<IVnpay, Vnpay>();
builder.Services.AddSingleton<IVerificationService, VerificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
builder.Services.AddScoped<ISeatReservationNotificationService, SeatReservationNotificationService>();
builder.Services.AddScoped<IDeviceTokenService, DeviceTokenService>();
builder.Services.AddScoped<BatchMovieShowingService>();

// Đăng ký các repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<DiscountRepository>();
builder.Services.AddScoped<MovieRepository>();
builder.Services.AddScoped<CinemaRepository>();
builder.Services.AddScoped<ShowingMovieRepository>();
builder.Services.AddScoped<ScreenRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ISeatReservationRepository, SeatReservationRepository>();

builder.Services.AddHostedService<ExpiredReservationCleanupService>();

//Config Redis
//Config Redis - Improved
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");

    Console.WriteLine($"Attempting to connect to Redis at: {configuration}");

    var options = ConfigurationOptions.Parse(configuration!);
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 10000;
    options.SyncTimeout = 10000;
    options.ReconnectRetryPolicy = new ExponentialRetry(5000);

    try
    {
        var multiplexer = ConnectionMultiplexer.Connect(options);
        Console.WriteLine("Successfully connected to Redis");
        return multiplexer;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to connect to Redis: {ex.Message}");
        throw;
    }
});

builder.Services.AddScoped<IVerificationService, VerificationService>();

builder.Services.AddSwaggerGen();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection:blobServiceUri"]!).WithName("StorageConnection");
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnection:queueServiceUri"]!).WithName("StorageConnection");
    clientBuilder.AddTableServiceClient(builder.Configuration["StorageConnection:tableServiceUri"]!).WithName("StorageConnection");
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "YourIssuer",
            ValidAudience = "http://localhost:5000",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dphung23_strong_secret_key_23022003"))        };
    });

// Thêm SignalR
builder.Services.AddSignalR(options =>
{
    // Tăng timeout để xử lý kết nối chậm
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);

    // Cho phép kết nối WebSocket
    options.EnableDetailedErrors = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// matching column names with underscore with C# properties in class
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();
app.UsePathBase("/tickat");
app.UseCors(builder =>
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

app.UseMiddleware<WebSocketMiddleware>();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
