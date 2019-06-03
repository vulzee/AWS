using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Identity;
using AWS.OCR.Data;
using AWS.OCR.Data.Ocr;
using Amazon.Lambda;
using Amazon;
using Amazon.Lambda.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace AWS.OCR.Controllers
{
    [Authorize]
    public class OcrController : Controller, IDisposable
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext dbContext;
        private AmazonLambdaClient awsLambdaClient;
        private AmazonS3Client awsS3Client;
        private TransferUtility fileTransferUtility;

        public OcrController(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
			this.InitializeAwsServices();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult History()
        {
            ViewData["Message"] = "History of your recognitions.";
            ViewData["ResultMessage"] = TempData["ResultMessage"]?.ToString();
            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId).ToList();

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var userId = userManager.GetUserId(HttpContext.User);
            var fileKey = $"{userId}_{file.FileName}";
            using (var stream = file.OpenReadStream())
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileKey,
                    BucketName = AwsAccess.S3BucketName,
                };
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            var text = await this.Recognize(file);	

			await dbContext.OcrElements.AddAsync(new Data.Ocr.OcrElement
            {
                ImageFileContentType = file.ContentType,
                ImageFilename = file.FileName,
                ImageFilenamePath = fileKey,
                OcrText = text,
                UserId = userId,
            });
            await dbContext.SaveChangesAsync();

            return RedirectToAction("History");
        }

        public async Task<IActionResult> Download(int id)
        {

            if (id == 0)
                return Content("File not found");

            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId && x.Id == id).FirstOrDefault();

            if (result == null)
                return Content("File not found");

            var memory = new MemoryStream();
            var downloadRequest = new TransferUtilityOpenStreamRequest
            {
                Key = result.ImageFilenamePath,
                BucketName = AwsAccess.S3BucketName,
            };
            using (var stream = fileTransferUtility.OpenStream(downloadRequest))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, result.ImageFileContentType, result.ImageFilename);
        }

        public async Task<IActionResult> DownloadText(int id)
        {

            if (id == 0)
                return Content("File not found");

            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId && x.Id == id).FirstOrDefault();

            if (result == null)
                return Content("File not found");

            var memory = new MemoryStream();
            var stream = new StreamWriter(memory);
            await stream.WriteAsync(result.OcrText);
            await stream.FlushAsync();
            memory.Position = 0;

            return File(memory, "text/html", Path.ChangeExtension(result.ImageFilename, "html"));
        }

        public async Task<IActionResult> Delete(int id)
        {

            if (id == 0)
            {
                TempData["ResultMessage"] = "File not found";
                return RedirectToAction("History");
            }

            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId && x.Id == id).FirstOrDefault();

            if (result == null)
            {
                TempData["ResultMessage"] = "File not found";
                return RedirectToAction("History");
            }

            var response = await awsS3Client.DeleteObjectAsync(AwsAccess.S3BucketName, result.ImageFilenamePath);
            if(response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                dbContext.OcrElements.Remove(result);
                await dbContext.SaveChangesAsync();
                TempData["ResultMessage"] = "File removed succesfully.";
                return RedirectToAction("History");
            }

            TempData["ResultMessage"] = "Error during file remove request.";
            return RedirectToAction("History");           
        }

        public IActionResult Edit(int id)
        {

            if (id == 0)
            {
                TempData["ResultMessage"] = "File not found";
                return RedirectToAction("History");
            }

            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId && x.Id == id).FirstOrDefault();

            if (result == null)
            {
                TempData["ResultMessage"] = "File not found";
                return RedirectToAction("History");
            }

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, OcrElement input)
        {
            if (id == 0)
            {
                TempData["ResultMessage"] = "File not found";
                return RedirectToAction("History");
            }

            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId && x.Id == id).FirstOrDefault();

            if (result == null)
            {
                TempData["ResultMessage"] = "File not found";
                return RedirectToAction("History");
            }

            result.OcrText = input.OcrText;
            dbContext.OcrElements.Update(result);
            await dbContext.SaveChangesAsync();

            TempData["ResultMessage"] = "Success";
            return RedirectToAction("History");
        }

		private void InitializeAwsServices()
		{
			var region = RegionEndpoint.GetBySystemName(AwsAccess.Region);

            this.awsS3Client = new AmazonS3Client(AwsAccess.AwsAccessKeyID, AwsAccess.AwsSecreteAccessKey, AwsAccess.Token, region);
            this.fileTransferUtility = new TransferUtility(awsS3Client);
            this.awsLambdaClient = new AmazonLambdaClient(AwsAccess.AwsAccessKeyID, AwsAccess.AwsSecreteAccessKey, AwsAccess.Token, region);
		}

		private string ConvertImageToBase64(IFormFile file)
		{
			using (var ms = new MemoryStream())
			{
				file.CopyTo(ms);
				var fileBytes = ms.ToArray();
				return Convert.ToBase64String(fileBytes);
			}
		}

		private async Task<string> Recognize(IFormFile file)
		{
			var lambdaRequest = new OcrLambdaRequest(this.ConvertImageToBase64(file));

			var payload = JsonConvert.SerializeObject(lambdaRequest, new JsonSerializerSettings
			{
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				}
			});

			InvokeRequest ir = new InvokeRequest
			{
				FunctionName = "x1",
				InvocationType = InvocationType.RequestResponse,
				Payload = payload
			};

			InvokeResponse response = await this.awsLambdaClient.InvokeAsync(ir);

			using (var sr = new StreamReader(response.Payload))
			{
				JsonReader reader = new JsonTextReader(sr);
				var serilizer = new JsonSerializer();
				var op = serilizer.Deserialize(reader);
				var text = op as string;
				//TODO add proper error handling
				if(text != null)
				{
					return text;
				}
				else
				{
					throw new Exception("Something went wrong!");
				}
			}
		}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            awsLambdaClient.Dispose();
            fileTransferUtility.Dispose();
            awsS3Client.Dispose();
        }
    }
}
