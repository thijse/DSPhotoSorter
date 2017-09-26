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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhotoSorter
{
    public class HashTool
    {
        static HashTool()
        {
        }


        public static byte[] HashFileBytes(string file)
        {
            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    using (var md5 = new MD5CryptoServiceProvider())
                        return md5.ComputeHash(fs);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static byte[] HashFileBytes(string file, int start, int count)
        {
            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var bytes = new byte[count];
                    fs.Seek(start, SeekOrigin.Begin);
                    fs.Read(bytes, 0, count);
                    using (var md5 = new MD5CryptoServiceProvider())
                        return md5.ComputeHash(bytes);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string QuickHashFile(string file)
        {
            if (new FileInfo(file).Length <= 0) return "";

            //fast byte checking
            using (var stream = File.OpenRead(file))
            {
                var length = stream.Length;
                var tags = new byte[3];

                stream.Seek(0, SeekOrigin.Begin);
                if (length <= 3)
                {
                    stream.Read(tags, 0, (int) length);
                }
                else
                {
                    //first byte
                    tags[0] = (byte) stream.ReadByte();

                    //middle byte, we need it especially for xml like files
                    stream.Seek(stream.Length/2, SeekOrigin.Begin);
                    tags[1] = (byte) stream.ReadByte();

                    //last byte
                    stream.Seek(0, SeekOrigin.End);
                    tags[2] = (byte) stream.ReadByte();
                }
                return GetHashText(tags);
            }
        }


        public static void SetQuickHash(FileItem file)
        {
            if (file.Size > 0)
            {
                //fast random byte checking
                using (var stream = File.OpenRead(file.FileName))
                {
                    var length = stream.Length;
                    var tags = new byte[3];
                    //first byte
                    stream.Seek(0, SeekOrigin.Begin);
                    tags[0] = (byte) stream.ReadByte();

                    //middle byte, we need it especially for xml like files
                    if (length > 1)
                    {
                        stream.Seek(stream.Length/2, SeekOrigin.Begin);
                        tags[1] = (byte) stream.ReadByte();
                    }

                    //last byte
                    if (length > 2)
                    {
                        stream.Seek(0, SeekOrigin.End);
                        tags[2] = (byte) stream.ReadByte();
                    }

                    file.QuickHash = HashTool.GetHashText(tags);
                }
            }
        }


        public static string GetHashText(byte[] hashBytes)
        {
            var sb = new StringBuilder(hashBytes.Length*2);
            foreach (var t in hashBytes)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string HashFile(string file)
        {
            var result = HashFileBytes(file);
            return result != null ? GetHashText(result) : null;
        }

        public static string HashFile(string file, int start, int count)
        {
            var result = HashFileBytes(file, start, count);
            return result != null ? GetHashText(result) : null;
        }
    }
}