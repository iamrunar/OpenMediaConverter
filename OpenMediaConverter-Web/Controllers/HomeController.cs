using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFMpegWrapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenMediaConverter_Web.Models;
using OpenMediaConverterWeb.ApplicationCore;

namespace OpenMediaConverter_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<ConvertSettings> _convertSettings;
        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(IOptions<ConvertSettings> convertSettings, IHostingEnvironment hostingEnvironment)
        {
            this._convertSettings = convertSettings;
            this.hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
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

        public async Task<IActionResult> Convert()
        {
            var ffmpegPath = _convertSettings.Value.FFmpegPath;
            var mediaConverter =  new MediaConverter(ffmpegPath);
            var sourcePath = Path.Combine(hostingEnvironment.WebRootPath, "source.webm");
            var destPath = Path.Combine(hostingEnvironment.WebRootPath, "dest.mp4");
            await mediaConverter.ConvertAsync(sourcePath, destPath, CancellationToken.None);
            return RedirectToAction(nameof(Index));
        }
    }
}
