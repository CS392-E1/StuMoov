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
using Stripe;
using StuMoov.Services.StripeService;
using StuMoov.Services.BookingService;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var policyName = "google-map-front-end-CORS"; //Policy to allow frontend to access

// Configure Stripe
var stripeApiKey = builder.Configuration["Stripe:SecretKey"];
StripeConfiguration.ApiKey = stripeApiKey;

var url = builder.Configuration["Supabase:SUPABASE_URL"]!;
var key = builder.Configuration["Supabase:SUPABASE_KEY"];

var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true
};

var supabase = new Supabase.Client(url, key, options);
await supabase.InitializeAsync();

string connectionString = builder.Configuration["Supabase:CONNECTION_STRING"]!;

// Register the DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Needed for cookies
        });
});

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("firebase-credentials.json")
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };

        // Support cookies
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to get the token from the cookie
                if (context.Request.Cookies.TryGetValue("auth_token", out string? token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LenderOnly",
      p => p.RequireClaim(ClaimTypes.Role, "LENDER"));
    options.AddPolicy("RenterOnly",
      p => p.RequireClaim(ClaimTypes.Role, "RENTER"));
});

builder.Services.AddSingleton(supabase);
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<StripeService>();

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

builder.Services.AddScoped<MessageDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new MessageDao(context);
});

// Register PaymentDao
builder.Services.AddScoped<PaymentDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new PaymentDao(context);
});

builder.Services.AddScoped<BookingService>();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        // Optional: use CamelCase
        // options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(policyName);
app.UseJwtCookieMiddleware(); // Add our custom middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
