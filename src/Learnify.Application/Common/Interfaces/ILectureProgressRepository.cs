using Learnify.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learnify.Application.Common.Interfaces;

public interface ILectureProgressRepository
{
    /// <summary>
    /// Gets the progress for a specific lecture and user
    /// </summary>
    Task<LectureProgress?> GetAsync(string userId, int lectureId);
    
    /// <summary>
    /// Gets all progress records for a user in a specific course
    /// </summary>
    Task<IEnumerable<LectureProgress>> GetByCourseAsync(string userId, int courseId);
    
    /// <summary>
    /// Creates or updates progress for a lecture
    /// </summary>
    Task<LectureProgress> UpsertAsync(LectureProgress progress);
    
    /// <summary>
    /// Marks a lecture as completed
    /// </summary>
    Task MarkCompletedAsync(string userId, int lectureId);
    
    /// <summary>
    /// Marks a lecture as not completed (undo)
    /// </summary>
    Task MarkIncompleteAsync(string userId, int lectureId);
}
