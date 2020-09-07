using Offer.Domain.AggregatesModel.ContentModel;
using System.Threading.Tasks;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public interface IContentServiceOld
    {
        Task<Metadata> CreateFolder(string name, string path, string kind, string purpose);
        Task<bool> DeleteFolderByPath(string folderPath, bool deleteRecursive = true);
        Task<Metadata> GetFolderMetadata(string folderPath);
    }
}
