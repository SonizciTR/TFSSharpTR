using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.AutoDeploy
{
    internal class HashItem
    {
        private const string seperator = "*?*";

        public string FileName { get; set; }

        public string FileHash { get; set; }

        public string HashLine
        {
            get
            {
                if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(FileHash))
                    return null;

                return FileName + seperator + FileHash;
            }
        }

        public HashItem()
        {
        }

        public HashItem(string fullFileName, string fileHash)
        {
            FileHash = fileHash;
            FileName = Path.GetFileName(fullFileName);
        }

        public bool ParseFromFileLine(string fileLine)
        {
            if (string.IsNullOrEmpty(fileLine))
                return false;

            var sprtd = fileLine.Split(new string[] { seperator }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (sprtd?.Length != 2)
                return false;

            FileHash = sprtd[0];
            FileName = sprtd[1];

            return true;
        }

        public static List<HashItem> ParseFromFileLineList(List<string> fileLine)
        {
            List<HashItem> lst = new List<HashItem>();
            foreach (var item in fileLine)
            {
                var tmp = new HashItem();
                tmp.ParseFromFileLine(item);
                lst.Add(tmp);
            }
            return lst;
        }
    }

    internal static class HashOperationHelper
    {

        public static List<HashItem> GenerateHashFileStructure(string sourceFolder, List<string> sourceFiles)
        {
            var data = new List<HashItem>();

            foreach (var file in sourceFiles)
            {
                string diffName = FileOperationHelper.RemoveBaseFolder(sourceFolder, file);
                byte[] fArray = FileOperationHelper.SafeFileRead(file);
                if (fArray == null)
                    throw new IOException("Source file could not readed = " + file);

                var tmpHsh = new HashItem(file, Hash512(fArray));
                data.Add(tmpHsh);
            }

            return data;
        }

        private static string Hash512(string word)
        {
            return Hash512(Encoding.UTF8.GetBytes(word));
        }

        private static string Hash512(byte[] message)
        {
            string hexNumber = "";
            using (SHA512Managed hashString = new SHA512Managed())
            {
                byte[] hashValue = hashString.ComputeHash(message);
                foreach (byte x in hashValue)
                {
                    hexNumber += string.Format("{0:x2}", x);
                }
            }
            return hexNumber;
        }
    }
}
