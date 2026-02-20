using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebShop.Model;

namespace WebShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkersController : ControllerBase
    {
        private readonly WorkerModel _model;
        public WorkersController(WorkerModel model)
        {
            _model = model;
        }
    }
}
