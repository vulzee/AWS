using Microsoft.AspNetCore.Mvc;

namespace AWS.OCR.Controllers
{
    public class AdminController : Controller
    {
        private string password = "zaq1@WSX";

        [HttpGet]
        [Route("/it/is/panel")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/it/is/panel")]
        public IActionResult Index([FromForm] string pass)
        {
            ViewData["password"] = string.Equals(pass, password);

            return View();
        }

        [HttpPost]
       // [Route("/Admin/UpdateTokens")]
        public IActionResult UpdateTokens(AwsAccessSetter model)
        {
            AwsAccess.AwsAccessKeyID = model.AwsAccessKeyID;
            AwsAccess.AwsSecreteAccessKey = model.AwsSecreteAccessKey;
            AwsAccess.Token = model.Token;
            
            return RedirectToAction("Index");
        }
    }
}
