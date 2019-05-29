using System;

namespace FFMpegWrapper
{
    public class MediaConverterOutputEventArgs : EventArgs
    {
        public MediaConverterOutputEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; }
    }


}
