using Microsoft.AspNetCore.Mvc;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Interfaces;

namespace WalletWise.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService category)
        {
            _categoryService = category;
        }

        [HttpGet]
        public async Task<ActionResult<CategoryResponseDto>> GetAllCategory()
        {
            var result = await _categoryService.GetAllCategoriesAsync();

            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategoryById(int id)
        {
            var response = await _categoryService.GetCategoryByIdAsync(id);

            if (response.Value is null)
            {
                return NotFound();
            }

            return Ok(response.Value);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> CreateCategory([FromBody] CreateCategoryRequestDto requestDto)
        {
            var response = await _categoryService.CreateCategoryAsync(requestDto);

            return CreatedAtAction(nameof(GetCategoryById), new { id = response.Value.Id }, response);
        }

        [HttpPut("{id}")]
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
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var response = await _categoryService.DeleteCategoryAsync(id);

            if(response is null)
            {
                return NotFound();
            }

            return NoContent();

        }

    }
}
