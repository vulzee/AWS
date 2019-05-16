using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AWS.OCR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Identity;
using AWS.OCR.Data;

namespace AWS.OCR.Controllers
{
    [Authorize]
    public class OcrController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext dbContext;

        public OcrController(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult History()
        {
            ViewData["Message"] = "History of your recognitions.";
            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId).ToList();

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        file.FileName);
            var userId = userManager.GetUserId(HttpContext.User);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await dbContext.OcrElements.AddAsync(new Data.Ocr.OcrElement
            {
                ImageFileContentType = file.ContentType,
                ImageFilename = file.FileName,
                ImageFilenamePath = path,
                OcrText = "tekst",
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

            if(result == null)
                return Content("File not found");

            var memory = new MemoryStream();
            using (var stream = new FileStream(result.ImageFilenamePath, FileMode.Open))
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
            using (var stream = new StreamWriter(memory))
            {
                await stream.WriteAsync(result.OcrText);
                await stream.FlushAsync();
            }
            memory.Position = 0;
            return File(memory, "text/plain", Path.ChangeExtension(result.ImageFilename, "txt"));
        }

        public async Task<IActionResult> Delete(int id)
        {

            if (id == 0)
            {
                ViewBag.ResultMessage = "File not found";
                return RedirectToAction("History");
            }

            var userId = userManager.GetUserId(HttpContext.User);
            var result = dbContext.OcrElements.Where(x => x.UserId == userId && x.Id == id).FirstOrDefault();

            if (result == null)
            {
                ViewBag.ResultMessage = "File not found";
                return RedirectToAction("History");
            }

            dbContext.OcrElements.Remove(result);
            await dbContext.SaveChangesAsync();

            ViewBag.ResultMessage = "File removed succesfully.";
            return RedirectToAction("History");
        }

        public async Task<IActionResult> Edit(int id)
        {

            //if (id == 0)
            //    return Content("File not found");

            //var userId = userManager.GetUserId(HttpContext.User);
            //var result = dbContext.OcrElements.Where(x => x.UserId == userId && x.Id == id).FirstOrDefault();

            //if (result == null)
            //    return Content("File not found");

            //dbContext.OcrElements.Update(result);
            //await dbContext.SaveChangesAsync();

            return Content("File saved succesfully.");
        }
    }
}
