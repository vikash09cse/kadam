using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.FileProviders;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Ensure all HTTP methods are allowed
    options.SuppressAsyncSuffixInActionNames = false;
});

// Add HTTP method configuration
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.AddServices();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"UploadFiles")),
    RequestPath = new PathString("/UploadFiles")
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
