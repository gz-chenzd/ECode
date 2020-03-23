using System;
using System.Diagnostics;
using System.Linq;
using ECode.Utility;

namespace ECode.Core
{
    public static class ObjectId
    {
        static readonly char[]      ENCODE_TABLE = {
            (char)'0', (char)'1', (char)'2', (char)'3', (char)'4',
            (char)'5', (char)'6', (char)'7', (char)'8', (char)'9',
            (char)'a', (char)'b', (char)'c', (char)'d', (char)'e', (char)'f'
        };


        static ushort       serialNum               = 0;
        static uint         longSerialNum           = 0;
        static char[]       hostNameHashCode        = new char[8];
        static char[]       currentProcessId        = new char[4];


        static ObjectId()
        {
            var hashCode = NetworkUtil.GetHostName().GetHashCode();
            var bytes = BitConverter.GetBytes(hashCode).Reverse().ToArray();
            for (int i = 0; i < 4; i++)
            {
                hostNameHashCode[i * 2] = ENCODE_TABLE[bytes[i] / 0x10];
                hostNameHashCode[i * 2 + 1] = ENCODE_TABLE[bytes[i] % 0x10];
            }

            var processId = (ushort)Process.GetCurrentProcess().Id;
            bytes = BitConverter.GetBytes(processId).Reverse().ToArray();
            for (int i = 0; i < 2; i++)
            {
                currentProcessId[i * 2] = ENCODE_TABLE[bytes[i] / 0x10];
                currentProcessId[i * 2 + 1] = ENCODE_TABLE[bytes[i] % 0x10];
            }
        }


        static uint GetTimeStamp()
        {
            return (uint)DateTime.Now.ToUnixTimeStamp();
        }

        static ulong GetLongTimeStamp()
        {
            return (ulong)DateTime.Now.ToLongUnixTimeStamp();
        }

        static ushort NextSerialNumber()
        {
            return ++serialNum;
        }

        static uint NextLongSerialNumber()
        {
            return ++longSerialNum;
        }


        public static string NewId()
        {
            var chArray  = new char[24];

            // timestamp part
            var bytes = BitConverter.GetBytes(GetTimeStamp());
            for (int i = 3; i >= 0; i--)
            {
                chArray[6 - (i * 2)] = ENCODE_TABLE[bytes[i] / 0x10];
                chArray[7 - (i * 2)] = ENCODE_TABLE[bytes[i] % 0x10];
            }

            // hostname part
            for (int i = 0; i < 8; i++)
            {
                chArray[8 + i] = hostNameHashCode[i];
            }

            // process part
            for (int i = 0; i < 4; i++)
            {
                chArray[16 + i] = currentProcessId[i];
            }

            // serialnum part
            bytes = BitConverter.GetBytes(NextSerialNumber());
            for (int i = 1; i >= 0; i--)
            {
                chArray[22 - (i * 2)] = ENCODE_TABLE[bytes[i] / 0x10];
                chArray[23 - (i * 2)] = ENCODE_TABLE[bytes[i] % 0x10];
            }

            return new string(chArray);
        }

        public static string LongId()
        {
            var chArray  = new char[32];

            // timestamp part
            byte[] bytes = BitConverter.GetBytes(GetLongTimeStamp());
            for (int i = 5; i >= 0; i--)
            {
                chArray[10 - (i * 2)] = ENCODE_TABLE[bytes[i] / 0x10];
                chArray[11 - (i * 2)] = ENCODE_TABLE[bytes[i] % 0x10];
            }

            // hostname part
            for (int i = 0; i < 8; i++)
            {
                chArray[12 + i] = hostNameHashCode[i];
            }

            // process part
            for (int i = 0; i < 4; i++)
            {
                chArray[20 + i] = currentProcessId[i];
            }

            // serialnum part
            bytes = BitConverter.GetBytes(NextLongSerialNumber());
            for (int i = 3; i >= 0; i--)
            {
                chArray[30 - (i * 2)] = ENCODE_TABLE[bytes[i] / 0x10];
                chArray[31 - (i * 2)] = ENCODE_TABLE[bytes[i] % 0x10];
            }

            return new string(chArray);
        }
    }
}
