using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using HorsesForCourses.Domain.Accounts;
using HorsesForCourses.Abstractions;

namespace HorsesForCourses.Api.Service.Warehouse.Accounts;

public class ApplicationUserDataConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> applicationUser)
    {
        applicationUser.HasKey(c => c.Id);

        var id = applicationUser.Property(c => c.Id)
            .HasConversion(new IdValueConverter<ApplicationUser>())
            .Metadata;
        id.SetValueComparer(new IdValueComparer<ApplicationUser>());
        id.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        id.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        applicationUser.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER")
            .HasAnnotation("Sqlite:Autoincrement", true);

        applicationUser.OwnsOne(c => c.Name, name =>
        {
            name.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(DefaultString.MaxLength);
        });

        applicationUser.OwnsOne(c => c.Email, email =>
        {
            email.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(DefaultString.MaxLength);
        });

        applicationUser.Property(a => a.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        applicationUser.Property(a => a.Role)
            .IsRequired()
            .HasMaxLength(50);
    }
}
