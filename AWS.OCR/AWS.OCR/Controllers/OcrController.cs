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
using AWS.OCR.Data.Ocr;

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

            if (result == null)
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

            System.IO.File.Delete(result.ImageFilenamePath);
            dbContext.OcrElements.Remove(result);
            await dbContext.SaveChangesAsync();

            TempData["ResultMessage"] = "File removed succesfully.";
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
    }
}
