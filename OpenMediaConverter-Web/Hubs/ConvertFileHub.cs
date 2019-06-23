using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace OpenMediaConverterWeb.Hubs
{
    public interface IConvertFileHub
    {
        Task ReceiveConvertProgress(float progress, string message);

        Task ReceiveConvertError(string error);

        Task ReceiveConvertComplete(string link);
    }

    public class ConvertFileHub : Hub<IConvertFileHub>
    {
        public async Task SendProgress(float progress, string message)
        {
            await Clients.All.ReceiveConvertProgress(progress, message);
        }
    }
}
