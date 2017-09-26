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

            var photoSorter = new PhotoSorter(config.Sources, config.DestinationPath, config.DuplicatesPath, config.ConfigurationPath, config.MapRootFrom, config.MapRootTo);

            photoSorter.IndexAndCleanSorted();
            photoSorter.MoveToSorted();
            Console.WriteLine("Done");

        }
    }

    

}
