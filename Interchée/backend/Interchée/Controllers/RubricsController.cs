using Interchée.Contracts.Assignments;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    [ApiController]
    [Route("rubrics")]
    [Authorize(Roles = "Admin,HR,Supervisor")]
    public class RubricsController(AppDbContext db) : ControllerBase
    {
        private readonly AppDbContext _db = db;

        /// <summary>Get all active rubrics</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RubricReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RubricReadDto>>> GetAll()
        {
            var rubrics = await _db.Rubrics
                .Where(r => r.IsActive)
                .Include(r => r.Items.OrderBy(i => i.Order))
                .Select(r => new RubricReadDto(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsActive,
                    r.CreatedAt,
                    r.Items.Select(i => new RubricItemReadDto(
                        i.Id, i.Criteria, i.Description, i.MaxScore, i.Order
                    )).ToList()
                ))
                .ToListAsync();

            return Ok(rubrics);
        }

        /// <summary>Create a new rubric</summary>
        [HttpPost]
        [ProducesResponseType(typeof(RubricReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RubricReadDto>> Create([FromBody] RubricCreateDto dto)
        {
            var rubric = new Rubric
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Add rubric items
            foreach (var itemDto in dto.Items.OrderBy(i => i.Order))
            {
                rubric.Items.Add(new RubricItem
                {
                    Criteria = itemDto.Criteria.Trim(),
                    Description = itemDto.Description?.Trim() ?? string.Empty,
                    MaxScore = itemDto.MaxScore,
                    Order = itemDto.Order
                });
            }

            _db.Rubrics.Add(rubric);
            await _db.SaveChangesAsync();

            var readDto = new RubricReadDto(
                rubric.Id,
                rubric.Name,
                rubric.Description,
                rubric.IsActive,
                rubric.CreatedAt,
                rubric.Items.Select(i => new RubricItemReadDto(
                    i.Id, i.Criteria, i.Description, i.MaxScore, i.Order
                )).ToList()
            );

            return Ok(readDto);
        }

        /// <summary>Delete a rubric (soft delete)</summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(long id)
        {
            var rubric = await _db.Rubrics
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rubric == null) return NotFound();

            // Check if rubric is being used by any grades
            var isUsed = await _db.Grades.AnyAsync(g => g.RubricId == id);
            if (isUsed)
            {
                return Conflict(new
                {
                    message = "Cannot delete rubric that is being used by existing grades. Archive it instead.",
                    suggestion = "Set IsActive to false to hide it from new assignments."
                });
            }

            // Soft delete by setting IsActive to false
            rubric.IsActive = false;

            // Optional: Also soft delete rubric items if needed
            foreach (var item in rubric.Items)
            {
                // If you have IsActive on RubricItem, set it to false here
                // item.IsActive = false;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}