using System;

namespace Xanadu_BRA_Decompress;

internal static class XanaduStructs
{
    /// <summary>
    /// File header for Tokyo Xanadu .BRA Archive format.
    /// </summary>
    public struct XanaduHeader
    {
        /// <summary>
        /// File type represented as string.
        /// </summary>
        public string FileHeader; // Constant "PDA", null terminated.

        /// <summary>
        /// Presumably compression type. Typically "2"
        /// </summary>
        public UInt32 CompressionType;

        /// <summary>
        /// File entries begin at this offset in the file.
        /// </summary>
        public UInt32 FileEntryOffset;

        /// <summary>
        /// The amount of files in the file struct.
        /// </summary>
        public UInt32 FileCount;
    }

    /// <summary>
    /// Entry for each individual file entry in Tokyo Xanadu, entries start after header & compressed data.
    /// </summary>
    public struct XanaduFileEntry
    {
        /// <summary>
        /// The time at which the file was last packed/modified in the archive.
        /// </summary>
        public UInt32 FilePackedTime;

        /// <summary>
        /// The purpose is unknown.
        /// </summary>
        public UInt32 Unknown;

        /// <summary>
        /// Compressed size of the file within the archive.
        /// </summary>
        public UInt32 CompressedSize;

        /// <summary>
        /// The expected size of the file post decompression.
        /// </summary>
        public UInt32 UncompressedSize;

        /// <summary>
        /// Length of the file name, maybe.
        /// </summary>
        public UInt16 FileNameLength;

        /// <summary>
        /// File flags. No special struct for them as they are unknown, I've no interest in finding out either.
        /// </summary>
        public UInt16 FileFlags;

        /// <summary>
        /// Offset of the file's compressed data relative to the start of file.
        /// </summary>
        public UInt32 FileOffset;

        /// <summary>
        /// The name of the compressed file.
        /// </summary>
        public string FileName;
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
        public UInt32 UncompressedSize;

        /// <summary>
        /// RAW size of compressed data present in file. 
        /// Basically from Header end to End of File.
        /// </summary>
        public UInt32 CompressedSize;

        /// Unknown
        public UInt32 Unknown1;
        public UInt32 Unknown2;
    }
}