using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UWPFastFileEnumeration
{
    public static class DirectoryHelper
    {

        private const int INVALID_HANDLE_VALUE = -1;

        private const int FIND_FIRST_EX_CASE_SENSITIVE = 1;
        private const int FIND_FIRST_EX_LARGE_FETCH = 2;
        private const int FIND_FIRST_EX_ON_DISK_ENTRIES_ONLY = 4;

        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFileExFromApp(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            int dwAdditionalFlags);

        [DllImport("api-ms-win-core-file-l1-1-0.dll", CharSet = CharSet.Unicode)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("api-ms-win-core-file-l1-1-0.dll")]
        private static extern bool FindClose(IntPtr hFindFile);
       
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            FINDEX_INFO_LEVELS findInfoLevel = FINDEX_INFO_LEVELS.FindExInfoBasic;
            int additionalFlags = FIND_FIRST_EX_LARGE_FETCH;

            Queue<string> subDirectorys = null;
            if (searchOption == SearchOption.AllDirectories) {
                subDirectorys = new Queue<string>();
            }

            do
            {
                IntPtr hFile = FindFirstFileExFromApp(
                    path + "\\" + searchPattern,
                    findInfoLevel,
                    out WIN32_FIND_DATA findData,
                    FINDEX_SEARCH_OPS.FindExSearchNameMatch,
                    IntPtr.Zero,
                    additionalFlags);

                if (hFile.ToInt64() != INVALID_HANDLE_VALUE)
                {
                    do
                    {
                        var attributes = (FileAttributes)findData.dwFileAttributes;

                        if (!attributes.HasFlag(FileAttributes.System))
                        {
                            if (!attributes.HasFlag(FileAttributes.Directory))
                            {
                                yield return findData.cFileName;
                            }
                            else if (searchOption == SearchOption.AllDirectories && findData.cFileName != "." && findData.cFileName != "..")
                            {
                                subDirectorys.Enqueue(Path.Combine(path, findData.cFileName));
                            }
                        }
                    }
                    while (FindNextFile(hFile, out findData));

                    FindClose(hFile);
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            while (searchOption == SearchOption.AllDirectories && subDirectorys.TryDequeue(out path));            
        }

        public static IEnumerable<string> EnumerateFiles(string path)
        {
            return EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

    }
}
