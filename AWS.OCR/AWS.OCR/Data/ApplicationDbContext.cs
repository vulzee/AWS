using System;
using System.Collections.Generic;
using System.Text;
using AWS.OCR.Data.Ocr;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AWS.OCR.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<OcrElement> OcrElements { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
