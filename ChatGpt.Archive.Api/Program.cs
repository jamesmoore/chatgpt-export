using ChatGpt.Archive.Api;
using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using System.IO.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddOptions<ArchiveSourcesOptions>()
    .Bind(builder.Configuration.GetSection("ArchiveSources"))
    .Validate(options => options.SourceDirectories.Count > 0,
              "At least one source directory must be configured");

builder.Services.AddControllers();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<IConversationsService, ConversationsService>();
builder.Services.AddSingleton<IConversationAssetsCache, ConversationAssetsCache>();
builder.Services.AddSingleton<ApiAssetLocator>();
builder.Services.AddSingleton<ConversationFormatterFactory>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize conversations.
app.Services.GetRequiredService<IConversationsService>().GetLatestConversations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();
