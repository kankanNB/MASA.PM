using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Masa.Utils.Caller.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddMasaBlazorExperimentalComponents();
builder.Services.AddMasaBlazor(builder =>
{
    builder.UseTheme(option =>
        {
            option.Primary = "#4318FF";
            option.Accent = "#4318FF";
        }
    );
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddGlobalForServer();
builder.Services.AddCaller(Assembly.Load("MASA.PM.Caller"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
