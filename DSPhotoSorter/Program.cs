using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSorter
{
    class Program
    {
        static void Main(string[] args)
        {


            var sources    = new [] {
                new PhotoSorter.photoSource()
                {
                    path    = @"\\nas\shared\Media\Photo\Fotorol Manou", postfix = "_manou"
                }
            };

            var destination = @"\\nas\shared\Media\Photo\Sorted";
            var duplicates  = @"\\nas\shared\Media\Photo\Duplicates";

            var photoSorter = new PhotoSorter(sources,destination,duplicates);

            //\\nas\shared\Media\Photo\test
            //photoSorter.ShowDirectory(@"\\nas\shared\Media\Photo\Fotorol Thijs");

            photoSorter.IndexAndCleanSorted();
            photoSorter.MoveToSorted();
            Console.WriteLine("Done");
            // \\nas\shared\Media\Photo\Fotorol Manou
        }
    }

    

}
