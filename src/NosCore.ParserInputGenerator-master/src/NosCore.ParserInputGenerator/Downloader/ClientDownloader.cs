//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NosCore.ParserInputGenerator.I18N;
using NosCore.Shared.Enumerations;

namespace NosCore.ParserInputGenerator.Downloader
{
    public class ClientDownloader : IClientDownloader
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ClientDownloader> _logger;

        public ClientDownloader(IHttpClientFactory clientFactory, ILogger<ClientDownloader> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public Task<ClientManifest> DownloadManifest() => DownloadManifestAsync(RegionType.EN);

        public async Task<ClientManifest> DownloadManifestAsync(RegionType region)
        {
            var client = _clientFactory.CreateClient();
            using var result = await client
                .GetAsync($"https://spark.gameforge.com/api/v1/patching/download/latest/nostale/default?locale=${region}&architecture=x64&branchToken")
                .ConfigureAwait(false);
            return JsonSerializer.Deserialize<ClientManifest>(await result.Content.ReadAsStringAsync().ConfigureAwait(false), new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            }) ?? throw new InvalidOperationException();
        }

        public async Task DownloadClientAsync() => await DownloadClientAsync(await DownloadManifest());

        public async Task DownloadClientAsync(ClientManifest manifest)
        {
            async Task Download(Entry entry)
            {
                var client = _clientFactory.CreateClient();

                if (entry.Folder)
                {
                    Directory.CreateDirectory($".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}{entry.File}");
                    return;
                }
                var file = $".{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}{entry.File}";
                if (File.Exists(file))
                {
                    await using var fop = File.OpenRead(file);
                    var chksum = BitConverter.ToString(System.Security.Cryptography.SHA1.Create().ComputeHash(fop)).Replace("-", "").ToLowerInvariant();
                    if (chksum == entry.Sha1)
                    {
                        return;
                    }
                }

                _logger.LogInformation(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.DOWNLOADING),
                    entry.Path);
                using var response = await client.GetAsync($"http://patches.gameforge.com/" + entry.Path)
                    .ConfigureAwait(false);

                var fileInfo = new FileInfo(file);

                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                }
                await using var fileStream = File.Create(file);
                await using var stream = await response.Content.ReadAsStreamAsync();
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream);
                _logger.LogInformation(LogLanguage.Instance.GetMessageFromKey(LogLanguageKey.DOWNLOAD_SUCCESSFULL), file);
            }

            await Task.WhenAll(manifest.Entries.Select(Download));
        }

    }
}