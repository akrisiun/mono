using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.XHtml.IOFile
{
    public static class DirectoryEnum
    {
        public static IEnumerable<string> ReadFiles(string path,
                String searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var resultHandler = new StringResultHandler(true, includeDirs: false);
            var iterator = new Win32FileSystemEnumerableIterator<string>(path, null, searchPattern, searchOption, resultHandler);
            var numer = iterator.GetEnumerator();

            while (numer.MoveNext())
                yield return Path.Combine(path, numer.Current);
        }

        public static IEnumerable<FileDataInfo> ReadFilesInfo(string path,
                       String searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var resultHandler = new FileDataInfoResultHandler();
            var iterator = new Win32FileSystemEnumerableIterator<FileDataInfo>(path, null, searchPattern, searchOption, resultHandler);
            var numer = iterator.GetEnumerator();

            while (numer.MoveNext())
                yield return numer.Current;
        }

        public class FileDataInfo
        {
            internal uint dwFileAttributes;
            internal Win32FindFile.FILE_TIME ftLastWriteTime;
            internal uint nFileSizeLow;

            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string cFileName;
            long? ticks;

            public string Name { get { return cFileName; } set { cFileName = value; } }
            public int Length { get { return (int)nFileSizeLow; } set { nFileSizeLow = (uint)value; } }

            public DateTime LastWriteTime {
                get { return ticks.HasValue ? DateTime.FromBinary(ticks.Value) : DateTime.FromFileTime(ftLastWriteTime.ToTicks()); }
                set { ticks = value.ToBinary(); }
            }
        }

        internal class FileDataInfoResultHandler : SearchResultHandlerEx<FileDataInfo>
        {
            [System.Security.SecurityCritical]
            internal override bool IsResultIncluded(SearchResult result)
            {
                // Win32FileSystemEnumerableHelpers
                return FileSystemEnumerableHelpers.IsFile(result.FindData);
            }

            [System.Security.SecurityCritical]
            internal override FileDataInfo CreateObject(SearchResult result)
            {
                // Win32FindFile
                Win32FindFile.WIN32_FIND_DATA data = result.FindData;
                var build = new StringBuilder(data.cFileName);
                FileDataInfo fi = new FileDataInfo
                {
                    cFileName = build.ToString(),
                    dwFileAttributes = (uint)data.dwFileAttributes,
                    nFileSizeLow = (uint)data.nFileSizeLow,
                    ftLastWriteTime = data.ftLastWriteTime
                };
                return fi;
            }
        }

        // internal sealed class SearchResultEx : SearchResult

        // internal abstract class SearchResultHandler<TSource> -> SearchResultHandlerEx

        internal class StringResultHandler : SearchResultHandlerEx<String>
        {
            private bool _includeFiles;
            private bool _includeDirs;

            internal StringResultHandler(bool includeFiles, bool includeDirs)
            {
                _includeFiles = includeFiles;
                _includeDirs = includeDirs;
            }

            [System.Security.SecurityCritical]
            internal override bool IsResultIncluded(SearchResult result)
            {
                bool includeFile = _includeFiles && FileSystemEnumerableHelpers.IsFile(result.FindData);
                bool includeDir = _includeDirs && FileSystemEnumerableHelpers.IsDir(result.FindData);
                Debug.Assert(!(includeFile && includeDir), result.FindData.cFileName + ": current item can't be both file and dir!");

                return (includeFile || includeDir);
            }

            [System.Security.SecurityCritical]
            internal override String CreateObject(SearchResult result)
            {
                return result.UserPath;
            }
        }

        internal static class FileSystemEnumerableHelpers
        {
            [System.Security.SecurityCritical]  // auto-generated
            internal static bool IsDir(Win32FindFile.WIN32_FIND_DATA data)
            {
                // Don't add "." nor ".."
                return (0 != (data.dwFileAttributes & Win32FindFile.FileAttributes.FILE_ATTRIBUTE_DIRECTORY))
                                                    && !data.cFileName.Equals(".") && !data.cFileName.Equals("..");
            }

            [System.Security.SecurityCritical]  // auto-generated
            internal static bool IsFile(Win32FindFile.WIN32_FIND_DATA data)
            {
                return 0 == (data.dwFileAttributes & Win32FindFile.FileAttributes.FILE_ATTRIBUTE_DIRECTORY);
            }
        }

    }
}


