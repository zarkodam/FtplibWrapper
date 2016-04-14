using System;
using System.Collections.Generic;

namespace FtplibWrapper
{
    public interface IFtpHandler : IDisposable
    {
        List<string> GetFoldersListIn(string folder = "/");
        List<string> GetFilesListIn(string folder = "/");
        bool CreateFolder(string newPath);
        bool RenameFile(string currentFilenameWithPath, string newFilenameWithPath);
        bool UploadFile(string localFolder, string filename, string ftpFolder = "", bool replace = true);
        bool DownloadFile(string localFolder, string filename, string ftpFolder = "");
        bool RemoveFolder(string folder);
        bool RemoveFile(string filename, string folder = "");
        void RemoveFolderWithContent(string folder);
    }
}