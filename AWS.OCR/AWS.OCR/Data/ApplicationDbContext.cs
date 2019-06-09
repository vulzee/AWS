using System;
using System.Collections.Generic;
using System.Text;
using AWS.OCR.Data.Ocr;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AWS.OCR.Data
{
    public class ApplicationDbContext : IdentityDbContext, IDataProtectionKeyContext
    {
        public DbSet<OcrElement> OcrElements { get; set; }

        public DbSet<AwsAccess> AwsAccesses { get; set; }

		public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
