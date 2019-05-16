using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AWS.OCR.Data.Ocr
{
    public class OcrElement
    {
        public int Id { get; set; }
        public string ImageFilename { get; set; }
        public string ImageFileContentType { get; set; }
        public string ImageFilenamePath { get; set; }

       // [AllowHtml]
        [UIHint("tinymce_jquery_full")]
        public string OcrText { get; set; }
        public string UserId { get; set; }
    }
}
