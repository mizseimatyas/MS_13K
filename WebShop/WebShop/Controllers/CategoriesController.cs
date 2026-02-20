using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CartModel _model;
        public CategoriesController(CartModel model)
        {
            _model = model;
        }
    }
}
