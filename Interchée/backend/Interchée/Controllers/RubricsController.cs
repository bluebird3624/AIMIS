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
    }
}