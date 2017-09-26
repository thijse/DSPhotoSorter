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

        public static string ChangeFileFolder(string sourcePath, string sourceBase, string destinationFolder)
        {
            var sourcePathAbs = Path.GetFullPath(sourcePath);
            var sourceBaseAbs = Path.GetFullPath(sourceBase);

            if (sourcePathAbs.Substring(0, sourceBaseAbs.Length) != sourceBaseAbs) return null;

            var temp = sourcePathAbs.Remove(0, sourceBaseAbs.Length);
            var destPath = Combine(destinationFolder, temp);
            return destPath;
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

        public static void DeleteDirectoryContents(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.GetFiles()) file.Delete();
            foreach (var subDirectory in dirInfo.GetDirectories()) subDirectory.Delete(true);
        }

        public static string GetAbsolutePath(string relativePath, string basePath)
        {
            if (relativePath == null) return null;
            basePath = basePath == null ? Path.GetFullPath(".") : GetAbsolutePath(basePath, null);
            string path;
            // specific for windows paths starting on \ - they need the drive added to them.
            // I constructed this piece like this for possible Mono support.
            if (!Path.IsPathRooted(relativePath) || "\\".Equals(Path.GetPathRoot(relativePath)))
            {
                var basePathRoot = Path.GetPathRoot(basePath)??"";
                path = relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()) ? Path.Combine(basePathRoot, relativePath.TrimStart(Path.DirectorySeparatorChar)) : Path.Combine(basePath, relativePath);
            }
            else
                path = relativePath;
            // resolves any internal "..\" to get the true full Path.
            return Path.GetFullPath(path);
        }

        public static System.Collections.Generic.IEnumerable<string> RecurseFilesInDirectories(string root)
        {
            // Data structure to hold names of sub-folders to be
            // examined for files.
            Stack<string> dirs = new Stack<string>(20);

            if (!System.IO.Directory.Exists(root))
            {
                throw new ArgumentException();
            }
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                var currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                    // An UnauthorizedAccessException exception will be thrown if we do not have
                    // discovery permission on a folder or file. It may or may not be acceptable 
                    // to ignore the exception and continue enumerating the remaining files and 
                    // folders. It is also possible (but unlikely) that a DirectoryNotFound exception 
                    // will be raised. This will happen if currentDir has been deleted by
                    // another application or thread after our call to Directory.Exists. The 
                    // choice of which exceptions to catch depends entirely on the specific task 
                    // you are intending to perform and also on how much you know with certainty 
                    // about the systems on which this code will run.
                catch (UnauthorizedAccessException )
                {
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException )
                {
                    continue;
                }

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir);
                }

                catch (UnauthorizedAccessException)
                {

                    //Console.WriteLine(e.Message);
                    continue;
                }

                catch (System.IO.DirectoryNotFoundException )
                {
                    //Console.WriteLine(e.Message);
                    continue;
                }
                // Perform the required action on each file here.
                // Modify this block to perform your required task.
                foreach (var file in files)
                {
                    yield return file;
                }

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (var str in subDirs) dirs.Push(str);
            }
        }
    }
}
