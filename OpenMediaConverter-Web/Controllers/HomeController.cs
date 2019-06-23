using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFMpegWrapper;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OpenMediaConverter_Web.Models;
using OpenMediaConverterWeb;
using OpenMediaConverterWeb.Hubs;

namespace OpenMediaConverter_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<ConvertSettings> _convertSettings;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IHubContext<ConvertFileHub, IConvertFileHub> hubContext;

        public HomeController(IOptions<ConvertSettings> convertSettings, IHostingEnvironment hostingEnvironment, IHubContext<ConvertFileHub, IConvertFileHub> hubContext)
        {
            this._convertSettings = convertSettings;
            this.hostingEnvironment = hostingEnvironment;
            this.hubContext = hubContext;
        }
        public IActionResult Index(string file)
        {
            if (file != null)
            {
                string filePath = Path.Combine("~/uploads/", file);
                return File(filePath, "application/octet-stream", file);
            }
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("UploadFiles")]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        public async Task<IActionResult> UploadFile(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            string filePath = null;

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    filePath = Path.GetTempFileName() + "."+ Path.GetExtension(formFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    break;
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            BackgroundJob.Enqueue(() => ConvertAndRemoveAsync(filePath));

            return RedirectToAction(nameof(Index));
        }

        string UploadPath => Path.Combine(this.hostingEnvironment.WebRootPath, "uploads");
        public async Task ConvertAndRemoveAsync(string sourceFilePath)
        {
            bool containsError = DateTime.Now.Ticks % 20 == 0;
            try
            {
                string fileName = Guid.NewGuid().ToString("n") + ".mp4";
                Directory.CreateDirectory(UploadPath);
                var outputFile = Path.Combine(UploadPath, fileName);
                var ffmpegPath = _convertSettings.Value.FFmpegPath;
                var mediaConverter = new MediaConverter(ffmpegPath);
                mediaConverter.Progress += async(s, e) =>
                  {
                      await hubContext.Clients.All
                        .ReceiveConvertProgress((float)(e.Duration.TotalMilliseconds/e.AboutTotal.TotalMilliseconds), string.Empty);
                  };
                await mediaConverter.ConvertAsync(sourceFilePath, outputFile, CancellationToken.None);

                //for (int i = 0; i < 100; i++)
                //{
                //    float progressValue = i ;
                //    string message = "mes";

                //    await hubContext.Clients.All
                //        .ReceiveConvertProgress(progressValue, message);
                //    await Task.Delay(50);

                //}
                var outputPath = $"?file={fileName}";
                await hubContext.Clients.All.ReceiveConvertComplete(outputPath);
            }
            catch (Exception ex)
            {
                await hubContext.Clients.All.ReceiveConvertError(ex.Message);
            }

            System.IO.File.Delete(sourceFilePath);
        }
    }
}