using System.IO;
using System.Threading.Tasks;

namespace AppXamarinFotoMultiPlataforma.ImagePickers.UWP
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}
