using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ECode.Core;
using ECode.Encoder;
using ECode.Utility;

namespace ECode.Cryptography
{
    public sealed class PemParser
    {
        public static RSAParameters ParseRSAParameters(string pem, string password = null)
        {
            AssertUtil.ArgumentNotEmpty(pem, nameof(pem));

            pem = pem.Trim();
            return ParseRSAParameters(pem, Encoding.ASCII.GetBytes(pem), password);
        }

        public static RSAParameters ParseRSAParameters(FileInfo file, string password = null)
        {
            AssertUtil.ArgumentNotNull(file, nameof(file));

            if (!file.Exists)
            { throw new FileNotFoundException("File cannot be found.", file.FullName); }

            using (var stream = file.OpenRead())
            {
                return ParseRSAParameters(stream, password);
            }
        }

        public static RSAParameters ParseRSAParameters(Stream stream, string password = null)
        {
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read"); }

            var countReaded = 0;
            var buffer = new byte[stream.Length];
            while (countReaded < stream.Length)
            {
                countReaded += stream.Read(buffer, countReaded, (int)(stream.Length - countReaded));
            }

            var pem = Encoding.ASCII.GetString(buffer).Trim();
            return ParseRSAParameters(pem, buffer, password);
        }

        private static RSAParameters ParseRSAParameters(string pem, byte[] buffer, string password = null)
        {
            RSAParameters? parameters = null;
            if (pem.StartsWith(PemReader.BEGIN_STRING, StringComparison.Ordinal))
            {
                parameters = ParsePemKey(pem, password);
            }
            else
            {
                parameters = ParseDerKey(buffer, password);
            }

            if (parameters == null)
            {
                throw new ArgumentException("Cannot parse the rsa key data.");
            }

            return parameters.Value;
        }


        private static RSAParameters? ParsePemKey(string pem, string password = null)
        {
            var reader = new PemReader(new System.IO.StringReader(pem));

            var pemObj = reader.ReadPemObject();
            if (pemObj == null)
            { return null; }

            if (pemObj.Type == "PUBLIC KEY")
            {
                return ParsePublicKey(pemObj.Content);
            }
            else if (pemObj.Type == "RSA PRIVATE KEY")
            {
                byte[] keyBytes = pemObj.Content;

                var headers = new Dictionary<string, string>();
                foreach (PemHeader header in pemObj.Headers)
                {
                    headers[header.Name] = header.Value;
                }

                headers.TryGetValue("Proc-Type", out string procType);
                if (procType == "4,ENCRYPTED")
                {
                    if (string.IsNullOrWhiteSpace(password))
                    { throw new ArgumentNullException(nameof(password)); }

                    var dekItems = headers["DEK-Info"].Split(',');
                    var algItems = dekItems[0].Trim().Split('-');
                    byte[] iv = new HexEncoder().Decode(Encoding.ASCII.GetBytes(dekItems[1]));

                    keyBytes = DecryptRsaPrivateKeyData(keyBytes, algItems, iv, password);
                }

                return ParseRsaPrivateKey(keyBytes);
            }
            else if (pemObj.Type == "PRIVATE KEY")  // nocrypt pkcs8
            {
                return ParsePrivateKey(pemObj.Content);
            }
            else if (pemObj.Type == "ENCRYPTED PRIVATE KEY")  // encrypted pkcs8
            {
                // 测试存在bug
                throw new NotSupportedException("Not yet supported.");
                //return ParseEncryptedPrivateKey(pemObj.Content, password);
            }
            else
            {
                return null;
            }
        }

        private static RSAParameters? ParseDerKey(byte[] buffer, string password = null)
        {
            var parameters = ParsePublicKey(buffer);
            if (parameters != null)
            {
                return parameters;
            }

            parameters = ParseRsaPrivateKey(buffer);
            if (parameters != null)
            {
                return parameters;
            }

            parameters = ParsePrivateKey(buffer);  // nocrypt pkcs8
            if (parameters != null)
            {
                return parameters;
            }

            // 测试存在bug
            parameters = ParseEncryptedPrivateKey(buffer, password);  // encrypted pkcs8
            if (parameters != null)
            {
                return parameters;
            }

            return null;
        }

