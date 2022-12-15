using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace LotReport.Utilities
{
    public static class SftpClientExtensions
    {
        public static void CreateDirectoryRecursively(this SftpClient client, string path)
        {
            path = path.Replace("\\", "/");

            string current = string.Empty;

            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            while (!string.IsNullOrEmpty(path))
            {
                int p = path.IndexOf('/');
                current += '/';
                if (p >= 0)
                {
                    current += path.Substring(0, p);
                    path = path.Substring(p + 1);
                }
                else
                {
                    current += path;
                    path = string.Empty;
                }

                if (client.Exists(current))
                {
                    SftpFileAttributes attrs = client.GetAttributes(current);
                    if (!attrs.IsDirectory)
                    {
                        throw new Exception($"The path '{current}' is not a directory.");
                    }
                }
                else
                {
                    client.CreateDirectory(current);
                }
            }
        }
    }
}
