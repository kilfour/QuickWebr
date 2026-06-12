using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using HorsesForCourses.Domain.Courses;
using HorsesForCourses.Domain.Courses.TimeSlots;
using HorsesForCourses.Abstractions;

namespace HorsesForCourses.Api.Service.Warehouse.Courses;

public class CourseDataConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> course)
    {
        course.HasKey(c => c.Id);

        var id = course.Property(c => c.Id)
            .HasConversion(new IdValueConverter<Course>())
            .Metadata;
        id.SetValueComparer(new IdValueComparer<Course>());
        id.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        id.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        course.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER")
            .HasAnnotation("Sqlite:Autoincrement", true);

        course.OwnsOne(c => c.Name, name =>
        {
            name.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(DefaultString.MaxLength);
        });


        course.OwnsOne(c => c.Period, period =>
        {
            period.Property(c => c.Start)
                .IsRequired();

            period.Property(c => c.End)
                .IsRequired();
        });

        course.Property(c => c.IsConfirmed)
            .IsRequired();

        course.OwnsMany(c => c.RequiredSkills, sb =>
        {
            sb.WithOwner().HasForeignKey("CourseId");

            sb.Property<string>("value")
              .HasColumnName("Skill")
              .IsRequired()
              .HasMaxLength(100);

            sb.HasKey("CourseId", "value");

            sb.ToTable("CourseRequiredSkills");
        });

        course.OwnsMany(c => c.TimeSlots, timeSlot =>
        {
            timeSlot.WithOwner().HasForeignKey("CourseId");
            timeSlot.ToTable("CourseTimeSlots");

            timeSlot.HasKey("CourseId", nameof(TimeSlot.Day), nameof(TimeSlot.Start));

            timeSlot.Property(t => t.Day)
                .HasColumnName("Day")
                .IsRequired();

            timeSlot.Property(t => t.Start)
                .HasColumnName("StartHour")
                .HasConversion(v => v.Value, v => OfficeHour.From(v))
                .IsRequired();

            timeSlot.Property(t => t.End)
                .HasColumnName("EndHour")
                .HasConversion(v => v.Value, v => OfficeHour.From(v))
                .IsRequired();
        });

        course.HasOne(c => c.AssignedCoach)
               .WithMany(c => c.AssignedCourses)
               .HasForeignKey("AssignedCoachId")
               .OnDelete(DeleteBehavior.Restrict);

        course
            .Navigation(c => c.AssignedCoach)
            .AutoInclude();

        course.ToTable("Courses");
    }
}
