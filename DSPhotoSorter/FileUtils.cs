using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CopyFileFolderStructs;

namespace PhotoSorter
{
    public class FileUtils
    {

        public static bool MatchesFolder(string folderName, string folderWildcard, bool caseSensitive = false)
        {
            folderName = Path.GetDirectoryName(folderName);
            if (string.IsNullOrEmpty(folderName)) return false;
            var wildcard = new Wildcard(folderWildcard, caseSensitive);
            return (wildcard.IsMatch(folderName));
        }


        public static bool MatchesFolder(string folderName, string[] folderWildcards, bool caseSensitive = false)
        {
            return folderWildcards.Any(folder => MatchesFolder(folderName, folder, caseSensitive));
        }

        public static bool MatchesFile(string fileName, string fileWildcard, bool caseSensitive = false)
        {
            fileName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(fileName)) return false;
            var wildcard = new Wildcard(fileWildcard, caseSensitive);
            return (wildcard.IsMatch(fileName));
        }
        public static List<string> MatchesFilesInFolder(string folderName, string[] fileWildcards, bool caseSensitive = false)
        {
            var fileList = new List<string>();
            if (!Directory.Exists(folderName)) return fileList;
            fileList.AddRange(Directory.GetFiles(folderName).Where(fileName => MatchesFile(fileName, fileWildcards, caseSensitive)));
            return fileList;
        }

        public static bool MatchesFile(string fileName, string[] fileWildcards, bool caseSensitive = false)
        {
            return (fileWildcards.Any(file => MatchesFile(fileName, file, caseSensitive)));
        }

        public static string GetLastPath(string sourcePath)
        {
            var fullPath = Path.GetFullPath(sourcePath).TrimEnd(Path.DirectorySeparatorChar);
            return fullPath.Split(Path.DirectorySeparatorChar).Last();
        }

        public static string ChangeFileFolder(string sourcePath, string destinationFolder)
        {
            return Combine(destinationFolder, Path.GetFileName(sourcePath));
        }


        /// <summary>
        /// There are a few interesting combinations of what will and won't combine correctly for Path.Combine, 
        /// and the MSDN page for Path.Combine explains some of these. There's one condition it won't cater for 
        /// properly though - if the 2nd parameter contains a leading '\', for instance '\file.txt', the final 
        /// result will ignore the first parameter. The output of Path.Combine("c:\directory", "\file.txt") is \file.txt'. 
        /// This is not the case if the 1st parameter contains a trailing '\'. 
        ///
        ///string part1 = @"c:\directory";
        ///string part2 = @"file.txt";
        ///
        ///(1) Console.WriteLine(Path.Combine(part1, part2));
        ///(2) Console.WriteLine(Path.Combine(part1 + @"\", part2));
        ///(3) Console.WriteLine(Path.Combine(part1 + @"\", @"\" + part2));
        ///(4) Console.WriteLine(Path.Combine(part1, @"\" + part2));
        ///
        ///The output of this is
        ///
        ///(1) c:\directory\file.txt
        ///(2) c:\directory\file.txt
        ///(3) \file.txt
        ///(4) \file.txt
        ///
        /// The FileUtils.Combine function will give the output
        ///
        ///(1) c:\directory\file.txt
        ///(2) c:\directory\file.txt
        ///(3) c:\directory\file.txt
        ///(4) c:\directory\file.txt 
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            var a = path1;
            var b = path2.Substring(0, 1) == @"\" ? path2.Substring(1, path2.Length - 1) : path2;
            var c = Path.Combine(a, b);
            return Path.Combine(path1, path2.Substring(0, 1) == @"\" ? path2.Substring(1, path2.Length - 1) : path2);
        }

        public static string Combine(string[] paths)
        {
            if (paths.Length == 0) return "";
            if (paths.Length == 1) return paths[0];

            var combinedPath = paths[0];

            return paths.Aggregate(combinedPath, Combine);
        }

        public static string GetBaseName(string path)
        {
            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            var baseName = Path.GetFileName(fullPath);
            return baseName;
        }


        public static string[] GetDirectories(string path)
        {
            path = path.Substring(Path.GetPathRoot(path).Length);
            var pathSeparators = new string[] { "\\" };
            var dirs = path.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);
            return dirs;
        }

        public static void Empty(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            foreach (FileInfo file in dirInfo.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in dirInfo.GetDirectories()) subDirectory.Delete(true);
        }
    }
}
