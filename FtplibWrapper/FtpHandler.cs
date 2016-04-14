using FtpLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FtplibWrapper
{
    public class FtpHandler : IFtpHandler
    {

        #region Fields

        private readonly FtpConnection _ftp;
        private readonly List<string> _foldersListToRemove = new List<string>();

        #endregion


        #region Cotr


        public FtpHandler(string ftpServer, string ftpUsername, string ftpPass, int port = 21)
        {
            try
            {
                _ftp = new FtpConnection(ftpServer, port, ftpUsername, ftpPass);
                _ftp.Open();
                _ftp.Login();
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        #endregion


        #region List

        public List<string> GetFoldersListIn(string folder = "/")
        {
            var foldersList = new List<string>();

            try
            {
                if (_ftp.DirectoryExists(folder))
                    foldersList.AddRange(_ftp.GetDirectories(folder)
                        .Where(i => !i.Name.Equals(".") && !i.Name.Equals(".."))
                        .Select(i => i.Name));
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
            }

            return foldersList;
        }

        public List<string> GetFilesListIn(string folder = "/")
        {
            var filesList = new List<string>();

            try
            {
                if (_ftp.DirectoryExists(folder))
                    filesList.AddRange(_ftp.GetFiles(folder).Select(i => i.Name));
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
            }

            return filesList;
        }

        #endregion


        #region Create

        public bool CreateFolder(string newPath)
        {
            try
            {
                if (!_ftp.DirectoryExists(newPath))
                    _ftp.CreateDirectory(newPath);
                return true;
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
                return false;
            }
        }

        #endregion


        #region Rename

        public bool RenameFile(string currentFilenameWithPath, string newFilenameWithPath)
        {
            try
            {
                if (!_ftp.FileExists(currentFilenameWithPath)) return false;

                _ftp.RenameFile(currentFilenameWithPath, newFilenameWithPath);
                return true;
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
                return false;
            }
        }

        #endregion


        #region UploadAndDownload


        public bool UploadFile(string localFolder, string filename, string ftpFolder = "", bool replace = true)
        {
            try
            {
                string fixedFtpFolder = string.IsNullOrWhiteSpace(ftpFolder) ? "/" : ftpFolder;

                if (!_ftp.DirectoryExists(ftpFolder) && !string.IsNullOrWhiteSpace(ftpFolder)) _ftp.CreateDirectory(ftpFolder);

                _ftp.SetCurrentDirectory(fixedFtpFolder);

                string ftpFilePath = string.Concat(ftpFolder, "/", filename);
                string localFilePath = string.Concat(localFolder, "\\", filename);

                if (_ftp.FileExists(ftpFilePath))
                {
                    if (!replace) return false;

                    _ftp.RemoveFile(ftpFilePath);
                    _ftp.PutFile(localFilePath);

                    return true;
                }

                _ftp.PutFile(localFilePath);
                return true;
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
                return false;
            }
        }

        public bool DownloadFile(string localFolder, string filename, string ftpFolder = "")
        {
            try
            {
                string fixedFtpFolder = string.IsNullOrWhiteSpace(ftpFolder) ? "/" : ftpFolder;

                if (!_ftp.DirectoryExists(fixedFtpFolder)) return false;

                _ftp.SetCurrentDirectory(fixedFtpFolder);

                string ftpFilePath = string.Concat(ftpFolder, "/", filename);

                if (!_ftp.FileExists(ftpFilePath)) return false;

                string localFilePath = string.Concat(localFolder, "\\", filename);

                _ftp.GetFile(ftpFilePath, localFilePath, false);

                return true;
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
                return false;
            }
        }

        #endregion


        #region Remove

        public bool RemoveFolder(string folder)
        {
            try
            {
                if (!_ftp.DirectoryExists(folder)) return false;

                _ftp.RemoveDirectory(folder);
                return true;
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
                return false;
            }
        }

        public bool RemoveFile(string filename, string folder = "")
        {
            try
            {
                if (!_ftp.DirectoryExists(folder)) return false;

                string ftpFilePath = string.Concat(folder, "/", filename);

                if (!_ftp.FileExists(ftpFilePath)) return false;

                _ftp.RemoveFile(ftpFilePath);
                return true;
            }
            catch (FtpException ex)
            {
                Debug.WriteLine($"FTP Error: {ex.ErrorCode} {ex.Message}");
                return false;
            }
        }

        private void RemoveFiles()
        {
            string currentFolder = _foldersListToRemove.Last();

            foreach (var file in _ftp.GetFiles(currentFolder))
                RemoveFile(file.Name, currentFolder);

            foreach (FtpDirectoryInfo directory in _ftp.GetDirectories(currentFolder)
                .Where(directory => directory.Name != "." && directory.Name != ".."))
            {
                _foldersListToRemove.Add(currentFolder + "/" + directory.Name);
                RemoveFiles();
            }
        }

        public void RemoveFolderWithContent(string folder)
        {
            _foldersListToRemove.Add(folder);

            RemoveFiles();

            for (int i = _foldersListToRemove.Count - 1; i >= 0; i--)
                RemoveFolder(_foldersListToRemove[i]);
        }

        #endregion


        #region Helpers

        private static string StringToUtf8(string stringFromConvert)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(stringFromConvert));
        }

        #endregion


        #region Dispose

        public void Dispose()
        {
            if (_ftp == null) return;

            _ftp.Close();
            _ftp.Dispose();
        }

        #endregion
    }
}
