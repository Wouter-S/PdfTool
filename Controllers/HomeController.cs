using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PfdTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO.Compression;

namespace PfdTool.Controllers
{

    public class HomeController : Controller
    {

        [Route("")]
        public IActionResult Index()
        {
            return File("~/index.html", "text/html");
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (!file.ContentType.Equals("application/pdf", StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest();
            }
            if (file.Length > 0)
            {
                var filePath = GetFilePath(file.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }

            return Ok();
        }

        [HttpPost]
        [Route("combine")]
        public IActionResult Combine()
        {
            CombineDocs();
            return Ok();
        }

        [HttpPost]
        [Route("split")]
        public IActionResult Split()
        {
            var file = Directory.GetFiles(GetFilePath("")).First();
            FileInfo info = new FileInfo(file);
            var doc = PdfReader.Open(file, PdfDocumentOpenMode.Import);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            for (int i = 0; i < doc.PageCount; i++)
            {
                using (PdfDocument outPdf = new PdfDocument())
                {
                    outPdf.AddPage(doc.Pages[i]);
                    outPdf.Save(GetFilePath($"{ (i + 1).ToString("00") }.pdf", "processed"));
                    GetFilePath($"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf", "processed");
                }
            }

            return Ok();
        }

        [HttpGet]
        [Route("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "processed", fileName);
            var stream = System.IO.File.OpenRead(filePath);

            return File(stream, "application/pdf");
        }

        [HttpGet]
        [Route("downloadall")]
        public IActionResult DownloadAll()
        {
            string zipPath = string.Empty;
            FileStream fileStream = null;
            try
            {
                
                var filePath = Path.Combine(Environment.CurrentDirectory, "processed");
                zipPath = Path.Combine(Environment.CurrentDirectory, "zip", "processed.zip");

                if (System.IO.File.Exists(zipPath))
                {
                    System.IO.File.Delete(zipPath);

                }
                ZipFile.CreateFromDirectory(filePath, zipPath);

                fileStream = System.IO.File.OpenRead(zipPath);

                return File(fileStream, "application/zip");

            }
            finally
            {
                //if (fileStream != null) { fileStream.Close(); }

            }
        }

        [HttpDelete]
        [Route("{option}")]
        public IActionResult Delete(DeleteOption option)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, option.ToString());
            foreach (FileInfo file in new DirectoryInfo(filePath).GetFiles())
            {
                file.Delete();
            }
            return Ok();
        }

        [HttpGet]
        [Route("files")]
        [Route("home/files")]
        public IActionResult GetFiles()
        {
            var uploads = GetFiles(GetFilePath(""));
            var processed = GetFiles(GetFilePath("", "processed"));
            return Json(new { uploads, processed });
        }

        private dynamic GetFiles(string directory)
        {
            var files = Directory.GetFiles(directory);
            var fileInfos = files.Select(s => new System.IO.FileInfo(s)).Select(s => new
            {
                s.FullName,
                s.Name,
                s.CreationTime,
                s.Length
            }).OrderBy(s => s.Name);
            return fileInfos;
        }
        private void CombineDocs()
        {
            try
            {
                var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "uploads")).Select(f => new FileInfo(f));
                List<PdfDocument> documents = new List<PdfDocument>();

                foreach (string file in files.OrderBy(f => f.Name).Select(f => f.FullName))
                {
                    documents.Add(PdfReader.Open(file, PdfDocumentOpenMode.Import));
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (PdfDocument outPdf = new PdfDocument())
                {
                    foreach (var document in documents)
                    {
                        CopyPages(document, outPdf);
                    }

                    outPdf.Save(GetFilePath($"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf", "processed"));
                }
            }
            catch (Exception e)
            {

            }
        }
        private void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }
        private string GetFilePath(string fileName, string subDir = "uploads")
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, subDir)))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, subDir));
            }

            return Path.Combine(Environment.CurrentDirectory, subDir, fileName);
        }


    }
}
