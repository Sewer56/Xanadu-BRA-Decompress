# Xanadu BRA Decompress

A tool to extract files from Tokyo Xanadu eX+ (PC) `.BRA/.PDA` archive files.

## Overview

This is a decompressor for Tokyo Xanadu eX+ PC Version's archive format, originally written on December 9th, 2017 - I got the game, right after release.

Originally released on the defunct [heroesoflegend.org forum](https://web.archive.org/web/20230401171414/https://heroesoflegend.org/forums/viewtopic.php?f=38&p=3416&sid=972b2173e333a0310f663639435c86ba).

**Important Caveats:**
- This code is ancient (written in 2017)
- This code was written on the day after game release
- This code does not follow best coding practices (it was from my early days of coding)

## Download & Usage

Pre-compiled binaries are available:

- **Windows**: [Xanadu-BRA-Decompress-win-x64.exe](https://github.com/Sewer56/Xanadu-BRA-Decompress/releases/latest/download/Xanadu-BRA-Decompress-win-x64.exe)
- **Linux**: [Xanadu-BRA-Decompress-linux-x64](https://github.com/Sewer56/Xanadu-BRA-Decompress/releases/latest/download/Xanadu-BRA-Decompress-linux-x64)
- **macOS (Intel)**: [Xanadu-BRA-Decompress-osx-x64](https://github.com/Sewer56/Xanadu-BRA-Decompress/releases/latest/download/Xanadu-BRA-Decompress-osx-x64)
- **macOS (Apple Silicon)**: [Xanadu-BRA-Decompress-osx-arm64](https://github.com/Sewer56/Xanadu-BRA-Decompress/releases/latest/download/Xanadu-BRA-Decompress-osx-arm64)

(These should not require any additional dependencies on a typical system.)  
You can then use from command prompt or terminal.

```bash
./Xanadu-BRA-Decompress-linux-x64 <BRA Archive> -t
# Or on Windows:
# Xanadu-BRA-Decompress-win-x64.exe <BRA Archive> -t
```

e.g. `Xanadu-BRA-Decompress-win-x64.exe System.bra -t`

### Options

- `-t` : Trim file extensions
  - Without `-t`: May result in unexpected extensions due to the format's padding issues
  - With `-t`: Trims extension length for cleaner output

## Archive Format Details

```c
// Archive Header (0x10 bytes)
struct XanaduHeader {
    char headerTag[4];          // "PDA\0" - Constant header tag
    uint32 compressionType;     // Constant value of 2
    uint32 fileEntryOffset;     // Offset to end of compressed file data
    uint32 fileCount;           // Number of files in archive
};

// Raw File Data Section (Variable Length)
// Each file consists of a 0x10 header followed by compressed data
struct FileData {
    uint32 uncompressedSize;    // Expected size after decompression
    uint32 compressedSize;      // Size of compressed data after this header
    uchar unknown[8];           // Unknown purpose, sometimes 0
    uchar compressedData[compressedSize - 16];  // Raw deflate compressed data
};

// File Entry Section (Variable Length)  
// Located at header.fileEntryOffset, repeated fileCount times
struct FileEntry {
    time_t archiveTime;         // C time format - when file was packed/modified
    uint32 unknown;             // Unknown purpose
    uint32 compressedSize;      // Size including 0x10 header
    uint32 uncompressedSize;    // Size after decompression
    uint16 fileNameLength;      // Length including junk padding
    uint16 fileFlags;           // Unknown flags
    uint32 fileOffset;          // Offset to raw data section
    char fileName[fileNameLength];  // Filename + 1-3 bytes random padding
                                    // Padded to make total length divisible by 4
};

XanaduHeader header;
FileData fileData[header.fileCount];
FileEntry fileEntry[header.fileCount];
```

## Building

```bash
dotnet build
```

## License

See LICENSE.md for details.
