global using iFinancing360.UI.Helper;
using iFinancing360.Helper;
using iFinancing360.UI;

DotNetEnv.Env.Load();
var builder = Application.Build(args);
builder.Services.AddScoped<GlobalConfig>();
var app = Application.Run(builder);

app.Run();
