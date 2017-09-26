using System.Collections.Generic;
using System.IO;

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

            AlternativeRootFolders = new List<string>()
            {
                @"\\NAS\",
                @"\\192.168.1.1",
            };
        }


    }
}
