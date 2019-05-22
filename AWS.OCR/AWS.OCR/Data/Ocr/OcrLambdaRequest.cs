using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWS.OCR.Data.Ocr
{
	public class OcrLambdaRequest
	{
		public string Image64 { get; set; }

		public OcrLambdaRequest(string image64)
		{
			this.Image64 = image64;
		}
	}
}
