using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NosCore.Shared.Enumerations;

namespace NosCore.ParserInputGenerator.Downloader
{
    public interface IClientDownloader
    {
        Task<ClientManifest> DownloadManifest();
        Task<ClientManifest> DownloadManifestAsync(RegionType region);

        Task DownloadClientAsync();
        Task DownloadClientAsync(ClientManifest manifest);
    }
}
