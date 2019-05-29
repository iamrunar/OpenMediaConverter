using System;

namespace FFMpegWrapper
{
    public class MediaConverterProgressEventArgs : EventArgs
    {
        public MediaConverterProgressEventArgs(TimeSpan duration, TimeSpan total)
        {
            Duration = duration;
            AboutTotal = total;
        }

        /// <summary>
        /// Текущее время видео-ряда.
        /// </summary>
        /// <value>The duration.</value>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Всего времени видео-ряда.
        /// </summary>
        /// <value>The total.</value>
        public TimeSpan AboutTotal { get; }
    }


}
