using System.Collections.Generic;
using System.IO;

namespace PhotoSorter
{
    public class DupDetector
    {
        private readonly SameSizeGroups _sameSizeGroups;
        private readonly SameQuickHashGroups _sameQuickHashGroups;
        private readonly SameFullHashGroups _sameFullHashGroups;
        private long _fileNo;
        private long _duplicates;

        public DupDetector()
        {
            _sameSizeGroups = new SameSizeGroups();
            _sameQuickHashGroups = new SameQuickHashGroups();
            _sameFullHashGroups = new SameFullHashGroups();
        }

        public static bool IsDuplicate(string fileName1, string fileName2)
        {
            if (new FileInfo(fileName1).Length != new FileInfo(fileName2).Length) return false;
            if (HashTool.QuickHashFile(fileName1) != HashTool.QuickHashFile(fileName2)) return false;
            return (HashTool.HashFile(fileName1) == HashTool.HashFile(fileName2));
        }

        public List<Duplicate> AddFilesFindDuplicates(string[] fileNames)
        {
            var list = new List<string>();
            list.AddRange(fileNames);
            return AddFilesFindDuplicates(list);
        }

        public List<Duplicate> AddFilesFindDuplicates(List<string> fileNames)
        {
            var fullHashes = new HashSet<string>();

            foreach (var fileName in fileNames)
            {
                
                var fullHash = AddFileGetHash(fileName);
                if (fullHash != null) fullHashes.Add(fullHash);
            }

            // create list of duplicates based 
            var duplicates = new List<Duplicate>();
            foreach (var fullHash in fullHashes)
            {
                if (_sameFullHashGroups[fullHash].Files.Count < 2)
                {
                    continue;
                }
                var duplicate = new Duplicate();

                duplicate.Items.AddRange(_sameFullHashGroups[fullHash].Files);
                duplicates.Add(duplicate);
            }
            return duplicates;
        }

        public void AddFiles(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                _fileNo++;
                AddFileGetHash(fileName);
            }
        }

        public Duplicate AddFileFindDuplicate(string fileName)
        {
            var fullHash = AddFileGetHash(fileName);
            if (fullHash == null) return null;
            var duplicate = new Duplicate();
            duplicate.Items.AddRange(_sameFullHashGroups[fullHash].Files);
            return duplicate;
        }

        public void AddFile(string fileName)
        {
            AddFileGetHash(fileName);
        }

        public Duplicate FindDuplicate(string fileName)
        {
            var fullHash = GetHash(fileName);
            if (fullHash == null) return null;

            var duplicate = new Duplicate();
            duplicate.Items.AddRange(_sameFullHashGroups[fullHash].Files);
            return duplicate;
        }

        public bool HasDuplicate(string fileName)
        {
            return GetHash(fileName)!=null;
        }

        public List<Duplicate> FindDuplicates(List<string> fileNames)
        {
            var fullHashes = new HashSet<string>();

            foreach (var fileName in fileNames)
            {
                _fileNo++;
                var fullHash = GetHash(fileName);
                if (fullHash != null) fullHashes.Add(fullHash);
            }

            // create list of duplicates based 
            var duplicates = new List<Duplicate>();
            foreach (var fullHash in fullHashes)
            {
                var duplicate = new Duplicate();
                duplicate.Items.AddRange(_sameFullHashGroups[fullHash].Files);
                duplicates.Add(duplicate);
            }
            return duplicates;
        }


        // ******** Private functions ****** /

