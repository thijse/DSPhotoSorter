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
            var photoSorter = new PhotoSorter(@"\\nas\shared\Media\Photo");

            //\\nas\shared\Media\Photo\test
            photoSorter.ShowDirectory(@"\\nas\shared\Media\Photo\Fotorol Thijs");

            // \\nas\shared\Media\Photo\Fotorol Manou
        }
    }

    

}
