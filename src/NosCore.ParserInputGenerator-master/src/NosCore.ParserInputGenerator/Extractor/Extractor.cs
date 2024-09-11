using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Microsoft.Extensions.Logging;
using NosCore.ParserInputGenerator.I18N;

namespace NosCore.ParserInputGenerator.Extractor
{
    public class Extractor : IExtractor
    {
        private readonly ILogger<Extractor> _logger;

        public Extractor(ILogger<Extractor> logger)
        {
            _logger = logger;
        }

        public Task ExtractAsync(FileInfo file, string dest) => ExtractAsync(file, dest, false);

        public Task ExtractAsync(FileInfo file) => ExtractAsync(file, $".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}");

        public async Task ExtractAsync(FileInfo nosFile, string directory, bool rename)
        {
            async Task WriteFile(string fileName, MemoryStream decryptedContent)
            {
                if (rename && fileName.Contains("."))
                {
                    var name = fileName.Substring(0, fileName.IndexOf('.'));
                    var ext = fileName[fileName.IndexOf('.')..];
                    await using var fileStream =
                        File.Create(
                            $"{directory}{name}{nosFile.Name.Substring(nosFile.Name.LastIndexOf('_'), 3)}{ext}");
                    decryptedContent.Seek(0, SeekOrigin.Begin);
                    await decryptedContent.CopyToAsync(fileStream);
                }
                else
                {
                    await using var fileStream = File.Create($"{directory}{fileName}");
                    decryptedContent.Seek(0, SeekOrigin.Begin);
                    await decryptedContent.CopyToAsync(fileStream);
                }
      
            }

            try
            {
                var fileInfo = new FileInfo(directory);
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                }
                var fsSource = await File.ReadAllBytesAsync(nosFile.FullName);
                if (Encoding.UTF8.GetString(fsSource.Take(7).ToArray()) == "NT Data")
                {
                    var currentIndex = 16;
                    var fileAmount = BitConverter.ToInt32(fsSource.Skip(currentIndex).Take(4).ToArray());
                    currentIndex += 5;
                    for (var i = 0; i < fileAmount; ++i)
                    {
                        var id = BitConverter.ToInt32(fsSource.Skip(currentIndex).Take(4).ToArray());
                        currentIndex += 4;
                        var offset = BitConverter.ToInt32(fsSource.Skip(currentIndex).Take(4).ToArray());
                        var previousIndex = currentIndex + 4;
                        currentIndex = offset + 4;
                        var dataSize = BitConverter.ToInt32(fsSource.Skip(currentIndex).Take(4).ToArray());
                        currentIndex += 4;
                        var compressedDataSize = BitConverter.ToInt32(fsSource.Skip(currentIndex).Take(4).ToArray());
                        currentIndex += 5;
                        await using var outputStream = new MemoryStream();
                        var bigEndian = fsSource.Skip(currentIndex).Take(dataSize + compressedDataSize).ToArray();
                        await using var compressedStream = new MemoryStream(bigEndian);
                        await using var inputStream = new InflaterInputStream(compressedStream);
                        await inputStream.CopyToAsync(outputStream);
                        currentIndex = previousIndex;
                        await WriteFile(id.ToString(), outputStream);
                    }
                }
                else
                {
                    var fileAmount = BitConverter.ToInt32(fsSource.Take(4).ToArray());
                    var currentIndex = 4;
                    for (var i = 0; i < fileAmount; i++)
                    {
                        currentIndex += 4;
                        var fileNameSize = BitConverter.ToInt32(fsSource.Skip(currentIndex).Take(4).ToArray());
                        currentIndex += 4;
                        var fileName = new string(fsSource.Skip(currentIndex).Take(fileNameSize).Select(Convert.ToChar)
                            .ToArray());
                        currentIndex += fileNameSize + 4;
                        var fileSize = BitConverter.ToInt32(fsSource.Skip(currentIndex).Take(4).ToArray());
                        currentIndex += 4;
                        var fileContent = fsSource.Skip(currentIndex).Take(fileSize).ToArray();
                        currentIndex += fileSize;

                        await using var decryptedContent = new MemoryStream(DecryptDat(fileContent));
                        await WriteFile(fileName, decryptedContent);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.ERROR), ex);
            }
        }

        private static byte[] DecryptDat(byte[] array)
        {
            var cryptoarray = new[]
                {0x00, 0x20, 0x2D, 0x2E, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x0A, 0x00};
            var decryptedFile = new List<byte>();
            var currIndex = 0;
            while (currIndex < array.Length)
            {
                var currentByte = array[currIndex];
                currIndex++;
                if (currentByte == 0xFF)
                {
                    decryptedFile.Add(0xD);
                    continue;
                }

                var validate = currentByte & 0x7F;

                if (Convert.ToBoolean(currentByte & 0x80))
                {
                    for (; validate > 0; validate -= 2)
                    {
                        if (currIndex >= array.Length)
                            break;

                        currentByte = array[currIndex];
                        currIndex++;

                        var firstByte = cryptoarray[(currentByte & 0xF0) >> 4];
                        decryptedFile.Add((byte)firstByte);

                        if (validate <= 1)
                            break;
                        var secondByte = cryptoarray[currentByte & 0xF];

                        if (!(Convert.ToBoolean(secondByte)))
                        {
                            break;
                        }

                        decryptedFile.Add((byte)secondByte);
                    }
                }
                else
                {
                    for (; validate > 0; --validate)
                    {
                        if (currIndex >= array.Length)
                            break;

                        currentByte = array[currIndex];
                        currIndex++;

                        decryptedFile.Add((byte)(currentByte ^ 0x33));
                    }
                }
            }

            return decryptedFile.ToArray();
        }
    }
}
