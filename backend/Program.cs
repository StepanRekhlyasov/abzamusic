using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<SongGenerator>();
builder.Services.AddSingleton<AlbumCoverGenerator>();
builder.Services.AddSingleton<MidiGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
