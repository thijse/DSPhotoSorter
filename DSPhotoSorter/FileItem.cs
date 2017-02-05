using System;

namespace PhotoSorter
{
    public class FileItem
    {
        public string FileName { get; set; }
        public DateTime ModifiedTime { get; set; }

        // Determine uniqueness
        public long Size { get; set; }
        public string QuickHash { get; set; }
        public string FullHash { get; set; }
    }
}