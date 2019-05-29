using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace OpenMediaConverter_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:8899","http://localhost:5000")
                .UseStartup<Startup>();
    }
}