        private static RSAParameters? ParsePublicKey(byte[] buffer)
        {
            // Encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                var twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130)
                {   //data read as little endian order (actual data order for Sequence is 30 81)
                    reader.ReadByte();       //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();      //advance 2 bytes
                }
                else
                {
                    return null;
                }

                var seq = reader.ReadBytes(15);        //read the Sequence OID
                if (!ArrayUtil.AreEqual(seq, SeqOID))
                {   //make sure Sequence for OID is correct
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes == 0x8103)
                {   //data read as little endian order (actual data order for Bit String is 03 81)
                    reader.ReadByte();       //advance 1 byte
                }
                else if (twobytes == 0x8203)
                {
                    reader.ReadInt16();      //advance 2 bytes
                }
                else
                {
                    return null;
                }

                var b = reader.ReadByte();
                if (b != 0x00)
                {   //expect null byte next
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130)
                {   //data read as little endian order (actual data order for Sequence is 30 81)
                    reader.ReadByte();       //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();      //advance 2 bytes
                }
                else
                {
                    return null;
                }

                byte low = 0x00;
                byte high = 0x00;
                twobytes = reader.ReadUInt16();
                if (twobytes == 0x8102)
                {   //data read as little endian order (actual data order for Integer is 02 81)
                    low = reader.ReadByte();     // read next bytes which is bytes in modulus
                }
                else if (twobytes == 0x8202)
                {
                    high = reader.ReadByte();    //advance 2 bytes
                    low = reader.ReadByte();
                }
                else
                {
                    return null;
                }

                byte[] modint = { low, high, 0x00, 0x00 };    //reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                byte first = reader.ReadByte();
                reader.BaseStream.Seek(-1, SeekOrigin.Current);

                if (first == 0x00)
                {   //if first byte (highest order) of modulus is zero, don't include it
                    reader.ReadByte();      //skip this null byte
                    modsize -= 1;           //reduce modulus buffer size by 1
                }

                var modulus = reader.ReadBytes(modsize);    //read the modulus bytes

                if (reader.ReadByte() != 0x02)
                {   //expect an Integer for the exponent data
                    return null;
                }

                int expbytes = reader.ReadByte();    // should only need one byte for actual exponent data (for all useful values)
                var exponent = reader.ReadBytes(expbytes);

                var parameters = new RSAParameters { Modulus = modulus, Exponent = exponent };

                // validate rsa parameters
                using (RSA rsa = RSA.Create())
                { rsa.ImportParameters(parameters); }

                return parameters;
            }
        }

        private static RSAParameters? ParseRsaPrivateKey(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                var twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130)
                {   //data read as little endian order (actual data order for Sequence is 30 81)
                    reader.ReadByte();       //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();      //advance 2 bytes
                }
                else
                {
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes != 0x0102)
                {   //version number
                    return null;
                }

                var b = reader.ReadByte();
                if (b != 0x00)
                {
                    return null;
                }

                int size = GetIntegerSize(reader);
                var modulus = reader.ReadBytes(size);

                size = GetIntegerSize(reader);
                var e = reader.ReadBytes(size);

                size = GetIntegerSize(reader);
                var d = reader.ReadBytes(size);

                size = GetIntegerSize(reader);
                var p = reader.ReadBytes(size);

                size = GetIntegerSize(reader);
                var q = reader.ReadBytes(size);

                size = GetIntegerSize(reader);
                var dp = reader.ReadBytes(size);

                size = GetIntegerSize(reader);
                var dq = reader.ReadBytes(size);

                size = GetIntegerSize(reader);
                var iq = reader.ReadBytes(size);

