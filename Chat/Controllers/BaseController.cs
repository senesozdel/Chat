using Chat.Entities;
using Microsoft.AspNetCore.Mvc;

public class BaseController<TEntity> : ControllerBase where TEntity : BaseEntity
{
    private readonly BaseService<TEntity> _service;

    public BaseController(BaseService<TEntity> service)
    {
        _service = service;
    }

    [HttpGet]
    public virtual async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public virtual async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null)
            return NotFound(new { message = $"Entity with ID {id} not found." });

        return Ok(entity);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TEntity entity)
    {
        if (entity == null)
            return BadRequest(new { message = "Invalid entity data." });

        entity.Id = await _service.GetNextSequenceValue(typeof(TEntity).Name);
        await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Update(int id, [FromBody] TEntity updatedEntity)
    {
        var success = await _service.UpdateAsync(id, updatedEntity);
        if (!success)
            return NotFound(new { message = $"Entity with ID {id} not found." });

        return Ok(new { message = "Entity updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = $"Entity with ID {id} not found." });

        return Ok(new { message = "Entity deleted successfully." });
    }
}
