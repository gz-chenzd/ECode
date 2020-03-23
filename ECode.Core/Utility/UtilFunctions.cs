using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace ECode.Utility
{
    public static class UtilFunctions
    {
        /// <summary>
        /// Fixes path separator, replaces / \ with platform separator char.
        /// </summary>
        public static string PathFix(string path)
        {
            AssertUtil.ArgumentNotEmpty(path, nameof(path));

            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Parses x509 certificate from specified data.
        /// </summary>
        /// <param name="cert">Certificate data.</param>
        /// <returns>Returns parsed certificate.</returns>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>cert</b> is null.</exception>
        public static X509Certificate2 ParseCertificate(object cert)
        {
            AssertUtil.ArgumentNotEmpty((byte[])cert, nameof(cert));

            /* NOTE: MS X509Certificate2((byte[])) has serious bug, it will create temp file
             * and leaves it open. The result is temp folder will get full.
             */
            string tmpFile = Path.GetTempFileName();
            try
            {
                using (var fs = File.Open(tmpFile, FileMode.Open))
                {
                    fs.Write((byte[])cert, 0, ((byte[])cert).Length);
                }

                return new X509Certificate2(tmpFile);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        /// <summary>
        /// Creates a new case-insensitive instance of the <see cref="Hashtable"/> class
        /// with the default initial capacity. 
        /// </summary>
        public static Hashtable CreateCaseInsensitiveHashtable()
        {
            return CollectionsUtil.CreateCaseInsensitiveHashtable();
        }
    }
}