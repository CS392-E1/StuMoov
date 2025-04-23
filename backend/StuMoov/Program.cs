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


var builder = WebApplication.CreateBuilder(args);
var policyName = "google-map-front-end-CORS"; //Policy to allow frontend to access

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
                  .AllowAnyMethod();
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
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LenderOnly",
      p => p.RequireClaim("role", "LENDER"));
    options.AddPolicy("RenterOnly",
      p => p.RequireClaim("role", "RENTER"));
});

builder.Services.AddSingleton(supabase);
builder.Services.AddSingleton<StorageLocationDao>(new StorageLocationDao());
builder.Services.AddControllers()
  .AddJsonOptions(opts =>
  {
      // allow string values for enums, case‐insensitive
      opts.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
      );
  });

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
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
