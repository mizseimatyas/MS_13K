using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly CartModel _model;
        public CartsController(CartModel model)
        {
            _model = model;
        }
    }
}
