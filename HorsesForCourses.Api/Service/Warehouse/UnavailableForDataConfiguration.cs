using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using HorsesForCourses.Domain;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Domain.Courses;

namespace HorsesForCourses.Api.Service.Warehouse;

public class UnavailableForDataConfiguration : IEntityTypeConfiguration<UnavailableFor>
{
    public void Configure(EntityTypeBuilder<UnavailableFor> unavailableFor)
    {
        unavailableFor.HasKey(c => c.Id);

        var id = unavailableFor.Property(c => c.Id)
            .HasConversion(new IdValueConverter<UnavailableFor>())
            .Metadata;
        id.SetValueComparer(new IdValueComparer<UnavailableFor>());
        id.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        id.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        unavailableFor.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER")
            .HasAnnotation("Sqlite:Autoincrement", true);

        unavailableFor.Property(c => c.CoachId)
            .HasConversion(new IdValueConverter<Coach>());

        unavailableFor.Property(c => c.CourseId)
            .HasConversion(new IdValueConverter<Course>());
    }
}
