using System;

namespace PhotoSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            var config           = new PhotoSorterConfig();
            var configSerializer = new ConfigurationSerializer<PhotoSorterConfig>("configuration.json", config);
            configSerializer.Deserialize();
            //PhotoSorterConfig.Create("configuration.json");
            //var config = PhotoSorterConfig.Load("configuration.json");

            //var sources = new[] {
            //    new PhotoSorter.PhotoSource() { Path = @"\\192.168.178.22\photo\Fotorol Thijs", Postfix = "_thijs" },
            //    new PhotoSorter.PhotoSource() { Path = @"\\192.168.178.22\photo\Fotorol Manou", Postfix = "_manou" }
            //};

            //var destination   = @"\\192.168.178.22\photo\Sorted\";
            //var duplicates    = @"\\192.168.178.22\photo\Duplicates\";
            //var configuration = @"\\192.168.178.22\photo\Configuration\";

            var photoSorter = new PhotoSorter(config.Sources, config.DestinationPath, config.DuplicatesPath, config.ConfigurationPath, config.MapRootFrom, config.MapRootTo);

            photoSorter.IndexAndCleanSorted();
            photoSorter.MoveToSorted();
            Console.WriteLine("Done");

        }
    }

    

}
