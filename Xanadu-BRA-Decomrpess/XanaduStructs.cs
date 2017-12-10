using System;
using System.Collections.Generic;
using System.Text;

namespace Xanadu_BRA_Decompress
{
    static class XanaduStructs
    {
        /// <summary>
        /// File header for Tokyo Xanadu .BRA Archive format.
        /// </summary>
        public struct XanaduHeader
        {
            /// <summary>
            /// File type represented as string.
            /// </summary>
            public string fileHeader; // Constant "PDA", null terminated.

            /// <summary>
            /// Presumably compression type. Typically "2"
            /// </summary>
            public UInt32 compressionType;

            /// <summary>
            /// File entries begin at this offset in the file.
            /// </summary>
            public UInt32 fileEntryOffset;

            /// <summary>
            /// The amount of files in the file struct.
            /// </summary>
            public UInt32 fileCount;
        }

        /// <summary>
        /// Entry for each individual file entry in Tokyo Xanadu, entries start after header & compressed data.
        /// </summary>
        public struct XanaduFileEntry
        {
            /// <summary>
            /// The time at which the file was last packed/modified in the archive.
            /// </summary>
            public UInt32 filePackedTime;

            /// <summary>
            /// The purpose is unknown.
            /// </summary>
            public UInt32 unknown;

            /// <summary>
            /// Compressed size of the file within the archive.
            /// </summary>
            public UInt32 compressedSize;

            /// <summary>
            /// The expected size of the file post decompression.
            /// </summary>
            public UInt32 uncompressedSize;

            /// <summary>
            /// Length of the file name, maybe.
            /// </summary>
            public UInt16 fileNameLength;

            /// <summary>
            /// File flags. No special struct for them as they are unknown, I've no interest in finding out either.
            /// </summary>
            public UInt16 fileFlags;

            /// <summary>
            /// Offset of the file's compressed data relative to the start of file.
            /// </summary>
            public UInt32 fileOffset;

            /// <summary>
            /// The name of the compressed file.
            /// </summary>
            public string fileName;
        }

        /// <summary>
        /// Defines the data present at file offset specified in XanaduFileEntry's offset field.
        /// To decompress, just strip the 16 byte header and decompress using Deflate.
        /// </summary>
        public struct XanaduFileData
        {
            /// <summary>
            /// Size of the file after decompression.
            /// </summary>
            public UInt32 uncompressedSize;

            /// <summary>
            /// RAW size of compressed data present in file. 
            /// Basically from Header end to End of File.
            /// </summary>
            public UInt32 compressedSize;

            /// Unknown
            public UInt32 unknown1;
            public UInt32 unknown2;
        }
    }
}
