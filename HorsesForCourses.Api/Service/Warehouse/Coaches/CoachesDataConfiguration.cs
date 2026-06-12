using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Abstractions;

namespace HorsesForCourses.Api.Service.Warehouse.Coaches;

public class CoachesDataConfiguration : IEntityTypeConfiguration<Coach>
{
    public void Configure(EntityTypeBuilder<Coach> coach)
    {
        coach.HasKey(c => c.Id);

        var id = coach.Property(c => c.Id)
            .HasConversion(new IdValueConverter<Coach>())
            .Metadata;
        id.SetValueComparer(new IdValueComparer<Coach>());
        id.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        id.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        coach.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("INTEGER")
            .HasAnnotation("Sqlite:Autoincrement", true);

        coach.OwnsOne(c => c.Name, name =>
        {
            name.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(DefaultString.MaxLength);
        });
        coach.OwnsOne(c => c.Email, email =>
        {
            email.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(DefaultString.MaxLength);
        });

        coach.OwnsMany(c => c.Skills, sb =>
        {
            sb.WithOwner().HasForeignKey("CoachId");

            sb.Property<string>("value")
              .HasColumnName("Skill")
              .IsRequired()
              .HasMaxLength(100);

            sb.HasKey("CoachId", "value");

            sb.ToTable("CoachSkills");
        });

        coach
            .Navigation(c => c.AssignedCourses)
            .AutoInclude();

        coach.ToTable("Coaches");
    }
}
