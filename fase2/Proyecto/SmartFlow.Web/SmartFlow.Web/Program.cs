using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers; // asegúrate de tener este using arriba

Console.WriteLine("🔒 Hash de admin123:");
Console.WriteLine(PasswordHelper.HashPassword("admin123"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();



builder.Services.AddDbContext<SmartFlowContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartFlowConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
//  Middleware global de acceso
app.UseMiddleware<SmartFlow.Web.Helpers.Acceso>();

app.UseAuthorization();

app.MapRazorPages(); 

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

app.Run();
