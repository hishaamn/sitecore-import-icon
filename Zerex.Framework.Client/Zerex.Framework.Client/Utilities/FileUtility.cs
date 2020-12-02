using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Configuration;
using Sitecore.IO;

namespace Zerex.Framework.Client.Utilities
{
    public static class FileUtility
    {
        public static bool DeleteFile(string filename)
        {
            var mainDirectory = new DirectoryInfo(FileUtil.MapPath(Settings.TempFolderPath));

            var filesInDir = mainDirectory.GetFiles("*" + filename + "*.*");

            foreach (var fileInfo in filesInDir)
            {
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception ex)
                {
                    
                }
            }

            var iconCacheDirectory = new DirectoryInfo($"{FileUtil.MapPath(Settings.TempFolderPath)}/IconCache");

            var iconCacheFolder = iconCacheDirectory.GetDirectories(filename);

            foreach (var folderInfo in iconCacheFolder)
            {
                try
                {
                    folderInfo.Delete(true);
                }
                catch (Exception ex)
                {

                }
            }

            return true;
        }
    }
}
