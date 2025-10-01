using Interchée.Contracts.Departments;
using Interchée.Data;
using Interchée.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Interchée.Controllers
{
    /// <summary>
    /// Departments define organizational units (e.g., IT, Finance).
    /// Other modules (Assignments, Absence) scope permissions by Department.
    /// </summary>
    [ApiController]
    [Route("departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DepartmentsController(AppDbContext db) => _db = db;

        /// <summary>
        /// List departments. Pass onlyActive=false to include inactive ones.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DepartmentReadDto>>> GetAll([FromQuery] bool onlyActive = true)
        {
            var q = _db.Departments.AsNoTracking().AsQueryable();
            if (onlyActive) q = q.Where(d => d.IsActive);

            var list = await q
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentReadDto(d.Id, d.Name, d.Code, d.IsActive))
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>Get a single department by id.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DepartmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentReadDto>> GetById(int id)
        {
            var d = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (d is null) return NotFound();
            return Ok(new DepartmentReadDto(d.Id, d.Name, d.Code, d.IsActive));
        }

        /// <summary>Create a department. (Admin or HR)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(DepartmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DepartmentReadDto>> Create([FromBody] DepartmentCreateDto dto)
        {
            // Friendly pre-check (DB also enforces unique Name via index)
            var exists = await _db.Departments.AnyAsync(d => d.Name == dto.Name);
            if (exists) return BadRequest($"Department with name '{dto.Name}' already exists.");

            var entity = new Department
            {
                Name = dto.Name.Trim(),
                Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code!.Trim(),
                IsActive = true
            };

            _db.Departments.Add(entity);
            await _db.SaveChangesAsync();

            var read = new DepartmentReadDto(entity.Id, entity.Name, entity.Code, entity.IsActive);
            return Ok(read);
        }

        /// <summary>Update name/code. (Admin or HR)</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(DepartmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DepartmentReadDto>> Update(int id, [FromBody] DepartmentUpdateDto dto)
        {
            var d = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id);
            if (d is null) return NotFound();

            // Unique name check (excluding self)
            var nameExists = await _db.Departments
                .AnyAsync(x => x.Id != id && x.Name == dto.Name);
            if (nameExists) return BadRequest($"Department with name '{dto.Name}' already exists.");

            d.Name = dto.Name.Trim();
            d.Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code!.Trim();

            await _db.SaveChangesAsync();

            return Ok(new DepartmentReadDto(d.Id, d.Name, d.Code, d.IsActive));
        }

        /// <summary>Activate/deactivate a department (soft delete). (Admin or HR)</summary>
        [HttpPut("{id:int}/active")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(typeof(DepartmentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentReadDto>> SetActive(int id, [FromBody] DepartmentActiveDto dto)
        {
            var d = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id);
            if (d is null) return NotFound();

            d.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();

            return Ok(new DepartmentReadDto(d.Id, d.Name, d.Code, d.IsActive));
        }

        /// <summary>
        /// Hard delete a department if there are NO references; else 409 with counts.
        /// Recommended path is to deactivate instead.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,HR")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id);
            if (d is null) return NotFound();

            // Reference checks (current modules)
            var roleAssignments = await _db.DepartmentRoleAssignments.CountAsync(x => x.DepartmentId == id);
            var onboardings = await _db.OnboardingRequests.CountAsync(x => x.DepartmentId == id);

            // TODO: when you add these modules, include their checks here:
            // var assignments   = await _db.Assignments.CountAsync(x => x.DepartmentId == id);
            // var absences      = await _db.AbsenceRequests.CountAsync(x => x.DepartmentId == id);

            if (roleAssignments > 0 || onboardings > 0 /* || assignments > 0 || absences > 0 */)
            {
                return Conflict(new
                {
                    message = "Department has related data. Deactivate instead, or remove references first.",
                    roleAssignments,
                    onboardings,
                    // assignments,
                    // absences
                });
            }

            _db.Departments.Remove(d);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
