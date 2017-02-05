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
        private readonly string _rootPath;

        struct FilenameInfo
        {
            public DateTime? date;
            public int FileNumber;
            public int FollowNumber;
        }


        public PhotoSorter(string rootPath)
        {
            _rootPath = rootPath;
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
                ShowDate(file);
            }
            Console.ReadLine();
        }

        public void CopyAndClean(string source, string dest)
        {
            // # check destination for redundancies
                // For each file in nested directory
                    //check if duplicate
                        // if duplicate
                            // keep with lowest follow number /oldest
                            // move duplicate to duplicate folder (mirroring subfolder structure)

            // # Copy files to folder structure
                // For each file in nested directory
                    //check if duplicate present
                        // if duplicate
                            // keep with lowest follow number /oldest
                            // move duplicate to duplicate folder (mirroring subfolder structure)
                        // if not duplicate
                            // copy file to suggested directory
                            // if same filename exist
                                // add/update follow number
        }

        private void ShowDate(string filename)
        {
            var creationTime = File.GetCreationTime(filename);

            // Ignore non-image and non movie files
            if (!FileUtils.MatchesFile(filename, new[] {"*.jpg", "*.mp4", "*.png", "*.mov", "*.gif", "*.mpg", "*.mpeg" }, false))
            {
                LogSkippedFiles(filename + ", NoMatch");
                return;
            }

            // Try to get date from name 
            var fileData = ParseName(filename);
            if (fileData.date== null) return;

            if (fileData.date.Value.Date > creationTime.Date)
            {
                LogSkippedFiles(filename + ", FilenameDatePastCreationDate");
                return;
            }

            var nameOnly = Path.GetFileName(filename);

            // File name reveals date as expect, see if it is numbered (duplicate version)
            var fileNumber = fileData.FileNumber;
            if (fileNumber < 0)
            {
                LogSkippedFiles(filename + ", NoFileNumber");
                return;
            }



            var newPath = string.Format(_cultureInfo, "{0}\\{1:yyyy}\\{2:MMMM}\\{3}", _rootPath, fileData.date.Value, fileData.date.Value, nameOnly);
            

            if (File.Exists(newPath)) { Console.WriteLine("{0} - EXISTS", newPath); } else { Console.WriteLine("{0}", newPath); }

            //if (dateFromName.Date == date.Date)
            //{
            //    Console.WriteLine("{0} -  {1:dd MMM yyyy} , {2:dd MMM yyyy} - SAME", nameOnly, date, dateFromName);
            //} else

            //Console.WriteLine("{0} -  {1:dd MMM yyyy} , {2:dd MMM yyyy}", nameOnly, date, dateFromName);
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
    }
}