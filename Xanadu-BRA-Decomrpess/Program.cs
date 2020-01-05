using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Xanadu_BRA_Decompress
{
    /// <summary>
    /// Decompresses .BRA files from Tokyo Xanadu, PC Version, Written 9th of December 2017
    /// </summary>
    class Program
    {
        /// <summary>
        /// Stores the archive in memory as a list of bytes.
        /// </summary>
        static byte[] xanaduArchive;

        /// <summary>
        /// Path to the .BRA Archive
        /// </summary>
        static string filePath;

        /// <summary>
        /// Defines Xanadu's file header.
        /// </summary>
        static XanaduStructs.XanaduHeader fileHeader;

        /// <summary>
        /// Defines a list of files stored within the archive.
        /// </summary>
        static List<XanaduStructs.XanaduFileEntry> fileList;

        /// <summary>
        /// Trims the file extension when writing out to files.
        /// </summary>
        static bool trimExtension;

        /// <summary>
        /// Entry Point!
        /// </summary>
        static void Main(string[] args)
        {
            // Write startup message
            Console.WriteLine("Xanadu .BRA Archive Exporter by Sewer56lol");
            Console.WriteLine("For personal use.");
            Console.WriteLine("Usage: Xanadu_BRA_Decompress <file.bra>");
            Console.WriteLine("Trim file extension (format has badly defined file names): Xanadu_BRA_Decompress <file.bra> -t\n");
            if (args?.Length == 0)
                return;
            
            // Set file path.
            filePath = args[0];

            foreach (string argument in args) { if (argument == "-t") { trimExtension = true; } }

            // Read in the archive file from 1st argument.
            BenchmarkMethod(ReadArchive, "Reading Archive");

            // Parse file Header.
            BenchmarkMethod(ParseHeader, "Parsing Archive");

            // Read File Details
            BenchmarkMethod(PopulateFileEntries, "Parsing File Details");

            // Read File Details
            BenchmarkMethod(WriteFiles, "Writing Files");

            // Hii
            Console.ReadKey();
        }

        ///////////////
        /// Methods ///
        ///////////////

        /// <summary>
        /// Reads the supplied archive file.
        /// </summary>
        private static void ReadArchive() { xanaduArchive = File.ReadAllBytes(filePath); }

        /// <summary>
        /// Parses the header of Tokyo Xanadu's .BRA Archive.
        /// </summary>
        private static void ParseHeader()
        {
            // Allocate Memory
            fileHeader = new XanaduStructs.XanaduHeader();

            // Read Header Contents
            fileHeader.fileHeader = Encoding.ASCII.GetString(xanaduArchive.SubArrayToNullTerminator(0));
            fileHeader.compressionType = BitConverter.ToUInt32(xanaduArchive, 4);
            fileHeader.fileEntryOffset = BitConverter.ToUInt32(xanaduArchive, 8);
            fileHeader.fileCount = BitConverter.ToUInt32(xanaduArchive, 12);
        }

        /// <summary>
        /// Populates the details of each file entry within the archive.
        /// </summary>
        private static void PopulateFileEntries()
        {
            // Allocate Memory
            fileList = new List<XanaduStructs.XanaduFileEntry>((int)fileHeader.fileCount);

            // Create file pointer & set to first entry.
            UInt32 filePointer = fileHeader.fileEntryOffset;

            // Read each file entry
            for (int x = 0; x < fileHeader.fileCount; x++)
            {
                // Generate file entry.
                XanaduStructs.XanaduFileEntry xanaduFileEntry = new XanaduStructs.XanaduFileEntry();

                // Read file entry details & increment pointer.
                xanaduFileEntry.filePackedTime = BitConverter.ToUInt32(xanaduArchive, (int)filePointer);
                filePointer += sizeof(UInt32);

                xanaduFileEntry.unknown = BitConverter.ToUInt32(xanaduArchive, (int)filePointer);
                filePointer += sizeof(UInt32);

                xanaduFileEntry.compressedSize = BitConverter.ToUInt32(xanaduArchive, (int)filePointer);
                filePointer += sizeof(UInt32);

                xanaduFileEntry.uncompressedSize = BitConverter.ToUInt32(xanaduArchive, (int)filePointer);
                filePointer += sizeof(UInt32);

                xanaduFileEntry.fileNameLength = BitConverter.ToUInt16(xanaduArchive, (int)filePointer);
                filePointer += sizeof(UInt16);

                xanaduFileEntry.fileFlags = BitConverter.ToUInt16(xanaduArchive, (int)filePointer);
                filePointer += sizeof(UInt16);

                xanaduFileEntry.fileOffset = BitConverter.ToUInt32(xanaduArchive, (int)filePointer);
                filePointer += sizeof(UInt32);

                xanaduFileEntry.fileName = Encoding.ASCII.GetString(xanaduArchive.SubArray((int)filePointer, (int)xanaduFileEntry.fileNameLength));
                filePointer += (uint)xanaduFileEntry.fileName.Length;

                // Sanitize File name
                xanaduFileEntry.fileName = xanaduFileEntry.fileName.ForceValidFilePath();

                // Trim file extension
                if (trimExtension) { xanaduFileEntry.fileName = xanaduFileEntry.fileName.Substring(0, xanaduFileEntry.fileName.IndexOf(".") + 4); }

                // Add onto list
                fileList.Add(xanaduFileEntry);
            }
        }

        /// <summary>
        /// Writes out the files to disk.
        /// </summary>
        private static void WriteFiles()
        {
            // Get path of folder to save files to & create.
            string folderPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
            Directory.CreateDirectory(folderPath);

            // Doubleline
            Console.WriteLine("\n");

            // Calculate formatting field lengths.
            int nameLength = 0;
            int archiveTimeLength = 0;
            int compressedSizeLength = 0;
            int uncompressedSizeLength = 0;
            for (int x = 0; x < fileList.Count; x++)
            {
                DateTime archiveTime = new System.DateTime(1970, 1, 1).AddSeconds(fileList[x].filePackedTime);
                if (Path.GetFileName(fileList[x].fileName).Length > nameLength) { nameLength = Path.GetFileName(fileList[x].fileName).Length; }
                if (archiveTime.ToString().Length > archiveTimeLength) { archiveTimeLength = archiveTime.ToString().Length; }
                if (Convert.ToString(fileList[x].compressedSize).Length > compressedSizeLength) { compressedSizeLength = Convert.ToString(fileList[x].compressedSize).Length; }
                if (Convert.ToString(fileList[x].uncompressedSize).Length > uncompressedSizeLength) { uncompressedSizeLength = Convert.ToString(fileList[x].uncompressedSize).Length; }
            }

            // For each file get subarray.
            for (int x = 0; x < fileList.Count; x++)
            {
                // Print file details to stdout.
                DateTime archiveTime = new System.DateTime(1970, 1, 1).AddSeconds(fileList[x].filePackedTime);
                string outputString = String.Format("{0, " + nameLength + "} | {1, "+  archiveTimeLength + "} | {2, " +  compressedSizeLength + "} | {3, " +  uncompressedSizeLength + "}", Path.GetFileName(fileList[x].fileName), archiveTime, fileList[x].compressedSize, fileList[x].uncompressedSize);
                Console.WriteLine(outputString);

                // Get path of final file
                string filePath = Path.Combine(folderPath, fileList[x].fileName.Replace("\\", Path.PathSeparator.ToString()));

                // Get subarray which contains the file.
                byte[] fileArray = xanaduArchive.SubArray((int)(fileList[x].fileOffset + 16), (int)(fileList[x].compressedSize - 16));

                // Create Directory
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Decompress using Deflate
                using (var fileStream = File.Create(filePath))
                using (var compressStream = new MemoryStream(fileArray))
                using (var deflateDecompressor = new DeflateStream(compressStream, CompressionMode.Decompress, false))
                {
                    // Check if decompression is necessary by comparing compressed & decompressed size.
                    if (fileList[x].uncompressedSize == fileList[x].compressedSize - 16) // 16 = File Entry Header Length, if sizes are equal, no compression. i.e. uncompressedSize includes header.
                    {
                        // Do not decompress if not compressed.
                        compressStream.CopyTo(fileStream);
                    }
                    else
                    {
                        // Decompress if compressed.
                        deflateDecompressor.CopyTo(fileStream);
                    } 
                }
            }
        }

        ////////////
        /// Misc ///
        ////////////

        /// <summary>
        /// Benchmarks an individual method call.
        /// </summary>
        private static void BenchmarkMethod(Action method, String actionText)
        {
            // Stopwatch to benchmark every action.
            Stopwatch performanceWatch = new Stopwatch();

            // Print out the action
            Console.Write(actionText + " | ");

            // Start the stopwatch.
            performanceWatch.Start();

            // Run the method.
            method();

            // Stop the stopwatch
            performanceWatch.Stop();

            // Print the results.
            Console.WriteLine(performanceWatch.ElapsedMilliseconds + "ms");
        }
    }
}
