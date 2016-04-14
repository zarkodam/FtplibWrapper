# FtplibWrapper
Wrapper over Ftplib, elementary FTP actions

**How to use:**

   ```sh
     using (IFtpHandler ftp = new FtpHandler("ftp.server.net", "username", "pass", port: 21))
            {
                // / = ftp root
                // /someFolder = some folder in root

                const string localRoot = "D:\\";
                const string ftpRoot = "/testFolder";
                const string ftpTestFolder = ftpRoot + "/ftpTest";


                if (ftp.CreateFolder(ftpTestFolder))
                {
                    Console.WriteLine("ftpTest folder created!");

                    const string ftpTestFile = "ftpTestFile.txt";

                    if (ftp.UploadFile(localRoot, ftpTestFile, ftpTestFolder, replace: true))
                        Console.WriteLine($"file {ftpTestFile} from {localRoot} uploaded to {ftpTestFolder} folder");

                    string currentFilenameWithPath = string.Concat(ftpTestFolder, "/", ftpTestFile);
                    string newFilenameWithPath = string.Concat(ftpTestFolder, "/", "ftpTestFileRENAMED.txt");

                    if (ftp.RenameFile(currentFilenameWithPath, newFilenameWithPath))
                        Console.WriteLine($"{newFilenameWithPath} renamed to: {newFilenameWithPath}");

                    const string ftpContentFolder = ftpTestFolder + "/content";

                    if (ftp.CreateFolder(ftpContentFolder))
                    {
                        Console.WriteLine($"Folder: {ftpContentFolder} created!");

                        string filesForUploadToFtp = string.Concat(localRoot, "FtpTest");

                        foreach (string filePath in Directory.EnumerateFiles(filesForUploadToFtp, "*.*"))
                        {
                            string folder = Path.GetDirectoryName(filePath);
                            string filename = Path.GetFileName(filePath);

                            if (ftp.UploadFile(folder, filename, ftpContentFolder))
                                Console.WriteLine($"File: {filePath} uploaded!");
                        }

                        Console.WriteLine("Downloading uploaded files...");

                        IEnumerable<string> filesInFtpContentFolder = ftp.GetFilesListIn(ftpContentFolder);

                        const string downloadDestination = localRoot + "FtpDownloads";

                        foreach (var filename in filesInFtpContentFolder)
                            if (ftp.DownloadFile(downloadDestination, filename, ftpContentFolder))
                                Console.WriteLine($"File {filename} from folder {ftpContentFolder} downloaded in {filesForUploadToFtp}");
                    }

                    ftp.RemoveFolderWithContent(ftpTestFolder);
                    Console.WriteLine($"Folder {ftpTestFolder} removed!");
                }

```