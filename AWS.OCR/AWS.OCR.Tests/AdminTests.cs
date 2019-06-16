using AWS.OCR.Controllers;
using AWS.OCR.Data;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AWS.OCR.Tests
{
    public class AdminTests
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
        private readonly AdminController _adminController;
        private readonly ApplicationDbContext dbContext;
        private readonly Mock<UserManager<IdentityUser>> userManager;

        public AdminTests()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                           .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                           .Options;
            dbContext = new ApplicationDbContext(dbOptions);
            dbContext.AwsAccesses.Add(awsAccess);
            dbContext.SaveChanges();

            _adminController = new AdminController(dbContext);
        }

        [Fact]
        public void UpdateTokens_Should_Update_AwsAccessEntity()
        {
            // arrange
            var awsAccess = new AwsAccess { Token = "new token", AwsAccessKeyID = "new key", AwsSecreteAccessKey = "secret" };

            // act
            _adminController.UpdateTokens(awsAccess).GetAwaiter().GetResult();

            // asserts
            var entity = dbContext.AwsAccesses.FirstOrDefault(x => x.Id == 1);
            Assert.Equal(entity.Token, awsAccess.Token);
            Assert.Equal(entity.AwsSecreteAccessKey, awsAccess.AwsSecreteAccessKey);
            Assert.Equal(entity.AwsAccessKeyID, awsAccess.AwsAccessKeyID);
        }
    }
}
