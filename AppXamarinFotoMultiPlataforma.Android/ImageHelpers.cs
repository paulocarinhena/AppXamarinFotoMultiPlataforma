using Android.Graphics;
using AppXamarinFotoMultiPlataforma.ImagePickers.Android;
using System;
using System.IO;
using System.Linq;

namespace AppXamarinFotoMultiPlataforma.Droid
{
    public class ImageHelpers : ICompressImages
    {
        //collectionName é o nome da pasta no diretório de imagens do Android
        public static readonly string collectionName = "TmpPictures";

        public string SaveFile(byte[] imageByte, string fileName)
        {
            var fileDir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), collectionName);
            if (!fileDir.Exists())
            {
                fileDir.Mkdirs();
            }

            var file = new Java.IO.File(fileDir, fileName);
            System.IO.File.WriteAllBytes(file.Path, imageByte);

            return file.Path;
        }

        public string CompressImage(string path, string IDQrCode)
        {
            byte[] imageBytes;

            //Pega bitmap.
            var originalImage = BitmapFactory.DecodeFile(path);

            //Defini os parâmetros imageSize (tamanho imagem) e imageCompression (compressão)
            var imageSize = .86;
            var imageCompression = 67;

            //Redimensiona e compacta como Jpeg.
            var width = (originalImage.Width * imageSize);
            var height = (originalImage.Height * imageSize);
            var scaledImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, true);

            using (MemoryStream ms = new MemoryStream())
            {
                scaledImage.Compress(Bitmap.CompressFormat.Jpeg, imageCompression, ms);
                imageBytes = ms.ToArray();
            }

            originalImage.Recycle();
            originalImage.Dispose();
            string pathSave = IDQrCode + "_" + Guid.NewGuid().ToString();
            GC.Collect();

            return SaveFile(imageBytes, pathSave);
        }
    }
}