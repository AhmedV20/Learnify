using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using Learnify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Infrastructure.Repositories;

public class LectureProgressRepository : ILectureProgressRepository
{
    private readonly ApplicationDbContext _context;

    public LectureProgressRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LectureProgress?> GetAsync(string userId, int lectureId)
    {
        return await _context.LectureProgress
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LectureId == lectureId);
    }

    public async Task<IEnumerable<LectureProgress>> GetByCourseAsync(string userId, int courseId)
    {
        // Get all progress for lectures in sections of the course
        return await _context.LectureProgress
            .Include(lp => lp.Lecture)
                .ThenInclude(l => l!.Section)
            .Where(lp => lp.UserId == userId && lp.Lecture!.Section!.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<LectureProgress> UpsertAsync(LectureProgress progress)
    {
        var existing = await GetAsync(progress.UserId, progress.LectureId);
        
        if (existing == null)
        {
            // Create new
            progress.CreatedAt = DateTime.UtcNow;
            progress.LastWatchedAt = DateTime.UtcNow;
            _context.LectureProgress.Add(progress);
        }
        else
        {
            // Update existing
            existing.WatchedSeconds = progress.WatchedSeconds;
            existing.IsCompleted = progress.IsCompleted;
            existing.LastWatchedAt = DateTime.UtcNow;
            _context.LectureProgress.Update(existing);
            progress = existing;
        }
        
        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task MarkCompletedAsync(string userId, int lectureId)
    {
        var progress = await GetAsync(userId, lectureId);
        
        if (progress == null)
        {
            progress = new LectureProgress
            {
                UserId = userId,
                LectureId = lectureId,
                IsCompleted = true,
                LastWatchedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.LectureProgress.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.LastWatchedAt = DateTime.UtcNow;
            _context.LectureProgress.Update(progress);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task MarkIncompleteAsync(string userId, int lectureId)
    {
        var progress = await GetAsync(userId, lectureId);
        
        if (progress != null)
        {
            progress.IsCompleted = false;
            progress.LastWatchedAt = DateTime.UtcNow;
            _context.LectureProgress.Update(progress);
            await _context.SaveChangesAsync();
        }
    }
}
