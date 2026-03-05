using MyFoundryPortal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bind FoundryPortal settings from config / environment variables.
// Environment variable names follow the pattern FOUNDRYPORTAL__<Key>
// (double underscore for nested keys in most hosts) or the
// AZURE_* variables that Azure Identity already reads automatically.
builder.Services.Configure<FoundryServiceOptions>(options =>
{
    // Prefer values from the "FoundryPortal" config section …
    var section = builder.Configuration.GetSection("FoundryPortal");
    options.ProjectEndpoint = section["ProjectEndpoint"]
        ?? Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT")
        ?? string.Empty;
    options.TenantId = section["TenantId"]
        ?? Environment.GetEnvironmentVariable("AZURE_TENANT_ID")
        ?? string.Empty;
    options.ClientId = section["ClientId"]
        ?? Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
        ?? string.Empty;
    options.ClientSecret = section["ClientSecret"]
        ?? Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")
        ?? string.Empty;
});

// Register FoundryService as a singleton – the underlying SDK clients are thread-safe.
builder.Services.AddSingleton<FoundryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