        private string GetHash(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            // Add all files in groups based on size
            if (!_sameSizeGroups.ContainsKey(fileInfo.Length)) return null;

            if (_sameSizeGroups[fileInfo.Length].Files.Count == 1 &&
                string.IsNullOrEmpty(_sameSizeGroups[fileInfo.Length].Files[0].QuickHash))
                {
                    // Single file in group, so no quickhash comparison done. Create one now
                    var prevFileItem = _sameSizeGroups[fileInfo.Length].Files[0];
                    AddToQuickHash(prevFileItem);
                }

            var quickHash = HashTool.QuickHashFile(fileName);
            if (!_sameQuickHashGroups.ContainsKey(quickHash)) return null;


            if (_sameQuickHashGroups[quickHash].Files.Count == 1 &&
                 string.IsNullOrEmpty(_sameQuickHashGroups[quickHash].Files[0].FullHash))
            {
                // quickhash in group, but no fullhash comparison done. Create one now
                var prevFileItem = _sameQuickHashGroups[quickHash].Files[0];
                AddToFullHash(prevFileItem);
            }

            var fullHash = HashTool.HashFile(fileName);
            if (!_sameFullHashGroups.ContainsKey(fullHash)) return null;
            return fullHash;
            //return !_sameFullHashGroups.ContainsKey(fullHash) ? null : fullHash;
        }


        private string AddFileGetHash(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            var fileItem = new FileItem()
            {
                FileName = fileName,
                Size = fileInfo.Length,
                ModifiedTime = fileInfo.LastWriteTime
            };
            _fileNo++;
            // Add all files in groups based on size
            if (_sameSizeGroups.ContainsKey(fileItem.Size))
            {
                if (_sameSizeGroups[fileItem.Size].Files.Count == 1 &&
                    string.IsNullOrEmpty(_sameSizeGroups[fileItem.Size].Files[0].QuickHash))
                {
                    // Single file in group, so no quickhash comparison done. Create one now
                    var prevFileItem = _sameSizeGroups[fileItem.Size].Files[0];
                    AddToQuickHash(prevFileItem);
                }
                _sameSizeGroups[fileItem.Size].Files.Add(fileItem);
                return AddToQuickHash(fileItem);
            }
            else
            {
                // filesize did not exist, so no duplicate file
                _sameSizeGroups.Add(fileItem.Size, new SameSizeGroup());
                _sameSizeGroups[fileItem.Size].Files.Add(fileItem);
                return null;
            }
        }

        private string AddToQuickHash(FileItem fileItem)
        {
            // for empty files, do not hash
            fileItem.QuickHash = fileItem.Size == 0 ? "" : HashTool.QuickHashFile(fileItem.FileName);
            if (_sameQuickHashGroups.ContainsKey(fileItem.QuickHash))
            {
                if (_sameQuickHashGroups[fileItem.QuickHash].Files.Count == 1 &&
                    string.IsNullOrEmpty(_sameQuickHashGroups[fileItem.QuickHash].Files[0].FullHash))
                {
                    // quickhash in group, so no fullhash comparison done. Create one now
                    var prevFileItem = _sameQuickHashGroups[fileItem.QuickHash].Files[0];
                    AddToFullHash(prevFileItem);
                    _duplicates++;
                    //Log(string.Format("{0}, {1}, {2}", _fileNo, _duplicates, _sameFullHashGroups.Count));
                }

                _sameQuickHashGroups[fileItem.QuickHash].Files.Add(fileItem);

                // This fileSize exists, so check on existing full hash
                return AddToFullHash(fileItem);
            }
            else
            {
                // quickhash did not exist, so no duplicate file
                _sameQuickHashGroups.Add(fileItem.QuickHash, new SameQuickHashGroup());
                _sameQuickHashGroups[fileItem.QuickHash].Files.Add(fileItem);
                return null;
            }
        }

        private string AddToFullHash(FileItem fileItem)
        {
            // for small files, the full hash is same as quickhash
            fileItem.FullHash = fileItem.Size < 4 ? fileItem.QuickHash : HashTool.HashFile(fileItem.FileName);
            if (_sameFullHashGroups.ContainsKey(fileItem.FullHash))
            {
                // fullhash exists, so duplicate file
                _sameFullHashGroups[fileItem.FullHash].Files.Add(fileItem);
                return fileItem.FullHash;
            }
            else
            {
                // fullhash did not exist, so no duplicate file
                _sameFullHashGroups.Add(fileItem.FullHash, new SameFullHashGroup());
                _sameFullHashGroups[fileItem.FullHash].Files.Add(fileItem);
                return null;
            }
        }
    }
}
