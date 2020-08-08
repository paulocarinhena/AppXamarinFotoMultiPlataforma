using System;
using System.Collections.Generic;
using System.Text;

namespace AppXamarinFotoMultiPlataforma.ImagePickers.Android
{
	public interface IMediaService
	{
		void OpenGallery();
		void ClearFileDirectory();
	}
}
