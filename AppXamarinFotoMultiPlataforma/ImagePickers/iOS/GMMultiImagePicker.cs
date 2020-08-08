using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace AppXamarinFotoMultiPlataforma.ImagePickers.iOS
{
    public class GMMultiImagePicker
    {
        private static readonly Lazy<IGMMultiImagePicker> Implementation = new Lazy<IGMMultiImagePicker>(CreateModalView,
           System.Threading.LazyThreadSafetyMode.PublicationOnly);

        public static IGMMultiImagePicker Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }
        private static IGMMultiImagePicker CreateModalView()
        {
#if PORTABLE
            return null;
#else
            return DependencyService.Get<IGMMultiImagePicker>();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return
                new NotImplementedException(
                    "Essa funcionalidade não é implementada na versão portátil deste assembly. Você deve fazer referência ao pacote NuGet de seu projeto de aplicativo principal para fazer referência à implementação específica da plataforma.");
        }
    }
}
