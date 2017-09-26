#region DS Photosorter - MIT - (c) 2017 Thijs Elenbaas.
/*
  DS Photosorter - tool that processes photos captured with Synology DS Photo

  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:

  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.

  Copyright 2017 - Thijs Elenbaas
*/
#endregion

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