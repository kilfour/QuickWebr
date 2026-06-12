using HorsesForCourses.Abstractions;
using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Domain;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace HorsesForCourses.Api.Service;

public sealed class UnavailableProjection
{
    private readonly IDbContextFactory<AppDbContext> factory;

    public UnavailableProjection(IDbContextFactory<AppDbContext> factory) => this.factory = factory;

    public async Task OnCoachUpdatedAsync(Id<Coach> coachId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // Load the coach (tracked=false for speed)
        var coach = await db.Coaches
            .AsNoTracking()
            .FirstAsync(c => c.Id == coachId, ct);

        // Stream only relevant courses (pre-filter to avoid scanning everything)
        var candidateCourses = db.Courses
            .AsNoTracking()
            .Where(c => c.IsConfirmed && c.AssignedCoach == null)
            // Optional: if you can translate skill/time filters, add them here to shrink the set
            .AsAsyncEnumerable();

        // NEW computed pairs
        var newlyComputed = new HashSet<(Id<Coach> CoachId, Id<Course> CourseId)>();
        await foreach (var u in CoachUnavailableForCourseUpdater.FromCoachAsync(coach, candidateCourses).WithCancellation(ct))
            newlyComputed.Add((u.CoachId, u.CourseId));

        // EXISTING pairs for this coach (limit to candidates’ course ids if you like)
        var existing = await db.UnavailableFor
            .AsNoTracking()
            .Where(u => u.CoachId == coachId)
            .Select(u => new { u.CoachId, u.CourseId })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(e => (e.CoachId, e.CourseId))
            .ToHashSet();

        // Diff
        var toInsert = newlyComputed.Except(existingSet).ToList();
        var toDelete = existingSet.Except(newlyComputed).ToList();

        if (toDelete.Count > 0)
        {
            db.UnavailableFor.RemoveRange(
                toDelete.Select(k => new UnavailableFor(k.CoachId, k.CourseId)));
        }

        if (toInsert.Count > 0)
        {
            // Batch to keep memory/transactions bounded
            const int Batch = 2_000;
            for (int i = 0; i < toInsert.Count; i += Batch)
            {
                var chunk = toInsert.Skip(i).Take(Batch)
                    .Select(k => new UnavailableFor(k.CoachId, k.CourseId));
                await db.UnavailableFor.AddRangeAsync(chunk, ct);
                await db.SaveChangesAsync(ct);
                db.ChangeTracker.Clear();
            }
        }
        else
        {
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task OnCourseUpdatedAsync(Id<Course> courseId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        var course = await db.Courses
            .AsNoTracking()
            .FirstAsync(c => c.Id == courseId, ct);

        // If the course is not applicable, just purge any existing pairs
        if (!course.IsConfirmed || course.AssignedCoach != null)
        {
            var stale = await db.UnavailableFor
                .Where(u => u.CourseId == courseId)
                .ToListAsync(ct);

            if (stale.Count > 0)
            {
                db.UnavailableFor.RemoveRange(stale);
                await db.SaveChangesAsync(ct);
            }
            return;
        }

        var candidateCoaches = db.Coaches.AsNoTracking().AsAsyncEnumerable();

        var newlyComputed = new HashSet<(Id<Coach> CoachId, Id<Course> CourseId)>();
        await foreach (var u in CoachUnavailableForCourseUpdater.FromCourseAsync(course, candidateCoaches).WithCancellation(ct))
            newlyComputed.Add((u.CoachId, u.CourseId));

        var existing = await db.UnavailableFor
            .AsNoTracking()
            .Where(u => u.CourseId == courseId)
            .Select(u => new { u.CoachId, u.CourseId })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(e => (e.CoachId, e.CourseId))
            .ToHashSet();

        var toInsert = newlyComputed.Except(existingSet).ToList();
        var toDelete = existingSet.Except(newlyComputed).ToList();

        if (toDelete.Count > 0)
            db.UnavailableFor.RemoveRange(toDelete.Select(k => new UnavailableFor(k.CoachId, k.CourseId)));

        if (toInsert.Count > 0)
            await db.UnavailableFor.AddRangeAsync(toInsert.Select(k => new UnavailableFor(k.CoachId, k.CourseId)), ct);

        await db.SaveChangesAsync(ct);
    }
}

