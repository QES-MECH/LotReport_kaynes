using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LotReport.Utilities;
using Renci.SshNet;

namespace LotReport.Models
{
    public class LotSSH
    {
        public LotSSH(string host, ushort port, string username, string password, string directory)
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            Directory = directory;
        }

        public string Host { get; }

        public ushort Port { get; }

        public string Username { get; }

        public string Password { get; }

        public string Directory { get; }

        public void SftpUpload(LotData lotData, string localBaseDirectory)
        {
            localBaseDirectory = Path.GetFullPath(localBaseDirectory);
            string absolutePath = lotData.FileInfo.Directory.FullName.Replace(localBaseDirectory, string.Empty);
            string relativePath = absolutePath.Substring(1);

            var connectionInfo = new ConnectionInfo(
                Host,
                Port,
                Username,
                new PasswordAuthenticationMethod(Username, Password));

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.ChangeDirectory(Directory);

                string remoteBaseDirectory = Path.Combine(client.WorkingDirectory, relativePath);
                client.CreateDirectoryRecursively(remoteBaseDirectory);

                foreach (FileInfo fi in lotData.FileInfo.Directory.GetFiles())
                {
                    using (FileStream fs = new FileStream(fi.FullName, FileMode.Open))
                    {
                        client.UploadFile(fs, Path.Combine(remoteBaseDirectory, fi.Name), true);
                    }
                }
            }
        }
    }
}
