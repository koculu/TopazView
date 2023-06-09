using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using TestMVC.Reg;
using Tenray.TopazView;

/*
 Recommendations: 
   * Use VSCode to edit your view files with Razor language highlighting settings.
 */

var path = "Views";
IViewEngine CreateTopazViewEngine()
{
    var topazViewEngine = new ViewEngineFactory()
        .SetContentProvider(new FileSystemContentProvider(path))
        .CreateViewEngine();

    var contentWatcher = new FileSystemContentWatcher();
    contentWatcher.StartWatcher(path, topazViewEngine);
    return topazViewEngine;
}

FileSystemContentWatcher CreateContentWatcher()
{
    var contentWatcher = new FileSystemContentWatcher();
    return contentWatcher;
}

void StartContentWatcher(IServiceProvider serviceProvider)
{
    var viewEngine = serviceProvider.GetService<IViewEngine>();
    serviceProvider.GetService<FileSystemContentWatcher>()?.StartWatcher(path, viewEngine);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container without Razor Pages.
builder.Services.AddControllers();

// Add reqired services by MVC controller.
builder.Services.Add(ServiceDescriptor.Scoped<ITempDataDictionaryFactory, TempDataDictionaryFactory>());
builder.Services.Add(ServiceDescriptor.Scoped<ITempDataProvider, SessionStateTempDataProvider>());
builder.Services.Add(ServiceDescriptor.Scoped<TempDataSerializer, DefaultTempDataSerializer>());

// Add custom ActionResultExecutor to render using Topaz View Engine.
builder.Services.Add(ServiceDescriptor.Scoped<IActionResultExecutor<ViewResult>, TopazViewActionResultExecutor>());

// Add Topaz View Engine.
builder.Services.Add(ServiceDescriptor.Singleton(CreateTopazViewEngine()));

// Add ContentWatcher to invalidate cached compilations.
builder.Services.Add(ServiceDescriptor.Singleton(CreateContentWatcher()));

var app = builder.Build();

StartContentWatcher(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
