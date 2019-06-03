using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWS.OCR
{
    public static class AwsAccess
    {
        public static string AwsAccessKeyID { get; set; }      
        public static string AwsSecreteAccessKey { get; set; } 
        public static string Region { get; set; }              
        public static string Token { get; set; }               
        public static string S3BucketName {get;set;}    
    }

    public class AwsAccessSetter
    {
        public string AwsAccessKeyID { get; set; }
        public string AwsSecreteAccessKey { get; set; }
        public string Token { get; set; }
    }
}
