using Microsoft.EntityFrameworkCore;
using StuMoov.Dao;
using StuMoov.Db;

var builder = WebApplication.CreateBuilder(args);
var policyName = "google-map-front-end-CORS"; //Policy to allow frontend to access

var url = builder.Configuration["Supabase:SUPABASE_URL"];
var key = builder.Configuration["Supabase:SUPABASE_KEY"];

var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true
};

var supabase = new Supabase.Client(url, key, options);
await supabase.InitializeAsync();

// Fixing the syntax error in the connection string assignment
string connectionString = builder.Configuration["Supabase:CONNECTION_STRING"];

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

//builder.Services.AddScoped<StorageLocationDao>(sp =>
//{
//    var context = sp.GetRequiredService<AppDbContext>();
//    return new StorageLocationDao(context);
//});
builder.Services.AddScoped<BookingDao>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    return new BookingDao(context);
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
app.UseAuthorization();

app.MapControllers();

app.Run();
