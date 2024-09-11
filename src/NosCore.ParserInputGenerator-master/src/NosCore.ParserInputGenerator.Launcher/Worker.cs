using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NosCore.ParserInputGenerator.Downloader;
using NosCore.ParserInputGenerator.Extractor;
using NosCore.ParserInputGenerator.I18N;
using NosCore.Shared.I18N;

namespace NosCore.ParserInputGenerator.Launcher
{
    public class Worker : BackgroundService
    {
        private const string ConsoleText = "PARSER INPUT GENERATOR - NosCoreIO";

        private readonly ILogger<Worker> _logger;
        private readonly IClientDownloader _client;
        private readonly IExtractor _extractor;

        private readonly string[] _parserInputFiles = {
            "NScliData_CZ.NOS",
            "NScliData_DE.NOS",
            "NScliData_ES.NOS",
            "NScliData_FR.NOS",
            "NScliData_IT.NOS",
            "NScliData_PL.NOS",
            "NScliData_RU.NOS",
            "NScliData_TR.NOS",
            "NScliData_UK.NOS",
            "NSlangData_CZ.NOS",
            "NSlangData_DE.NOS",
            "NSlangData_ES.NOS",
            "NSlangData_FR.NOS",
            "NSlangData_IT.NOS",
            "NSlangData_PL.NOS",
            "NSlangData_RU.NOS",
            "NSlangData_TR.NOS",
            "NSlangData_UK.NOS",
            "NStcData.NOS",
            "NSgtdData.NOS"
        };

        public Worker(ILogger<Worker> logger, IClientDownloader client, IExtractor extractor)
        {
            _logger = logger;
            _client = client;
            _extractor = extractor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.PrintHeader(ConsoleText);
            }
            catch
            {
                // ignored as header is not important
            }
            var manifest = await _client.DownloadManifest();
            var fileslist = _parserInputFiles.Select(o => $"NostaleData{Path.DirectorySeparatorChar}{o}").ToList();
            manifest.Entries = manifest.Entries.Where(s => fileslist.Contains(s.File)).ToArray();
            await _client.DownloadClientAsync(manifest);
            foreach (var file in fileslist)
            {
                var rename = file.Contains("NScliData");
                var dest = file.Contains("NStcData") ? $".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}parser{Path.DirectorySeparatorChar}maps{Path.DirectorySeparatorChar}" : $".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}parser{Path.DirectorySeparatorChar}";
                var fileInfo = new FileInfo($".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}{file}");
                await _extractor.ExtractAsync(fileInfo, dest, rename);
            }
            var directoryOfFilesToBeTarred = new DirectoryInfo($".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}parser");
            var filesInDirectory = directoryOfFilesToBeTarred.GetFiles("*.*", SearchOption.AllDirectories);
            var tarArchiveName = $".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}parser-input-files.tar.bz2";
            if (File.Exists(tarArchiveName))
            {
                File.Delete(tarArchiveName);
            }
            await using Stream targetStream = new BZip2OutputStream(File.Create(tarArchiveName));
            using var tarArchive = TarArchive.CreateOutputTarArchive(targetStream, TarBuffer.DefaultBlockFactor);
            foreach (var fileToBeTarred in filesInDirectory)
            {
                var entry = TarEntry.CreateEntryFromFile(fileToBeTarred.FullName);
                tarArchive.WriteEntry(entry, true);
            }
            tarArchive.Dispose();
            _logger.LogInformation(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.PARSER_INPUT_GENERATED));
        }
    }
}