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
    public class PhotoSorterConfig
    {
        public string DestinationPath                { get; set; }
        public string DuplicatesPath                 { get; set; }
        public string ConfigurationPath              { get; set; }
        public List<PhotoSorter.PhotoSource> Sources { get; set; }
        public List<string> AlternativeRootFolders   { get; set; }
        public string MapRootFrom                    { get; set; }
        public string MapRootTo                      { get; set; }


        public PhotoSorterConfig()
        {
            DestinationPath   = @"\\NAS\photo\Sorted\"       ;
            DuplicatesPath    = @"\\NAS\photo\Duplicates\"   ;
            ConfigurationPath = @"\\NAS\photo\Configuration\";
            MapRootFrom       = "";
            MapRootTo         = "";


            Sources = new List<PhotoSorter.PhotoSource>()
            {
                new PhotoSorter.PhotoSource() {Path = @"\\NAS\photo\Camera Roll Name1", Postfix = "_name1"},
            };
        }


    }
}
