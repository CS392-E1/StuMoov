using StuMoov.Dao;

var builder = WebApplication.CreateBuilder(args);
var policyName = "google-map-front-end-CORS"; //Policy to allow frontend to access

var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");

var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = true
};

#pragma warning disable CS8604 // Possible null reference argument.
var supabase = new Supabase.Client(url, key, options);
#pragma warning restore CS8604 // Possible null reference argument.
await supabase.InitializeAsync();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Allow frontend(5173) to visit
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddSingleton<StorageLocationDao>(new StorageLocationDao());
builder.Services.AddControllers();
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
