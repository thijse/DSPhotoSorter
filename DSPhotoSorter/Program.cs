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
                    path    = @"\\nas\shared\Media\Photo\Fotorol Thijs", postfix = "_thijs"
                }
            };

            var destination = @"\\nas\shared\Media\Photo\Sorted";
            var duplicates  = @"\\nas\shared\Media\Photo\Duplicates";

            var photoSorter = new PhotoSorter(sources,destination,duplicates);

            //\\nas\shared\Media\Photo\test
            //photoSorter.ShowDirectory(@"\\nas\shared\Media\Photo\Fotorol Thijs");

            photoSorter.RemoveDuplicatesFromSorted();
            photoSorter.MoveToSorted();

            // \\nas\shared\Media\Photo\Fotorol Manou
        }
    }

    

}
