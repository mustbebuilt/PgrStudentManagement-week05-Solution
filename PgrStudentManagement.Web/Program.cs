using PgrStudentManagement.Web.Modules.Students.Contracts;
using PgrStudentManagement.Web.Modules.Students.Data;
using PgrStudentManagement.Web.Modules.Students.Data.Daos;
using PgrStudentManagement.Web.Modules.Students.Services;

using PgrStudentManagement.Web.Modules.AcademicSupervisors.Data;
using PgrStudentManagement.Web.Modules.AcademicSupervisors.Services;

using PgrStudentManagement.Web.Modules.ResearchActivities.Data;
using PgrStudentManagement.Web.Modules.ResearchActivities.Services;

using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StudentManagementDbContext>(
    options => options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "StudentManagementDatabase")));

builder.Services.AddSingleton<IMongoClient>(
    _ => new MongoClient(
        builder.Configuration["ResearchActivityDatabase:ConnectionString"]));

// Add application services
builder.Services.AddSingleton<StudentStorage>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<EnrolmentDao>();
builder.Services.AddScoped<StudentDataSeeder>();

builder.Services.AddScoped<ResearchActivityRepository>();
builder.Services.AddScoped<ResearchActivityService>();

// Adding StudentService as the implementation of the IStudentLookup contract
builder.Services.AddScoped<IStudentLookup>(
    provider => provider.GetRequiredService<StudentService>());

builder.Services.AddSingleton<AcademicSupervisorStorage>();
builder.Services.AddScoped<AcademicSupervisorService>();
builder.Services.AddScoped<AcademicSupervisorDataSeeder>();

var app = builder.Build();

// Seeding initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<StudentManagementDbContext>();

    await context.Database.MigrateAsync();
    await StudentDataSeeder.SeedAsync(context);

    var researchActivityRepository = scope.ServiceProvider
        .GetRequiredService<ResearchActivityRepository>();
    await ResearchActivityDataSeeder.SeedAsync(researchActivityRepository);

    var studentSeeder = scope.ServiceProvider
        .GetRequiredService<StudentDataSeeder>();

    studentSeeder.Seed();

    var academicSupervisorSeeder =
        scope.ServiceProvider
            .GetRequiredService<
                AcademicSupervisorDataSeeder>();

    academicSupervisorSeeder.Seed();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

//Code for .Net 8.0
app.UseStaticFiles();
app.MapControllerRoute(
    name: "research_activities",
    pattern: "ResearchActivities/{action=Index}/{id?}",
    defaults: new { controller = "ResearchActivity" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Code for .Net 10.0
//app.MapStaticAssets();
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}")
//     .WithStaticAssets();

app.Run();
