using Microsoft.EntityFrameworkCore;
using HorsesForCourses.Api.Service.Warehouse.Coaches;
using HorsesForCourses.Api.Service.Warehouse.Courses;
using HorsesForCourses.Api.Service.Warehouse.Accounts;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Domain.Courses;
using HorsesForCourses.Domain.Accounts;
using HorsesForCourses.Domain;

namespace HorsesForCourses.Api.Service.Warehouse;

public class AppDbContext : DbContext
{
    public DbSet<Coach> Coaches { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<UnavailableFor> UnavailableFor { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CoachesDataConfiguration());
        modelBuilder.ApplyConfiguration(new CourseDataConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationUserDataConfiguration());
        modelBuilder.ApplyConfiguration(new UnavailableForDataConfiguration());
    }
}
