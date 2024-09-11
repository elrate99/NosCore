using System.Text.Json;
using System.Text.Json.Serialization;

namespace NosCore.ParserInputGenerator.Downloader
{
    public class ClientManifest
    {
        [JsonPropertyName("entries")]
        public Entry[] Entries { get; set; } = null!;

        [JsonPropertyName("totalSize")]
        public long TotalSize { get; set; }

        [JsonPropertyName("build")]
        public long Build { get; set; }
    }

    public class Entry
    {
        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("sha1")]
        public string? Sha1 { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; } = null!;

        [JsonPropertyName("flags")]
        public long Flags { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("folder")]
        public bool Folder { get; set; }
    }

}
