using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Xanadu_BRA_Decompress;

/// <summary>
/// Decompresses .BRA files from Tokyo Xanadu, PC Version, Written 9th of December 2017
/// </summary>
internal class Program
{
    /// <summary>
    /// Stores the archive in memory as a list of bytes.
    /// </summary>
    private static byte[] _xanaduArchive;

    /// <summary>
    /// Path to the .BRA Archive
    /// </summary>
    private static string _filePath;

    /// <summary>
    /// Defines Xanadu's file header.
    /// </summary>
    private static XanaduStructs.XanaduHeader _fileHeader;

    /// <summary>
    /// Defines a list of files stored within the archive.
    /// </summary>
    private static List<XanaduStructs.XanaduFileEntry> _fileList;

    /// <summary>
    /// Trims the file extension when writing out to files.
    /// </summary>
    private static bool _trimExtension;

    /// <summary>
    /// Entry Point!
    /// </summary>
    private static void Main(string[] args)
    {
        // Write startup message
        Console.WriteLine("Xanadu .BRA Archive Exporter by Sewer56lol");
        Console.WriteLine("For personal use.");
        Console.WriteLine("Usage: Xanadu_BRA_Decompress <file.bra>");
        Console.WriteLine("Trim file extension (format has badly defined file names): Xanadu_BRA_Decompress <file.bra> -t\n");
        if (args?.Length == 0)
            return;
            
        // Set file path.
        _filePath = args[0];

        foreach (var argument in args) { if (argument == "-t") { _trimExtension = true; } }

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

    /// <summary>
    /// Reads the supplied archive file.
    /// </summary>
    private static void ReadArchive() { _xanaduArchive = File.ReadAllBytes(_filePath); }

    /// <summary>
    /// Parses the header of Tokyo Xanadu's .BRA Archive.
    /// </summary>
    private static void ParseHeader()
    {
        // Allocate Memory
        _fileHeader = new XanaduStructs.XanaduHeader();

        // Read Header Contents
        _fileHeader.FileHeader = Encoding.ASCII.GetString(_xanaduArchive.SubArrayToNullTerminator(0));
        _fileHeader.CompressionType = BitConverter.ToUInt32(_xanaduArchive, 4);
        _fileHeader.FileEntryOffset = BitConverter.ToUInt32(_xanaduArchive, 8);
        _fileHeader.FileCount = BitConverter.ToUInt32(_xanaduArchive, 12);
    }

    /// <summary>
    /// Populates the details of each file entry within the archive.
    /// </summary>
    private static void PopulateFileEntries()
    {
        // Allocate Memory
        _fileList = new List<XanaduStructs.XanaduFileEntry>((int)_fileHeader.FileCount);

        // Create file pointer & set to first entry.
        var filePointer = _fileHeader.FileEntryOffset;

        // Read each file entry
        for (var x = 0; x < _fileHeader.FileCount; x++)
        {
            // Generate file entry.
            var xanaduFileEntry = new XanaduStructs.XanaduFileEntry();

            // Read file entry details & increment pointer.
            xanaduFileEntry.FilePackedTime = BitConverter.ToUInt32(_xanaduArchive, (int)filePointer);
            filePointer += sizeof(uint);

            xanaduFileEntry.Unknown = BitConverter.ToUInt32(_xanaduArchive, (int)filePointer);
            filePointer += sizeof(uint);

            xanaduFileEntry.CompressedSize = BitConverter.ToUInt32(_xanaduArchive, (int)filePointer);
            filePointer += sizeof(uint);

            xanaduFileEntry.UncompressedSize = BitConverter.ToUInt32(_xanaduArchive, (int)filePointer);
            filePointer += sizeof(uint);

            xanaduFileEntry.FileNameLength = BitConverter.ToUInt16(_xanaduArchive, (int)filePointer);
            filePointer += sizeof(ushort);

            xanaduFileEntry.FileFlags = BitConverter.ToUInt16(_xanaduArchive, (int)filePointer);
            filePointer += sizeof(ushort);

            xanaduFileEntry.FileOffset = BitConverter.ToUInt32(_xanaduArchive, (int)filePointer);
            filePointer += sizeof(uint);

            xanaduFileEntry.FileName = Encoding.ASCII.GetString(_xanaduArchive.SubArray((int)filePointer, xanaduFileEntry.FileNameLength));
            filePointer += (uint)xanaduFileEntry.FileName.Length;

            // Sanitize File name
            xanaduFileEntry.FileName = xanaduFileEntry.FileName.ForceValidFilePath();

            // Trim file extension
            if (_trimExtension) { xanaduFileEntry.FileName = xanaduFileEntry.FileName.Substring(0, xanaduFileEntry.FileName.IndexOf(".", StringComparison.Ordinal) + 4); }

            // Add onto list
            _fileList.Add(xanaduFileEntry);
        }
    }

    /// <summary>
    /// Writes out the files to disk.
    /// </summary>
    private static void WriteFiles()
    {
        // Get path of folder to save files to & create.
        var folderPath = Path.Combine(Path.GetDirectoryName(_filePath)!, Path.GetFileNameWithoutExtension(_filePath)!);
        Directory.CreateDirectory(folderPath);

        // Doubleline
        Console.WriteLine("\n");

        // Calculate formatting field lengths.
        var nameLength = 0;
        var archiveTimeLength = 0;
        var compressedSizeLength = 0;
        var uncompressedSizeLength = 0;
        for (var x = 0; x < _fileList.Count; x++)
        {
            var fileNameLength = Path.GetFileName(_fileList[x].FileName)!.Length; 
            var archiveTime = new DateTime(1970, 1, 1).AddSeconds(_fileList[x].FilePackedTime).ToString(CultureInfo.InvariantCulture);
            if (fileNameLength > nameLength) { nameLength = fileNameLength; }
            if (archiveTime.Length > archiveTimeLength) { archiveTimeLength = archiveTime.ToString().Length; }
            if (Convert.ToString(_fileList[x].CompressedSize).Length > compressedSizeLength) { compressedSizeLength = Convert.ToString(_fileList[x].CompressedSize).Length; }
            if (Convert.ToString(_fileList[x].UncompressedSize).Length > uncompressedSizeLength) { uncompressedSizeLength = Convert.ToString(_fileList[x].UncompressedSize).Length; }
        }

        // For each file get subarray.
        for (var x = 0; x < _fileList.Count; x++)
        {
            // Print file details to stdout.
            var archiveTime = new System.DateTime(1970, 1, 1).AddSeconds(_fileList[x].FilePackedTime);
            var outputString = String.Format("{0, " + nameLength + "} | {1, "+  archiveTimeLength + "} | {2, " +  compressedSizeLength + "} | {3, " +  uncompressedSizeLength + "}", Path.GetFileName(_fileList[x].FileName), archiveTime, _fileList[x].CompressedSize, _fileList[x].UncompressedSize);
            Console.WriteLine(outputString);

            // Get path of final file
            var filePath = Path.Combine(folderPath, _fileList[x].FileName!.Replace("\\", Path.DirectorySeparatorChar.ToString()));

            // Get subarray which contains the file.
            var fileArray = _xanaduArchive.SubArray((int)(_fileList[x].FileOffset + 16), (int)(_fileList[x].CompressedSize - 16));

            // Create Directory
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // Decompress using Deflate
            using var fileStream = File.Create(filePath);
            using var compressStream = new MemoryStream(fileArray);
            using var deflateDecompressor = new DeflateStream(compressStream, CompressionMode.Decompress, false);

            // Check if decompression is necessary by comparing compressed & decompressed size.
            if (_fileList[x].UncompressedSize == _fileList[x].CompressedSize - 16) // 16 = File Entry Header Length, if sizes are equal, no compression. i.e. uncompressedSize includes header.
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

    /// <summary>
    /// Benchmarks an individual method call.
    /// </summary>
    private static void BenchmarkMethod(Action method, string actionText)
    {
        // Stopwatch to benchmark every action.
        var performanceWatch = new Stopwatch();

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