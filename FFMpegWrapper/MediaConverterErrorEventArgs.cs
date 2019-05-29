using System;

namespace FFMpegWrapper
{
    public class MediaConverterErrorEventArgs : EventArgs
    {
        public MediaConverterErrorEventArgs(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; }
    }


}
