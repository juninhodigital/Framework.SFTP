using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Renci.SshNet;

namespace Framework.SFTP
{
    /// <summary>
    /// Class that handles SSH File Transfer Protocol operations (SFTP)
    /// </summary>
    public sealed class SFTP : IDisposable
    {
        #region| Fields |  

        private string userName    = string.Empty;
        private string passWord    = string.Empty;
        private string hostAddress = string.Empty;
        private int portNumber     = 21;
        private int timeOut        = 20000;
        private bool usePassive    = true;

        #endregion

        #region| Constructor |

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="HostAddress">HostAddress</param>
        /// <param name="UserName">UserName</param>
        /// <param name="PassWord">PassWord</param>
        /// <param name="PortNumber">PortNumber</param>
        /// <param name="Timeout">Gets or sets the number of milliseconds to wait for a request. Default value is 20000</param>
        /// <param name="UsePassive">Gets or sets the behavior of a client application's data transfer process. Default value is true</param>
        public SFTP(string HostAddress, string UserName, string PassWord, int PortNumber, int Timeout = 20000, bool UsePassive = true)
        {
            this.userName    = UserName;
            this.passWord    = PassWord;
            this.hostAddress = HostAddress;
            this.portNumber  = PortNumber;
            this.timeOut     = Timeout;
            this.usePassive  = UsePassive;
        }

        #endregion

        #region| Methods |   

        /// <summary>
        /// This method downloads the SFTP file specified by SftpPath and saves it to FilePath.
        /// Throws a WebException on encountering a network error.
        /// </summary>
        /// <param name="SftpPath">SFTP Path with the file name (Example: "/synchro/incoming/sample.txt")</param>
        /// <param name="FilePath">Entire File Path where the file will be downloaded (Example: c:\sample.txt")</param>
        public bool DownloadFile(string SftpPath, string FileName)
        {
            var output = false;

            var connection = GetConnection();

            using (var sftp = new SftpClient(connection))
            {
                sftp.Connect();

                if(sftp.Exists(SftpPath))
                {
                    output = true;

                    //sftp.ChangeDirectory(FtpPath);

                    using (Stream oFile = File.OpenWrite(FileName))
                    {
                        sftp.DownloadFile(SftpPath, oFile);
                    }
                }              

                sftp.Disconnect();
            }

            connection = null;

            return output;

        }

        /// <summary>
        /// Load a file from disk and upload it to the SFTP server
        /// </summary>
        /// <param name="SftpPath">SFTP Path with the file name (Example: "/synchro/incoming/sample.txt")</param>
        /// <param name="FilePath">File on the local Hard Disk to upload (Example: "c:\sample.txt")</param>
        /// <returns>The server response in a byte[]</returns>
        public void UploadFile(string SftpPath, string FilePath)
        {
            var connection = GetConnection();

            using (var sftp = new SftpClient(connection))
            {
                sftp.Connect();

                using (var fileStream = new FileStream(FilePath, FileMode.Open))
                {
                    sftp.UploadFile(fileStream, SftpPath);
                }                    

                sftp.Disconnect();
            }

            connection = null;

        }

        /// <summary>
        /// Retrieve the List of files on the SFTP server
        /// </summary>
        /// <param name="SftpPath">SFTP directory Path. Example ("/synchro/outgoing")</param>
        /// <returns>List of files on the SFTP server</returns>
        public List<string> GetFiles(string SftpPath)
        {
            var output = new List<string>();

            var connection = GetConnection();

            using (var sftp = new SftpClient(connection))
            {
                sftp.Connect();

                var fileList = sftp.ListDirectory(SftpPath).ToList();

                if(fileList!=null && fileList.Count()>0)
                {
                    foreach (var file in fileList)
                    {
                        output.Add(file.Name);
                    }
                }

                sftp.Disconnect();
            }

            connection = null;

            return output;
        }

        /// <summary>
        /// Delete a file on the SFTP server
        /// </summary>
        /// <param name="SftpPath">File path on the SFTP server (Example: "/synchro/incoming/sample.txt")</param>
        /// <returns>True if file was delete sucessfuly</returns>
        public void DeleteFile(string SftpPath)
        {
            var connection = GetConnection();

            using (var sftp = new SftpClient(connection))
            {
                sftp.Connect();

                sftp.DeleteFile(SftpPath);

                sftp.Disconnect();
            }

            connection = null;
        }

        /// <summary>
        /// Get the connection object in order to stabilish a connection to the SFTP
        /// </summary>
        /// <returns></returns>
        private ConnectionInfo GetConnection()
        {
            // Setup Credentials and Server Information
            var output = new ConnectionInfo(this.hostAddress, this.portNumber, this.userName, new AuthenticationMethod[] 
            {
                    // Password based Authentication
                    new PasswordAuthenticationMethod(this.userName,this.passWord),

            });

            return output;
        }


        #endregion

        #region| IDisposable |

        /// <summary>
        /// Release allocated resources
        /// </summary>
        public void Dispose()
        {

        }

        #endregion
    }
}