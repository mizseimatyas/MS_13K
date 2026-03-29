using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebShop.Dto;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ItemModel _model;
        public ItemsController(ItemModel model)
        {
            _model = model;
        }

        #region AllItems
        [HttpGet("allitems")]
        public async Task<ActionResult<IEnumerable<AdminItemDto>>> AllItems()
        {
            try
            {
                var response = await _model.AllItems();
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemById
        [HttpGet("itembyid")]
        public async Task<ActionResult<AllItemDto>> ItemById([FromQuery] int id)
        {
            try
            {
                var response = await _model.ItemById(id);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region AdmItemByName

        [HttpGet("admitembyname")]
        public async Task<ActionResult<AllItemDto>> AdmItemByName([FromQuery]string iname)
        {
            try
            {
                var response = await _model.AdmItemByName(iname);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }


        #endregion

        #region ItemsWithQuantity0
        [Authorize(Roles = "Worker")]
        [HttpGet("itemswithquantity0")]
        public async Task<ActionResult<IEnumerable<AllItemDto>>> ItemsQuantityZero()
        {
            try
            {
                var response = await _model.ItemsWithQunatity0();
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsQuantityOrderByLowestFirst
        [Authorize(Roles = "Worker")]
        [HttpGet("itemsquantityasc")]
        public async Task<ActionResult<IEnumerable<AllItemDto>>> ItemsByQuantityAsc()
        {
            try
            {
                var response = await _model.ItemsWithQuantityOrderByAsc();
                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsQuantityOrderByHighestFirst
        [Authorize(Roles = "Worker")]
        [HttpGet("itemsquantitydesc")]
        public async Task<ActionResult<IEnumerable<AllItemDto>>> ItemsByQuantityDesc()
        {
            try
            {
                var response = await _model.ItemsWithQuantityOrderByDesc();
                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsQuantityInCategoryOrderByLowestFirst
        [Authorize(Roles = "Worker")]
        [HttpGet("itemsquantityincategoryasc")]
        public async Task<ActionResult<IEnumerable<AllItemDto>>> CategoryItemsQuantityAsc([FromQuery] string category)
        {
            try
            {
                var response = await _model.CategoryItemsQuantityOrderByAsc(category);
                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsQuantityInCategoryOrderByHighestFirst
        [Authorize(Roles = "Worker")]
        [HttpGet("itemsquantityincategorydesc")]
        public async Task<ActionResult<IEnumerable<AllItemDto>>> CategoryItemsQuantityDesc([FromQuery] string category)
        {
            try
            {
                var response = await _model.CategoryItemsQuantityOrderByDesc(category);
                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region AddNewItem
        [Authorize(Roles = "Worker,Admin")]
        [HttpPost("addnewitem")]
        public async Task<ActionResult> AddNewItem([FromBody] AddNewItemDto dto)
        {
            try
            {
                await _model.AddNewItem(dto);
                return Ok();
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException)
            {
                return Conflict();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ModifyItem
        [HttpPut("modifyitem")]
        public async Task<ActionResult> ModifyItem([FromBody] ModifyItemDto dto)
        {
            try
            {
                await _model.ModifyItem(dto);
                return Ok();
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException)
            {
                return Conflict();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region DeleteItem
        [Authorize(Roles = "Worker")]
        [HttpDelete("deleteitem")]
        public async Task<ActionResult> DeleteItem([FromQuery] int id)
        {
            try
            {
                await _model.DeleteItem(id);
                return Ok();
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemByName
        [HttpGet("itembyname")]
        public async Task<ActionResult<ItemDto>> ItemByName([FromQuery] string name)
        {
            try
            {
                var response = await _model.ItemByName(name);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemByNameFragment
        [HttpGet("itemnamebyfragment")]
        public async Task<ActionResult<IEnumerable<SearchItemsByDto>>> ItemsByNameFragment([FromQuery] string fragname)
        {
            try
            {
                var response = await _model.ItemsByNameSnipet(fragname);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsInCategory
        [HttpGet("itemsincategory")]
        public async Task<ActionResult<IEnumerable<SearchItemsByDto>>> ItemsInCategory([FromQuery] string category)
        {
            try
            {
                var response = await _model.ItemsByCategory(category);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsByPriceMax
        [HttpGet("itemsbypricemax")]
        public async Task<ActionResult<IEnumerable<SearchItemsByPriceDto>>> ItemsByMaxPrice([FromQuery] int max)
        {
            try
            {
                var response = await _model.ItemsByPriceMax(max);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsByPriceMin
        [HttpGet("itemsbypricemin")]
        public async Task<ActionResult<IEnumerable<SearchItemsByPriceDto>>> ItemsByMinPrice([FromQuery] int min)
        {
            try
            {
                var response = await _model.ItemsByPriceMin(min);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion

        #region ItemsInPriceRange
        [HttpGet("itemsinpricerange")]
        public async Task<ActionResult<IEnumerable<SearchItemsByPriceDto>>> ItemsInPriceRange([FromQuery] int min, [FromQuery] int max)
        {
            try
            {
                var response = await _model.ItemsByPriceMinMax(min, max);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        #endregion


    }
}
