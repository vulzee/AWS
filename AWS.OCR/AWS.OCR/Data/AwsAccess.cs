using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWS.OCR.Data
{
    public class AwsAccess
    {
        public int Id { get; set; }
        public string AwsAccessKeyID { get; set; }      
        public string AwsSecreteAccessKey { get; set; } 
        public string Region { get; set; }              
        public string Token { get; set; }               
        public string S3BucketName {get;set;}    
    }
}
