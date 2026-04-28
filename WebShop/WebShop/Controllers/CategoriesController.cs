using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebShop.Dto;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryModel _model;
        public CategoriesController(CategoryModel model)
        {
            _model = model;
        }

        #region AllCategories
        [HttpGet("allcategories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> AllCategories()
        {
            try
            {
                var response = await _model.AllCategories();
                return Ok(response);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region AddNewCategory
        [HttpPost("addnewcategory")]
        public async Task<ActionResult> AddNewCategory([FromQuery] string categ)
        {
            try
            {
                await _model.AddNewCategory(categ);
                return Ok();
            }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region ModifyCategory
        [HttpPut("modifycategory")]
        public async Task<ActionResult> ModifyCategory([FromBody] ModifyCategoryDto dto)
        {
            try
            {
                await _model.ModifyCategory(dto);
                return Ok();
            }
            catch (ArgumentException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion

        #region DeleteCategory
        [HttpDelete("deletecategory")]
        public async Task<ActionResult> DeleteCategory([FromQuery] int categid)
        {
            try
            {
                await _model.DeleteCategory(categid);
                return Ok();
            }
            catch (ArgumentOutOfRangeException) { return StatusCode(StatusCodes.Status406NotAcceptable); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
        #endregion
    }
}