using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace LivePhotoFrame.UWP.Models
{
    interface IPhotoProvider
    {
        int Count { get; }

        Task Init();
        Task<IRandomAccessStream> NextStream();

        void Done();
    }
}