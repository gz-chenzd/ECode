using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECode.Core;

namespace ECode.Utility
{
    public static class HtpasswdUtil
    {
        public enum Algorithm
        {
            MD5,

            SHA1
        }


        static string itoa64 = "./0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";


        static string GenerateSalt(int v, int num)
        {
            string salt = "";
            while (--num >= 0)
            {
                salt += itoa64[v & 0x3f];
                v >>= 6;
            }

            return salt;
        }

        public static string Create(string userName, string password, Algorithm algorithm = Algorithm.MD5)
        {
            AssertUtil.ArgumentNotEmpty(userName, nameof(userName));
            AssertUtil.ArgumentNotEmpty(password, nameof(password));

            var random = new Random((int)DateTime.Now.Ticks);
            var salt = GenerateSalt(random.Next(16777215, int.MaxValue), 4)
                     + GenerateSalt(random.Next(16777215, int.MaxValue), 4);

            string cpw = "";
            switch (algorithm)
            {
                case Algorithm.SHA1:
                    cpw = "{SHA}" + Encoding.ASCII.GetBytes(password).ComputeSHA1().ToBase64();
                    break;

                default:
                    var buffer = new List<byte>(Encoding.ASCII.GetBytes(password + "$apr1$" + salt));
                    var hashBytes = Encoding.ASCII.GetBytes(password + salt + password).ComputeMD5();
                    buffer.AddRange(hashBytes.Take(password.Length));

                    for (var i = password.Length; i != 0; i >>= 1)
                    {
                        if ((i & 1) != 0)
                        { buffer.Add((byte)0); }
                        else
                        { buffer.Add((byte)password[0]); }
                    }

                    List<byte> temp = null;
                    hashBytes = buffer.ToArray().ComputeMD5();

                    for (var i = 0; i < 1000; i++)
                    {
                        temp = new List<byte>();

                        if ((i & 1) != 0)
                        { temp.AddRange(Encoding.ASCII.GetBytes(password)); }
                        else
                        { temp.AddRange(hashBytes.Take(16)); }

                        if ((i % 3) != 0)
                        { temp.AddRange(Encoding.ASCII.GetBytes(salt)); }

                        if ((i % 7) != 0)
                        { temp.AddRange(Encoding.ASCII.GetBytes(password)); }

                        if ((i & 1) != 0)
                        { temp.AddRange(hashBytes.Take(16)); }
                        else
                        { temp.AddRange(Encoding.ASCII.GetBytes(password)); }

                        hashBytes = temp.ToArray().ComputeMD5();
                    }

                    cpw = "$apr1$" + salt + '$';
                    cpw += GenerateSalt((hashBytes[0] << 16) | (hashBytes[6] << 8) | hashBytes[12], 4);
                    cpw += GenerateSalt((hashBytes[1] << 16) | (hashBytes[7] << 8) | hashBytes[13], 4);
                    cpw += GenerateSalt((hashBytes[2] << 16) | (hashBytes[8] << 8) | hashBytes[14], 4);
                    cpw += GenerateSalt((hashBytes[3] << 16) | (hashBytes[9] << 8) | hashBytes[15], 4);
                    cpw += GenerateSalt((hashBytes[4] << 16) | (hashBytes[10] << 8) | hashBytes[5], 4);
                    cpw += GenerateSalt(hashBytes[11], 2);
                    break;
            }

            if (userName.Length + 1 + cpw.Length > 255)
            { throw new DataSizeExceededException($"The computing result is too long (> 255)."); }

            return userName + ':' + cpw;
        }
    }
}