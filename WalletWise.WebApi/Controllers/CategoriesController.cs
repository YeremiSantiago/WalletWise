using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Interfaces;

namespace WalletWise.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/categories")]
    [EnableRateLimiting("AuthenticatedUserApi")]
    [SwaggerTag("Proporciona operaciones CRUD para administrar categorías de billetera.")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService category)
        {
            _categoryService = category;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener todas las categorias",
            Description = "Te devuelve todas las categorias existestes")]
        [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAllCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();

            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtener una categoria por Id",
            Description = "Devuelve la categoria registrada asociada al identificador proporcionado, si existe.")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryResponseDto>> GetCategoryById(int id)
        {
            var response = await _categoryService.GetCategoryByIdAsync(id);

            if (response.Value is null)
            {
                return NotFound();
            }

            return Ok(response.Value);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear categoria",
            Description = "Permite crear categorias y te la devuelve si fue creada exitosamente")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CategoryResponseDto>> CreateCategory([FromBody] CreateCategoryRequestDto requestDto)
        {
            var response = await _categoryService.CreateCategoryAsync(requestDto);

            if (!response.IsSuccess || response.Value is null)
            {
                return UnprocessableEntity(new { error = response.Error });
            }

            return CreatedAtAction(nameof(GetCategoryById), new { id = response.Value.Id }, response.Value);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Actualizar categoria existente",
            Description = "Te permite actualizar una categoria existente y te la devuelve con los datos actualizados")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequestDto requestDto)
        {
            var response = await _categoryService.UpdateCategoryAsync(id, requestDto);

            if (response.Value is null)
            {
                return NotFound();
            }

            return Ok(response.Value);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Borrar categoria existente",
            Description = "Permite borrar una categoria existente si no esta registrada en una transaccion")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var response = await _categoryService.DeleteCategoryAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return NoContent();

        }

    }
}
