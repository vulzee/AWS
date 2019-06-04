using AWS.OCR.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace AWS.OCR.Controllers
{
    public class AdminController : Controller
    {
        private string password = "zaq1@WSX";
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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
        public async Task<IActionResult> UpdateTokens(AwsAccess model)
        {
            var awsAccess = _dbContext.AwsAccesses.FirstOrDefault() ?? new AwsAccess();
           
            awsAccess.AwsAccessKeyID = model.AwsAccessKeyID;
            awsAccess.AwsSecreteAccessKey = model.AwsSecreteAccessKey;
            awsAccess.Token = model.Token;

            if (awsAccess.Id != 0)
            {
                _dbContext.Update(awsAccess);
            }
            else
            {
                _dbContext.Add(awsAccess);
            }
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
