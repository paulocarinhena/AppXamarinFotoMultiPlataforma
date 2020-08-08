using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppXamarinFotoMultiPlataforma.Service
{
    public interface IMultiMediaPickerService
    {
        event EventHandler<MediaFile> OnMediaPicked;
        event EventHandler<IList<MediaFile>> OnMediaPickedCompleted;
        Task<IList<MediaFile>> PickPhotosAsync();
        Task<IList<MediaFile>> PickVideosAsync();
        void Clean();
    }
}
