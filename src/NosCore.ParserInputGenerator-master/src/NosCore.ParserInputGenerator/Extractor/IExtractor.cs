//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.IO;
using System.Threading.Tasks;

namespace NosCore.ParserInputGenerator.Extractor
{
    public interface IExtractor
    {
        Task ExtractAsync(FileInfo file);
        Task ExtractAsync(FileInfo file, string directory);
        Task ExtractAsync(FileInfo file, string directory, bool rename);
    }
}