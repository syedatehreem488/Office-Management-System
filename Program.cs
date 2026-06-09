using OfficeManagementSystem.Data;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF is free under the Community license for small projects.
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<LeaveRepository>();
builder.Services.AddScoped<PayrollRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<MeetingRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<DatabaseHelper>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
