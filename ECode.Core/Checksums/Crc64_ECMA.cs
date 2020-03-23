using System;
using System.Collections.Generic;
using System.IO;
using ECode.Utility;

namespace ECode.Checksums
{
    public sealed class Crc64_ECMA : IChecksum
    {
        /// <summary>
        /// The ECMA polynomial, defined in ECMA 182.
        /// </summary>
        const ulong         ECMA    = 0xC96C5795D7870F42;


        static ulong[]      CrcTable;
        static ulong[][]    SlicingTables;


        static Crc64_ECMA()
        {
            MakeCrcTable();
            MakeSlicingTables();
        }


        static void MakeCrcTable()
        {
            if (CrcTable != null)
            { return; }


            CrcTable = new ulong[256];
            for (int i = 0; i < 256; i++)
            {
                var crc = (ulong) i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                    { crc = (crc >> 1) ^ ECMA; }
                    else
                    { crc >>= 1; }
                }

                CrcTable[i] = crc;
            }
        }

        static void MakeSlicingTables()
        {
            if (SlicingTables != null)
            { return; }


            var list = new List<ulong[]>();
            for (int i = 0; i < 8; i++)
            { list.Add(new ulong[256]); }

            SlicingTables = list.ToArray();


            MakeCrcTable();
            SlicingTables[0] = CrcTable;

            for (int i = 0; i < 256; i++)
            {
                var crc = CrcTable[i];
                for (int j = 1; j < 8; j++)
                {
                    crc = CrcTable[crc & 0xff] ^ (crc >> 8);
                    SlicingTables[j][i] = crc;
                }
            }
        }



        private ulong       checksum    = 0;

        public ulong Value
        {
            get { return checksum; }
        }


        public void Reset()
        {
            checksum = 0;
        }


        public void Update(byte b)
        {
            Update(new[] { b });
        }

        public void Update(byte[] bytes)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            Update(bytes, 0, bytes.Length);
        }

        public void Update(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            { throw new ArgumentNullException(nameof(bytes)); }

            if (index < 0)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value must be >= 0."); }

            if (index > bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            if (index + count > bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(index)} + {nameof(count)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }


            checksum = ~checksum;

            if (count >= 64)
            {
                while (count > 8)
                {
                    checksum ^= ((ulong)bytes[index]) | ((ulong)bytes[index + 1]) << 8 | ((ulong)bytes[index + 2]) << 16 | ((ulong)bytes[index + 3]) << 24
                        | ((ulong)bytes[index + 4]) << 32 | ((ulong)bytes[index + 5]) << 40 | ((ulong)bytes[index + 6]) << 48 | ((ulong)bytes[index + 7]) << 56;

                    checksum = SlicingTables[7][checksum & 0xff]
                        ^ SlicingTables[6][(checksum >> 8) & 0xff]
                        ^ SlicingTables[5][(checksum >> 16) & 0xff]
                        ^ SlicingTables[4][(checksum >> 24) & 0xff]
                        ^ SlicingTables[3][(checksum >> 32) & 0xff]
                        ^ SlicingTables[2][(checksum >> 40) & 0xff]
                        ^ SlicingTables[1][(checksum >> 48) & 0xff]
                        ^ SlicingTables[0][checksum >> 56];

                    index += 8;
                    count -= 8;
                }
            }

            while (--count >= 0)
            {
                checksum = CrcTable[((byte)checksum) ^ bytes[index++]] ^ (checksum >> 8);
            }

            checksum = ~checksum;
        }

        public void Update(Stream stream)
        {
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read."); }


            var count = 0;
            var buffer = new byte[1024];
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                Update(buffer, 0, count);
            }
        }
    }
}
