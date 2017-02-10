using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace PhotoSorter
{
    public class PhotoSorter
    {
        private StreamWriter _logfiles;
        private CultureInfo _cultureInfo;
        private readonly photoSource[] _sourcePaths;
        private string _duplicatePath;
        private string _sortedPath;
        private DupDetector _destinationDuplicates;

        struct FilenameInfo
        {
            public DateTime? date;
            public int FileNumber;
            public int FollowNumber;
        }

        public struct photoSource
        {
            public string path;
            public string postfix;
        }

        public PhotoSorter(photoSource[] sourcePaths, string sortedPath, string duplicatePath)
        {
            _sourcePaths = sourcePaths;
            _sortedPath = sortedPath;
            _duplicatePath = duplicatePath;
            _cultureInfo = new CultureInfo("nl-NL"); ;
            _logfiles = new System.IO.StreamWriter(@"SkippedFiles.txt");
        }

        ~PhotoSorter()
        {
  
        }

        private void LogSkippedFiles(string logLine)
        {
            _logfiles.WriteLine(logLine);
        }

        public  void ShowDirectory(string path)
        {
            //path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path)) return;


            var fileList = Directory.GetFiles(path);


            foreach (var file in fileList)
            {
                CreateDestPathFromFileName(file);
            }
            Console.ReadLine();
        }

        public void RemoveDuplicatesFromSorted()
        {
            // # check destination for redundancies
            _destinationDuplicates = new DupDetector();
            foreach (var file in FileUtils.RecurseFilesInDirectories(_sortedPath))
            {

                if (
                    !FileUtils.MatchesFile(file,
                        new[] {"*.jpg", "*.mp4", "*.png", "*.bmp", "*.raw", "*.mov", "*.gif", "*.mpg", "*.mpeg"}, false))
                {
                    Console.WriteLine("{0} no image/movie, skipping", file);
                    continue;
                }

                // For each file in nested directory
                if (_destinationDuplicates.HasDuplicate(file))
                {
                    // move to duplicate directory
                    var duplicateFilePath = FileUtils.ChangeFileFolder(file, _sortedPath, _duplicatePath);
                    if (CreateDirectory(duplicateFilePath)) continue;

                    if (File.Exists(duplicateFilePath) && DupDetector.IsDuplicate(duplicateFilePath, file))
                    {
                        Console.WriteLine("{0} has already been copied, should be deleted", file);
                    }

                    duplicateFilePath = GetUniqueNumberedFileName(duplicateFilePath);
                    Console.WriteLine("Moving duplicate {0} to {1}", file, duplicateFilePath);

                }
                else
                {
                    // add to list
                    _destinationDuplicates.AddFile(file);
                    //Console.WriteLine("No duplicate found for {0}",file);
                }
            }
        }

        private static bool CreateDirectory(string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName == null) return true;

            if (!Directory.Exists(directoryName))
            {
                Console.WriteLine("Creating directory {0}", directoryName);
                //Directory.CreateDirectory(t);
            }
            return false;
        }

        public void MoveToSorted()
        {
            foreach (var sourcePath in _sourcePaths)
            {
                foreach (var sourceFile in FileUtils.RecurseFilesInDirectories(sourcePath.path))
                {
                    var destFile = CreateDestPathFromFileName(sourceFile);
                    if (
                        !FileUtils.MatchesFile(destFile,
                            new[] {"*.jpg", "*.mp4", "*.png", "*.bmp", "*.raw", "*.mov", "*.gif", "*.mpg", "*.mpeg"}, false))
                    {
                        Console.WriteLine("{0} no image/movie, skipping", destFile);
                        continue;
                    }
                    
                    if (File.Exists(destFile) && DupDetector.IsDuplicate(destFile, sourceFile))
                    {
                        // File with same name already exists, with same CRC, so file already copied
                        Console.WriteLine("{0} has already been copied, will not be copied again",sourceFile);
                    }
                    else
                    {
                        CreateDestPathFromFileName(destFile);

                        // File will be copied to other name
                        destFile = GetUniqueNumberedFileName(destFile);
                        Console.WriteLine("copying {0} to {1}",sourceFile,destFile);    
                    }
                }
            }
        }


        private string CreateDestPathFromFileName(string filename)
        {
            var creationTime = File.GetCreationTime(filename);

            // Ignore non-image and non movie files
            if (!FileUtils.MatchesFile(filename, new[] {"*.jpg", "*.mp4", "*.png", "*.mov", "*.gif", "*.mpg", "*.mpeg" }, false))
            {
                //LogSkippedFiles(filename + ", NoMatch");
                return "";
            }

            // Try to get date from name 
            var fileData = ParseName(filename);
            if (fileData.date== null) return "";

            if (fileData.date.Value.Date > creationTime.Date)
            {
                //LogSkippedFiles(filename + ", FilenameDatePastCreationDate");
                return "";
            }

            var nameOnly = Path.GetFileName(filename);

            var newPath = string.Format(_cultureInfo, "{0}\\{1:yyyy}\\{2:MMMM}\\{3}", _sortedPath, fileData.date.Value, fileData.date.Value, nameOnly);
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
                    filenameInfo.date = DateTime.ParseExact(dateFromNameString, "yyyyMMdd", CultureInfo.InvariantCulture);                   
                }
                catch
                {
                    LogSkippedFiles(filename + ", FilenameNotParsable");
                    filenameInfo.date = null;
                }                
            }
            else
            {
                LogSkippedFiles(filename + ", FilenameNotParsable");
                filenameInfo.date = null;
            }

            // Some sanity checks on date
            
            if (filenameInfo.date.Value.Year < 2000 || filenameInfo.date > DateTime.Now + TimeSpan.FromDays(1))
            {
                LogSkippedFiles(filename + ", UnexpectedDate");
                filenameInfo.date = null;
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

            var name = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            // Check if valid filename
            if (string.IsNullOrWhiteSpace(name)) return null;

            int followNumber;
            string filenameWithoutNumber;

            var match = Regex.Match(name, @"(.+)(_| |-)(\d+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                followNumber = int.Parse(match.Groups[2].Value);
                filenameWithoutNumber = match.Groups[0].Value + match.Groups[1].Value;
            }
            else
            {
                filenameWithoutNumber = name;
                followNumber = 0;
            }
            followNumber++;
            var composedFileName = filenameWithoutNumber + postfix +followNumber + extension;
            while (!File.Exists(composedFileName))
            {
                followNumber++;
                composedFileName = filenameWithoutNumber + postfix + followNumber + extension;
            }
            return composedFileName;

        }

    }
}