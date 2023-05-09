using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using LotReport.Utilities;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace LotReport.Models
{
    public class LotSSH
    {
        public LotSSH(string host, ushort port, string username, string password, string privateKeyFileName, string directory)
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            PrivateKeyFileName = privateKeyFileName;
            Directory = directory;
        }

        public string Host { get; }

        public ushort Port { get; }

        public string Username { get; }

        public string Password { get; }

        public string PrivateKeyFileName { get; }

        public string Directory { get; }

        public void SftpUpload(LotData lotData, string localBaseDirectory)
        {
            localBaseDirectory = Path.GetFullPath(localBaseDirectory);
            string absolutePath = lotData.FileInfo.Directory.FullName.Replace(localBaseDirectory, string.Empty);
            string relativePath = absolutePath.Substring(1);

            var authenticationMethods = new List<AuthenticationMethod>();

            if (!string.IsNullOrEmpty(Password))
            {
                authenticationMethods.Add(new PasswordAuthenticationMethod(Username, Password));
            }

            var regularKey = File.ReadAllBytes(PrivateKeyFileName);
            var pk = new PrivateKeyFile(new MemoryStream(regularKey));
            RsaSha256Util.ConvertToKeyWithSha256Signature(pk);

            using (var authenticationMethodRsa = new PrivateKeyAuthenticationMethod(Username, pk))
            {
                authenticationMethods.Add(authenticationMethodRsa);

                var connectionInfo = new ConnectionInfo(
                    Host,
                    Port,
                    Username,
                    authenticationMethodRsa);
                RsaSha256Util.SetupConnection(connectionInfo);

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

        /// <summary>
        /// Utility class which allows ssh.net to connect to servers using ras-sha2-256
        /// </summary>
        public static class RsaSha256Util
        {
            public static void SetupConnection(ConnectionInfo connection)
            {
                connection.HostKeyAlgorithms["rsa-sha2-256"] = data => new KeyHostAlgorithm("rsa-sha2-256", new RsaKey(), data);
            }

            /// <summary>
            /// Converts key file to rsa key with sha2-256 signature
            /// Due to lack of constructor: https://github.com/sshnet/SSH.NET/blob/bc99ada7da3f05f50d9379f2644941d91d5bf05a/src/Renci.SshNet/PrivateKeyFile.cs#L86
            /// We do that in place
            /// </summary>
            /// <param name="keyFile">Private Key file</param>
            /// <exception cref="ArgumentNullException">keyFile is not a KeyHostAlgorithm -or- RsaKey</exception>
            public static void ConvertToKeyWithSha256Signature(PrivateKeyFile keyFile)
            {
                var oldKeyHostAlgorithm = keyFile.HostKey as KeyHostAlgorithm;
                if (oldKeyHostAlgorithm == null)
                {
                    throw new ArgumentNullException(nameof(oldKeyHostAlgorithm));
                }

                var oldRsaKey = oldKeyHostAlgorithm.Key as RsaKey;
                if (oldRsaKey == null)
                {
                    throw new ArgumentNullException(nameof(oldRsaKey));
                }

                var newRsaKey = new RsaWithSha256SignatureKey(oldRsaKey.Modulus, oldRsaKey.Exponent, oldRsaKey.D, oldRsaKey.P, oldRsaKey.Q, oldRsaKey.InverseQ);

                UpdatePrivateKeyFile(keyFile, newRsaKey);
            }

            private static void UpdatePrivateKeyFile(PrivateKeyFile keyFile, RsaWithSha256SignatureKey key)
            {
                var keyHostAlgorithm = new KeyHostAlgorithm(key.ToString(), key);

                var hostKeyProperty = typeof(PrivateKeyFile).GetProperty(nameof(PrivateKeyFile.HostKey));
                hostKeyProperty.SetValue(keyFile, keyHostAlgorithm);

                var keyField = typeof(PrivateKeyFile).GetField("_key", BindingFlags.NonPublic | BindingFlags.Instance);
                keyField.SetValue(keyFile, key);
            }
        }

        /// <summary>
        /// Based on https://github.com/sshnet/SSH.NET/blob/1d5d58e17c68a2f319c51e7f938ce6e964498bcc/src/Renci.SshNet/Security/Cryptography/RsaDigitalSignature.cs#L12
        ///
        /// With following changes:
        ///
        /// - OID changed to sha2-256
        /// - hash changed from sha1 to sha2-256
        /// </summary>
        public class RsaSha256DigitalSignature : CipherDigitalSignature, IDisposable
        {
            private bool _isDisposed;
            private HashAlgorithm _hash;

            // custom OID
            public RsaSha256DigitalSignature(RsaWithSha256SignatureKey rsaKey)
                : base(new ObjectIdentifier(2, 16, 840, 1, 101, 3, 4, 2, 1), new RsaCipher(rsaKey))
            {
                // custom
                _hash = SHA256.Create();
            }

            ~RsaSha256DigitalSignature()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected override byte[] Hash(byte[] input)
            {
                return _hash.ComputeHash(input);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_isDisposed)
                {
                    return;
                }

                if (disposing)
                {
                    var hash = _hash;
                    if (hash != null)
                    {
                        hash.Dispose();
                        _hash = null;
                    }

                    _isDisposed = true;
                }
            }
        }

        public class RsaWithSha256SignatureKey : RsaKey
        {
            private RsaSha256DigitalSignature _digitalSignature;

            public RsaWithSha256SignatureKey(BigInteger modulus, BigInteger exponent, BigInteger d, BigInteger p, BigInteger q, BigInteger inverseQ)
                : base(modulus, exponent, d, p, q, inverseQ)
            {
            }

            protected override DigitalSignature DigitalSignature
            {
                get
                {
                    if (_digitalSignature == null)
                    {
                        _digitalSignature = new RsaSha256DigitalSignature(this);
                    }

                    return _digitalSignature;
                }
            }

            public override string ToString()
            {
                return "rsa-sha2-256";
            }
        }
    }
}
