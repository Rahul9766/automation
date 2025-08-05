using AzureServerManager.Services;
using AzureServerManager.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ServerService>();
builder.Services.AddSingleton<FileOperationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();
app.MapHub<ServiceHub>("/serviceHub");

// Default route to serve the main page
app.MapGet("/", async context =>
{
    context.Response.Redirect("/index.html");
});

app.Run(); 