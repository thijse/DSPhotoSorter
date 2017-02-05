using System.Collections.Generic;

namespace PhotoSorter
{
    public class SameSizeGroups : Dictionary<long, SameSizeGroup>
    {
    }

    public class SameSizeGroup
    {
        public long Size { get; set; }
        public List<FileItem> Files { get; set; }

        public SameSizeGroup()
        {
            Files = new List<FileItem>();
        }
    }

    public class SameQuickHashGroups : Dictionary<string, SameQuickHashGroup>
    {
    }

    public class SameQuickHashGroup
    {
        //public long Size { get; set; }
        public string QuickHash { get; set; }
        public List<FileItem> Files { get; set; }

        public SameQuickHashGroup()
        {
            Files = new List<FileItem>();
        }
    }

    public class SameFullHashGroups : Dictionary<string, SameFullHashGroup>
    {
    }

    public class SameFullHashGroup
    {
        public string FullHash { get; set; }
        public List<FileItem> Files { get; set; }

        public SameFullHashGroup()
        {
            Files = new List<FileItem>();
        }
    }

    public class Duplicate
    {
        public List<FileItem> Items { get; set; }

        public Duplicate()
        {
            Items = new List<FileItem>();
        }
    }
}