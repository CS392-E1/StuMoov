/*
 * Program.cs
 * 
 * Main application entry point and configuration for StuMoov API
 * Configures services, middleware, authentication, and database connections
 */

using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Supabase;
using Microsoft.EntityFrameworkCore;
using StuMoov.Dao;
using StuMoov.Db;
using Google.Apis.Auth.OAuth2;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StuMoov.Middleware;
using StuMoov.Services.AuthService;
using StuMoov.Services.BookingService;
using Stripe;
using StuMoov.Services.StripeService;
using StuMoov.Services.BookingService;
using System.Security.Claims;
using StuMoov.Services.ChatService;
using StuMoov.Service;
using StuMoov.Services.PaymentService;

// Create web application builder with default configurations
var builder = WebApplication.CreateBuilder(args);
var policyName = "google-map-front-end-CORS"; // Policy name to allow frontend access

// ===================================
// STRIPE CONFIGURATION
// ===================================
// Initialize Stripe with API key from configuration
var stripeApiKey = builder.Configuration["Stripe:SecretKey"];
StripeConfiguration.ApiKey = stripeApiKey;

// ===================================
// SUPABASE CONFIGURATION
// ===================================
// Set up Supabase client for realtime features
var url = builder.Configuration["Supabase:SUPABASE_URL"]!;
var key = builder.Configuration["Supabase:SUPABASE_KEY"];

var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true // Enable automatic WebSocket connections
};

// Initialize Supabase client asynchronously
var supabase = new Supabase.Client(url, key, options);
await supabase.InitializeAsync();

// Get database connection string from configuration
string connectionString = builder.Configuration["Supabase:CONNECTION_STRING"]!;

// ===================================
// DATABASE CONFIGURATION
// ===================================
// Register DbContext with Postgres provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ===================================
// CORS CONFIGURATION
// ===================================
// Configure Cross-Origin Resource Sharing policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for cookie authentication
        });
});

// ===================================
// FIREBASE CONFIGURATION
// ===================================
// Initialize Firebase Admin SDK with service account credentials
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("firebase-credentials.json")
});

// ===================================
// JWT AUTHENTICATION CONFIGURATION
// ===================================
// Get JWT settings from configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

// Configure JWT authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Configure JWT validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero // No tolerance for token expiration
        };

        // Add support for cookie-based authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Extract token from cookie if present
                if (context.Request.Cookies.TryGetValue("auth_token", out string? token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

// ===================================
// AUTHORIZATION CONFIGURATION
// ===================================
// Configure role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LenderOnly",
      p => p.RequireClaim(ClaimTypes.Role, "LENDER"));
    options.AddPolicy("RenterOnly",
      p => p.RequireClaim(ClaimTypes.Role, "RENTER"));
});

// ===================================
// SERVICE REGISTRATION
// ===================================
// Register shared services
builder.Services.AddSingleton(supabase); // Register Supabase client as singleton
builder.Services.AddScoped<AuthService>(); // Authentication service
builder.Services.AddScoped<StripeService>(); // Stripe payment processing service
builder.Services.AddScoped<ChatSessionService>(); // Chat session management
builder.Services.AddScoped<ChatMessageService>(); // Chat message handling

// Register DAO (Data Access Object) services
// Each DAO is scoped to the HTTP request lifetime
builder.Services.AddScoped<StorageLocationDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new StorageLocationDao(context);
});
builder.Services.AddScoped<BookingDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new BookingDao(context);
});
builder.Services.AddScoped<UserDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new UserDao(context);
});

builder.Services.AddScoped<StripeCustomerDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new StripeCustomerDao(context);
});

builder.Services.AddScoped<StripeConnectAccountDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new StripeConnectAccountDao(context);
});

builder.Services.AddScoped<ChatSessionDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new ChatSessionDao(context);
});

// Register payment data access service
builder.Services.AddScoped<PaymentDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new PaymentDao(context);
});

// Register business logic services
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<BookingService>();

builder.Services.AddScoped<ChatMessageDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new ChatMessageDao(context);
});

builder.Services.AddScoped<ImageDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new ImageDao(context);
});

// ===================================
// CONTROLLER CONFIGURATION
// ===================================
// Configure API controllers with Newtonsoft JSON settings
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        // Prevent circular references in JSON responses
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        // Convert enums to strings in JSON responses
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        // Optional: Use camelCase for property names
        // options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

// Add Swagger API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===================================
// APPLICATION PIPELINE CONFIGURATION
// ===================================
// Build the application
var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI in development environment only
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// Apply CORS policy
app.UseCors(policyName);

// Apply custom middleware to handle JWT in cookies
app.UseJwtCookieMiddleware();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Start the application
app.Run();