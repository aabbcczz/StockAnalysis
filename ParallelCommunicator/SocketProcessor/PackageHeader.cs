namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using FastRank;

    internal struct PackageHeader
    {
        // package header related fields and functions
        public const uint StandardMagicNumber = 0x4D474B50; // "PKGM"
        public static int SizeOfHeader = 5 * sizeof(uint);

        public uint MagicNumber;
        public int SerialNumber;
        public PackageFlags Flags;
        public int RawDataLength; // the length of package data which does not include the size of package header
        public int CompressedDataLength; // the length of package data if Flags shows that the package is compressed

        private static int globalSerialNumber = 0;

        [Flags]
        internal enum PackageFlags : uint
        {
            None = 0,
            Compressed = 1
        }

        public static int GetSerialNumber()
        {
            return Interlocked.Increment(ref PackageHeader.globalSerialNumber);
        }

        public void ToByteArray(byte[] buffer, ref int offset)
        {
            MagicNumber.ToByteArray(buffer, ref offset);
            SerialNumber.ToByteArray(buffer, ref offset);
            ((uint)Flags).ToByteArray(buffer, ref offset);
            RawDataLength.ToByteArray(buffer, ref offset);
            CompressedDataLength.ToByteArray(buffer, ref offset);
        }

        public void FromByteArray(byte[] buffer, ref int offset)
        {
            MagicNumber = buffer.ToUInt(ref offset);
            SerialNumber = buffer.ToInt(ref offset);
            Flags = (PackageFlags)buffer.ToUInt(ref offset);
            RawDataLength = buffer.ToInt(ref offset);
            CompressedDataLength = buffer.ToInt(ref offset);
        }
    }
}
