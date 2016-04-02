﻿// <copyright file="Compressor.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class wraps several functions for compressing/decompressing data by
    /// using System.IO.Compression.DeflateStream
    /// </summary>
    public static class Compressor
    {
        /// <summary>
        /// Compress an array of bytes
        /// </summary>
        /// <param name="rawData">data to be compressed</param>
        /// <returns>compressed data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage", 
            "CA2202:Do not dispose objects multiple times", 
            Justification = "object will not be disposed multiple times")]
        public static byte[] Compress(byte[] rawData)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // memoryStream will not be disposed twice because leaveOpen is set to true
                // in constructing DeflateStream.
                using (DeflateStream stream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                {
                    stream.Write(rawData, 0, rawData.Length);
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// compress data
        /// </summary>
        /// <param name="buffer">buffer contains raw data</param>
        /// <param name="index">start index of data in buffer</param>
        /// <param name="count">number of bytes of data</param>
        /// <returns>decompressed data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage",
            "CA2202:Do not dispose objects multiple times",
            Justification = "object will not be disposed multiple times")]
        public static byte[] Compress(byte[] buffer, int index, int count)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // memoryStream will not be disposed twice because leaveOpen is set to true
                // in constructing DeflateStream.
                using (DeflateStream stream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                {
                    stream.Write(buffer, index, count);
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress data
        /// </summary>
        /// <param name="compressedData">compressed data generated by calling Compress() function</param>
        /// <returns>decompressed data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage",
            "CA2202:Do not dispose objects multiple times",
            Justification = "object will not be disposed multiple times")]
        public static byte[] Decompress(byte[] compressedData)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // inputStream will not be disposed twice because leaveOpen is set to true
                // in constructing DeflateStream.
                using (MemoryStream inputStream = new MemoryStream(compressedData, false))
                {
                    using (DeflateStream stream = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                    {
                        stream.CopyTo(memoryStream);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress data
        /// </summary>
        /// <param name="buffer">buffer contains data</param>
        /// <param name="index">start index of compressed data in buffer</param>
        /// <param name="count">number of bytes of compressed data</param>
        /// <returns>decompressed data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage",
            "CA2202:Do not dispose objects multiple times",
            Justification = "object will not be disposed multiple times")]
        public static byte[] Decompress(byte[] buffer, int index, int count)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // inputStream will not be disposed twice because leaveOpen is set to true
                // in constructing DeflateStream.
                using (MemoryStream inputStream = new MemoryStream(buffer, index, count))
                {
                    using (DeflateStream stream = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                    {
                        stream.CopyTo(memoryStream);
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}