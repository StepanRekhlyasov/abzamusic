using backend.Services;

var builder = WebApplication.CreateBuilder(args);

var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(wwwrootPath);
builder.WebHost.UseWebRoot(wwwrootPath);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<SongGenerator>();
builder.Services.AddSingleton<AlbumCoverGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
