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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PhotoSorter
{
    public class PhotoSorter
    {
        private readonly CultureInfo _cultureInfo;
        private readonly List<PhotoSource> _sourcePaths;
        private readonly string _duplicatePath;
        private readonly string _sortedPath;        
        private readonly string _configurationPath;
        private readonly HashSet<string> _processedFilesList;
        private readonly string _mapRootFrom;
        private readonly string _mapRootTo;
        private DupDetector _destinationDuplicates;
        private int _processedFilesListPrevCount;

        struct FilenameInfo
        {
            public DateTime? Date;
            public int FileNumber;
            public int FollowNumber;
        }

        public struct PhotoSource
        {
            public string Path;
            public string Postfix;
        }

        public PhotoSorter(List<PhotoSource> sourcePaths, string sortedPath, string duplicatePath, string configurationPath, string mapRootFrom, string mapRootTo)
        {
            _sourcePaths            = sourcePaths;
            _sortedPath             = sortedPath;
            _duplicatePath          = duplicatePath;
            _configurationPath      = configurationPath;
            _mapRootFrom            = mapRootFrom;
            _mapRootTo              = mapRootTo;
            _cultureInfo            = new CultureInfo("nl-NL"); 
            _processedFilesList     = new HashSet<string>();
        }


        ~PhotoSorter() { }

        private void ReadProcessedFiles()
        {
            _processedFilesList.Clear();

            var processedFilesTxt = FileUtils.Combine(_configurationPath, @"processedFiles.txt");
            var processedFilesBak = FileUtils.Combine(_configurationPath, @"processedFiles.bak");
            var processedFiles    = "";
            if (!File.Exists(processedFilesTxt) && !File.Exists(processedFilesBak))                      { return; }
            if ( File.Exists(processedFilesTxt) && !File.Exists(processedFilesBak))                      { processedFiles = processedFilesTxt; }
            else if (!File.Exists(processedFilesTxt) && (File.Exists(processedFilesBak)))                { processedFiles = processedFilesBak; }
            else if (File.GetCreationTime(processedFilesTxt) >= File.GetCreationTime(processedFilesBak)) { processedFiles = processedFilesTxt; } else { processedFiles = processedFilesBak; }
            
            _processedFilesList.UnionWith(File.ReadAllLines(processedFiles));
        }

        private void UpdateProcessedFiles()
        {
            var processedFilesListCount  = _processedFilesList.Count;
            if (processedFilesListCount <= _processedFilesListPrevCount+100) return;
            _processedFilesListPrevCount = processedFilesListCount;
            WriteProcessedFiles();
        }

        private void WriteProcessedFiles()
        {
            CreateDirectory(_configurationPath);
            var processedFilesTxt  = FileUtils.Combine(_configurationPath, @"processedFiles.txt");
            var processedFilesBak  = FileUtils.Combine(_configurationPath, @"processedFiles.bak");
            var processedFilesTemp = FileUtils.Combine(_configurationPath, @"processedFiles-" + Guid.NewGuid().ToString() + ".temp");
            if (File.Exists(processedFilesTemp))
            {
                try
                {
                    File.Delete(processedFilesTemp);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to delete temp file");
                }
            }
            
            IEnumerable<string> orderedProcessedFilesList = _processedFilesList.OrderBy(pathName => pathName);

            try
            {
                using (var processedFilesTempWriter = new System.IO.StreamWriter(processedFilesTemp,false))
                {
                    foreach (string filePath in orderedProcessedFilesList)
                    {
                        processedFilesTempWriter.WriteLine(filePath);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to write file: {0}",e.Message);
                try { File.Delete(processedFilesTemp); } catch (Exception) { }
            }

            if (File.Exists(processedFilesTxt))
            {
                if (File.Exists(processedFilesBak)) File.Delete(processedFilesBak);
                try
                {
                    File.Move(processedFilesTxt, processedFilesBak);
                }
                catch 
                {
                    try { File.Delete(processedFilesTxt); } catch (Exception) { }
                }
            }

            try
            {
                File.Move(processedFilesTemp, processedFilesTxt);
            }
            catch 
            {
                try { File.Copy(processedFilesBak, processedFilesTxt); } catch (Exception) { }
            }
        }

        public void IndexAndCleanSorted()
        {
            var progressCount = 0;
            // # check destination for redundancies
            _destinationDuplicates = new DupDetector();
            foreach (var file in FileUtils.RecurseFilesInDirectories(_sortedPath))
            {
                progressCount++;
                if (progressCount%100 == 0) { Console.WriteLine("Indexed & cleaned {0} files", progressCount); }

                if (
                    !FileUtils.MatchesFile(file,
                        new[] {"*.jpg", "*.mp4", "*.png", "*.bmp", "*.raw", "*.mov", "*.gif", "*.mpg", "*.mpeg",".psd"}, false))
                {
                  //  Console.WriteLine("{0} no image/movie, skipping", file);
                    continue;
                }

                // For each file in nested directory
                if (_destinationDuplicates.HasDuplicate(file))
                {
                    // move to duplicate directory
                    var duplicateFilePath = FileUtils.ChangeFileFolder(file, _sortedPath, _duplicatePath);

                    if (File.Exists(duplicateFilePath) && DupDetector.IsDuplicate(duplicateFilePath, file))
                    {
                        Console.WriteLine("{0} has already been copied, should be deleted", file);
                    }

                    duplicateFilePath = GetUniqueNumberedFileName(duplicateFilePath);
                    MoveFile(file, duplicateFilePath);

                }
                else
                {
                    // add to list
                    var duplicates = _destinationDuplicates.AddFileFindDuplicate(file);
                    if (duplicates!=null && duplicates.Items.Count > 1)
                    {
                        Console.WriteLine("Unexpected!");    
                    }
                    //Console.WriteLine("No duplicate found for {0}",file);
                }
            }
        }

        public void MoveToSorted()
        {
            var progressCount = 0;
            ReadProcessedFiles();
            foreach (var sourcePath in _sourcePaths)
            {
                progressCount++;
                if (progressCount % 100 == 0) { Console.WriteLine("Processed {0} files for sorting", progressCount); }

                foreach (var sourceFile in FileUtils.RecurseFilesInDirectories(sourcePath.Path))
                {                   
                    if (_processedFilesList.Contains(MapSource(sourceFile)))
                    {
                        //Console.WriteLine("{0} processed last time, skipping", sourceFile);
                        continue;
                    }

                    if (
                        !FileUtils.MatchesFile(sourceFile,
                            new[] { "*.jpg", "*.mp4", "*.png", "*.bmp", "*.raw", "*.mov", "*.gif", "*.mpg", "*.mpeg", "*.psd" }, false))
                    {
                        //Console.WriteLine("{0} no image/movie, skipping", destFile);
                        continue;
                    }


                    var destFile = CreateDestPathFromFileName(sourceFile);
                    if (destFile == "")
                    {
                        Console.WriteLine("could not create Path from name {0}", sourceFile);
                        continue;
                    }                    

                    if (_destinationDuplicates.HasDuplicate(sourceFile)) { 
                        Console.WriteLine("{0} is duplicate, will not be copied again", sourceFile);                       
                    }
                    else
                    {
                        // File will be copied to other name
                        destFile = GetUniqueNumberedFileName(destFile);
                        CopyFile(sourceFile, destFile);

                        var duplicates = _destinationDuplicates.AddFileFindDuplicate(destFile);
                        if (duplicates != null && duplicates.Items.Count > 1)
                        {
                            Console.WriteLine("Unexpected!");
                        }
                        
                        //Console.WriteLine("copying {0} to {1}", sourceFile, destFile);
                    }
                    _processedFilesList.Add(MapSource(sourceFile));
                    UpdateProcessedFiles();
                }
            }
            WriteProcessedFiles();
        }

        private string MapSource(string sourceFile)
        {
            //Map processedFiles to new root if needed to make sure that if we processing is done from a different PC,
            // or a different IP adress, or a different mapped drive, processed files are still recognized
            if (_mapRootFrom == "") return sourceFile;
            if (sourceFile.IndexOf(_mapRootFrom, StringComparison.Ordinal) == 0)
            {
                var destination = sourceFile.Replace(_mapRootFrom, _mapRootTo);
                return destination;
            }
            return sourceFile;
        }

        private static void MoveFile(string sourceFilePath, string destinationFilePath)
        {
            if (sourceFilePath==null || destinationFilePath== null) return;
            if (!File.Exists(sourceFilePath) || File.Exists(destinationFilePath)) return;            
            Console.WriteLine("Moving duplicate {0} to {1}", sourceFilePath, destinationFilePath);
            CreateDirectory(destinationFilePath);
            File.Move(sourceFilePath, destinationFilePath);
        }

        private static void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            if (sourceFilePath == null || destinationFilePath == null) return;
            if (!File.Exists(sourceFilePath) || File.Exists(destinationFilePath)) return;
            Console.WriteLine("Copying file {0} to {1}", sourceFilePath, destinationFilePath);
            CreateDirectory(destinationFilePath);
            File.Copy(sourceFilePath, destinationFilePath);
        }

        private static bool CreateDirectory(string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryName)) return true;

            if (!Directory.Exists(directoryName))
            {
                Console.WriteLine("Creating directory {0}", directoryName);
                Directory.CreateDirectory(directoryName);
            }
            return false;
        }

        private string CreateDestPathFromFileName(string filename)
        {
            var creationTime = File.GetCreationTime(filename);

            // Ignore non-image and non movie files
            if(!FileUtils.MatchesFile(filename,new[] { "*.jpg", "*.mp4", "*.png", "*.bmp", "*.raw", "*.mov", "*.gif", "*.mpg", "*.mpeg", "*.psd" }, false))    
            {
                return "";
            }

            // Try to get date from name 
            var fileData = ParseName(filename);
            if (fileData.Date== null) return "";

            if (fileData.Date.Value.Date > creationTime.Date|| fileData.Date.Value.Date> DateTime.Now)
            {
                return "";
            }

            var nameOnly = Path.GetFileName(filename);

            var newPath = string.Format(_cultureInfo, "{0}\\{1:yyyy}\\{2:MMMM}\\{3}", _sortedPath, fileData.Date.Value, fileData.Date.Value, nameOnly);
            return newPath;
        }

        private FilenameInfo ParseName(string filename)
        {
            FilenameInfo filenameInfo = new FilenameInfo();
            var nameOnly = Path.GetFileName(filename);

            // Check if valid filename
            if (string.IsNullOrWhiteSpace(nameOnly)) return filenameInfo;

            var match = Regex.Match(nameOnly, @"IMG_(\d{8})_(\d+)(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var dateFromNameString = match.Groups[1].Value;
                try
                {
                    //try parse matched expression as date
                    filenameInfo.Date = DateTime.ParseExact(dateFromNameString, "yyyyMMdd", CultureInfo.InvariantCulture);                   
                }
                catch
                {
                    filenameInfo.Date = null;
                }                
            }
            else
            {
                filenameInfo.Date = null;
            }

            // Some sanity checks on date
            
            if (filenameInfo.Date.Value.Year < 2000 || filenameInfo.Date > DateTime.Now + TimeSpan.FromDays(1))
            {
                filenameInfo.Date = null;
            }

            if (match.Groups.Count > 2)
            {
                var fileNumber = match.Groups[2].Value;
                //try parse matched expression as date
                var result = int.TryParse(fileNumber, NumberStyles.Any, CultureInfo.InvariantCulture,
                    out filenameInfo.FileNumber);
                if (!result) filenameInfo.FileNumber = -1;

                if (match.Groups.Count > 3)
                {
                    var remainder = match.Groups[3].Value;
                    var followNumbermatch = Regex.Match(remainder, @"^_(\d+).*", RegexOptions.IgnoreCase);
                    if (followNumbermatch.Success)
                    {
                        var followNumber = followNumbermatch.Groups[1].Value;
                        //try parse matched expression as date
                        result = int.TryParse(followNumber, NumberStyles.Any, CultureInfo.InvariantCulture,out filenameInfo.FollowNumber);
                        if (!result) filenameInfo.FollowNumber = -1;
                    }
                }
            }
            return filenameInfo;
        }

        private string GetUniqueNumberedFileName(string filename,string postfix = "")
        {
            if (!File.Exists(filename)) return filename;
            var directory = Path.GetDirectoryName(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            // Check if valid filename
            if (string.IsNullOrWhiteSpace(name)) return null;

            int followNumber;
            string filenameWithoutNumber;

            //var match = Regex.Match(name, @"(.+)(_| |-)(\d+)$", RegexOptions.IgnoreCase);
            var match = Regex.Match(name, @"IMG_(\d{8})_(\d+)_(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                followNumber = int.Parse(match.Groups[2].Value);
                filenameWithoutNumber = "IMG_"+match.Groups[0].Value +"_" + match.Groups[1].Value;
            }
            else
            {
                filenameWithoutNumber = name;
                followNumber = 0;
            }
            followNumber++;
            var composedFileName = directory + @"\" + filenameWithoutNumber + postfix + "_"+followNumber + extension;
            while (File.Exists(composedFileName))
            {
                followNumber++;
                composedFileName = directory + @"\" + filenameWithoutNumber + postfix + "_" + followNumber + extension;
            }
            return composedFileName;

        }

    }
}