using AWS.OCR.Controllers;
using AWS.OCR.Data;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AWS.OCR.Data.Ocr;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;

namespace AWS.OCR.Tests
{
    public class OcrTests
    {
        private readonly AwsAccess awsAccess = new AwsAccess
        {
            Id = 1,
            AwsAccessKeyID = "fgdgdf",
            AwsSecreteAccessKey = "dfgdfgf",
            Region = "us-east-1",
            S3BucketName = "dgdfg",
            Token = "dfgdfgdfg",
        };
        private readonly OcrController _ocrController;
        private readonly ApplicationDbContext dbContext;
        private readonly Mock<UserManager<IdentityUser>> userManager;

        public OcrTests()
        {
            var ocrElem = new OcrElement { Id = 1, UserId = System.Guid.NewGuid().ToString() };

            var store = new Mock<IUserStore<IdentityUser>>();
            userManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(ocrElem.UserId);

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                           .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                           .Options;
            dbContext = new ApplicationDbContext(dbOptions);
            dbContext.AwsAccesses.Add(awsAccess);
            dbContext.OcrElements.Add(ocrElem);
            dbContext.SaveChanges();

            _ocrController = new OcrController(userManager.Object, dbContext);
            _ocrController.ControllerContext = new ControllerContext();
            _ocrController.ControllerContext.HttpContext = new DefaultHttpContext();
            _ocrController.TempData = new TempDataDictionary(_ocrController.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public void Edit_Should_Update_DbSet()
        {
            // arrange
            var text = "text";

            // act
            _ocrController.Edit(1, new OcrElement { Id = 1, OcrText = text }).GetAwaiter().GetResult();

            // asserts
            Assert.Equal(dbContext.OcrElements.FirstOrDefault(x => x.Id == 1).OcrText, text);
        }

        [Fact]
        public void DownloadText_Should_Return_File()
        {
            // arrange

            // act
            var result =  _ocrController.DownloadText(1).GetAwaiter().GetResult();

            // asserts
            Assert.True(result is FileStreamResult);
        }

        [Fact]
        public void Hisotry_Should_Return_ListOfRecognitions()
        {
            // arrange          

            // act
            var result = _ocrController.History();
            
            // asserts
            Assert.True(result is ViewResult);
            Assert.True((result as ViewResult).Model is List<OcrElement>);
        }
    }
}