                var parameters = new RSAParameters
                {
                    Modulus = modulus,
                    Exponent = e,
                    D = d,
                    P = p,
                    Q = q,
                    DP = dp,
                    DQ = dq,
                    InverseQ = iq
                };

                // validate rsa parameters
                using (RSA rsa = RSA.Create())
                { rsa.ImportParameters(parameters); }

                return parameters;
            }
        }

        private static RSAParameters? ParsePrivateKey(byte[] buffer)
        {
            // Encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                var twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130)
                {   //data read as little endian order (actual data order for Sequence is 30 81)
                    reader.ReadByte();       //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();      //advance 2 bytes
                }
                else
                {
                    return null;
                }

                var onebyte = reader.ReadByte();
                if (onebyte != 0x02)
                {
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes != 0x0001)
                {
                    return null;
                }

                var seq = reader.ReadBytes(15);        //read the Sequence OID
                if (!ArrayUtil.AreEqual(seq, SeqOID))
                {   //make sure Sequence for OID is correct
                    return null;
                }

                onebyte = reader.ReadByte();
                if (onebyte != 0x04)
                {   //expect an Octet string 
                    return null;
                }

                onebyte = reader.ReadByte();    //read next byte, or next 2 bytes is 0x81 or 0x82; otherwise bt is the byte count
                if (onebyte == 0x81)
                {
                    reader.ReadByte();
                }
                else if (onebyte == 0x82)
                {
                    reader.ReadUInt16();
                }

                //at this stage, the remaining sequence should be the RSA private key
                return ParseRsaPrivateKey(reader.ReadBytes((int)(buffer.Length - stream.Position)));
            }
        }

        private static RSAParameters? ParseEncryptedPrivateKey(byte[] buffer, string password)
        {
            // Encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] OIDpkcs5PBES2 = { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x05, 0x0D };
            byte[] OIDpkcs5PBKDF2 = { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x05, 0x0C };
            byte[] OIDdesEDE3CBC = { 0x06, 0x08, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x03, 0x07 };

            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                var twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130)
                {   //data read as little endian order (actual data order for Sequence is 30 81)
                    reader.ReadByte();       //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();      //advance 2 bytes
                }
                else
                {
                    return null;
                }

                twobytes = reader.ReadUInt16();    //inner sequence
                if (twobytes == 0x8130)
                {
                    reader.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();
                }

                var seq = reader.ReadBytes(11);               //read the Sequence OID
                if (!ArrayUtil.AreEqual(seq, OIDpkcs5PBES2))
                {   //is it a OIDpkcs5PBES2 ?
                    return null;
                }

                twobytes = reader.ReadUInt16();    //inner sequence for pswd salt
                if (twobytes == 0x8130)
                {
                    reader.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();
                }

                twobytes = reader.ReadUInt16();    //inner sequence for pswd salt
                if (twobytes == 0x8130)
                {
                    reader.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();
                }

                seq = reader.ReadBytes(11);                       //read the Sequence OID
                if (!ArrayUtil.AreEqual(seq, OIDpkcs5PBKDF2))
                {   //is it a OIDpkcs5PBKDF2 ?
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130)
                {
                    reader.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();
                }

                var onebyte = reader.ReadByte();
                if (onebyte != 0x04)
                {   //expect octet string for salt
                    return null;
                }

                onebyte = reader.ReadByte();
                var salt = reader.ReadBytes(onebyte);

                onebyte = reader.ReadByte();
                if (onebyte != 0x02)
                {   //expect an integer for PBKF2 interation count
                    return null;
                }

                int iterations = 0;
                onebyte = reader.ReadByte();    //PBKD2 iterations should fit in 2 bytes.
                if (onebyte == 1)
                {
                    iterations = reader.ReadByte();
                }
                else if (onebyte == 2)
                {
                    iterations = 256 * reader.ReadByte() + reader.ReadByte();
                }
                else
                {
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130)
                {
                    reader.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16();
                }

                seq = reader.ReadBytes(10);                      //read the Sequence OID
                if (!ArrayUtil.AreEqual(seq, OIDdesEDE3CBC))
                {   //is it a OIDdes-EDE3-CBC ?
                    return null;
                }

                onebyte = reader.ReadByte();
                if (onebyte != 0x04)
                {   //expect octet string for IV
                    return null;
                }

                onebyte = reader.ReadByte();    // IV byte size should fit in one byte (24 expected for 3DES)
                var iv = reader.ReadBytes(onebyte);

                onebyte = reader.ReadByte();
                if (onebyte != 0x04)
                {   // expect octet string for encrypted PKCS8 data
                    return null;
                }

                int encblobsize = 0;
                onebyte = reader.ReadByte();
                if (onebyte == 0x81)
                {
                    encblobsize = reader.ReadByte();    // data size in next byte
                }
                else if (onebyte == 0x82)
                {
                    encblobsize = 256 * reader.ReadByte() + reader.ReadByte();
                }
                else
                {
                    encblobsize = onebyte;              // we already have the data size
                }

                var encryptData = reader.ReadBytes(encblobsize);
                var decryptData = DecryptPrivateKeyData(encryptData, salt, iv, password, iterations);
                if (decryptData == null)
                {   // probably a bad pswd entered.
                    return null;
                }

                return ParsePrivateKey(decryptData);
            }
        }

        private static byte[] DecryptPrivateKeyData(byte[] buffer, byte[] salt, byte[] iv, string password, int iterations)
        {
            using (var kd = new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes(password), salt, iterations))
            {
                var provider = new SymmetricCrypto(SymmetricAlgName.TripleDES);
                provider.Key = kd.GetBytes(24);
                provider.IV = iv;

                return provider.Decrypt(buffer);
            }
        }

        private static byte[] DecryptRsaPrivateKeyData(byte[] buffer, string[] algItems, byte[] iv, string password)
        {
            int requiredKeyLength = 0;
            SymmetricCrypto provider = null;

            if (algItems[0] == "AES")
            {
                provider = new SymmetricCrypto(SymmetricAlgName.AES);
                provider.Mode = (CipherMode)Enum.Parse(typeof(CipherMode), algItems[algItems.Length - 1]);

                requiredKeyLength = int.Parse(algItems[1]) / 8;
            }
            else if (algItems[1] == "EDE3")
            {
                provider = new SymmetricCrypto(SymmetricAlgName.TripleDES);

                requiredKeyLength = 24;
            }
            else
            {
                provider = new SymmetricCrypto(SymmetricAlgName.DES);

                requiredKeyLength = 8;
            }

            var keyBytes = new List<byte>();
            keyBytes.AddRange(Encoding.ASCII.GetBytes(password));
            keyBytes.AddRange(iv.Take(8));

            var keys = new List<byte>();
            var block = new List<byte>();
            while (keys.Count() < requiredKeyLength)
            {
                block.AddRange(keyBytes);
                keys.AddRange(block.ToArray().ComputeMD5());

                block.Clear();
                block.AddRange(keys);
            }

            provider.IV = iv;
            provider.Key = keys.Take(requiredKeyLength).ToArray();

            return provider.Decrypt(buffer);
        }

        private static int GetIntegerSize(BinaryReader reader)
        {
            int count = 0;
            byte b = reader.ReadByte();
            if (b != 0x02)
            {   //expect integer
                return 0;
            }

            b = reader.ReadByte();
            if (b == 0x81)
            {
                count = reader.ReadByte();    // data size in next byte
            }
            else if (b == 0x82)
            {
                var high = reader.ReadByte(); // data size in next 2 bytes
                var low = reader.ReadByte();
                byte[] modint = { low, high, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = b;     // we already have the data size
            }

            while (reader.ReadByte() == 0x00)
            {   //remove high order zeros in data
                count -= 1;
            }

            reader.BaseStream.Seek(-1, SeekOrigin.Current);    //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }
    }
}