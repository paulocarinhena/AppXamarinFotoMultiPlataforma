using Android.App;
using Android.Content;
using Android.Widget;
using AppXamarinFotoMultiPlataforma.Droid;
using AppXamarinFotoMultiPlataforma.ImagePickers.Android;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MediaService))]
namespace AppXamarinFotoMultiPlataforma.Droid
{
    public class MediaService : Java.Lang.Object, IMediaService
    {
        public static int OPENGALLERYCODE = 100;
        public void OpenGallery()
        {
            try
            {
                var imageIntent = new Intent(Intent.ActionPick);
                imageIntent.SetType("image/*");
                imageIntent.PutExtra(Intent.ExtraAllowMultiple, true);
                imageIntent.SetAction(Intent.ActionGetContent);
                ((Activity)Forms.Context).StartActivityForResult(Intent.CreateChooser(imageIntent, "Selecione as Fotos"), OPENGALLERYCODE);
                Toast.MakeText(Xamarin.Forms.Forms.Context, "Toque e segure para selecionar várias fotos", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Toast.MakeText(Forms.Context, "Erro. Não é possível continuar, tente novamente.", ToastLength.Long).Show();
            }
        }

        /// <summary>
        ///     Deleta os arquivos temporários.
        /// </summary>
		void IMediaService.ClearFileDirectory()
        {
            var directory = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), ImageHelpers.collectionName).Path.ToString();

            if (Directory.Exists(directory))
            {
                var list = Directory.GetFiles(directory, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        File.Delete(list[i]);
                    }
                }
            }
        }
    }
}