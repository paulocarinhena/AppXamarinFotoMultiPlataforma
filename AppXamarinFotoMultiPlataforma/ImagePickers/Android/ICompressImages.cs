using System;
using System.Collections.Generic;
using System.Text;

namespace AppXamarinFotoMultiPlataforma.ImagePickers.Android
{
    public interface ICompressImages
    {
        string CompressImage(string path, string IDQrCode);
    }
}
