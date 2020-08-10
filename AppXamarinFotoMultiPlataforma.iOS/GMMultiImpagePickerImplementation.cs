using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppXamarinFotoMultiPlataforma.ImagePickers.iOS;
using AppXamarinFotoMultiPlataforma.iOS;
using CoreFoundation;
using Foundation;
using GMImagePicker;
using Photos;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(GMMultiImpagePickerImplementation))]
namespace AppXamarinFotoMultiPlataforma.iOS
{
    public class GMMultiImpagePickerImplementation : IGMMultiImagePicker
    {
        private int GetRequestId()
        {
            var id = _requestId;
            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;
            return id;
        }

        private int _requestId;
        private TaskCompletionSource<List<string>> _completionSource;

        /// <summary>
        ///     Imagens NÃO modificadas.
        /// </summary>
        /// <returns></returns>
        public Task<List<string>> PickMultiImage()
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<List<string>>(id);
            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
            {
#if DEBUG
                throw new InvalidOperationException("Apenas uma operação pode estar ativa por vez");
#else
                return null;
#endif
            }

            //init media picker without high quality format.
            ShowImagePicker(false);
            return _completionSource.Task;
        }

        /// <summary>
        ///     Retorna Imagens modificadas
        /// </summary>
        /// <returns></returns>
        public Task<List<string>> PickMultiImage(bool needsHighQuality)
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<List<string>>(id);
            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
            {
#if DEBUG
                throw new InvalidOperationException("Apenas uma operação pode estar ativa por vez");
#else
                return null;
#endif
            }

            //inicia a midia com formato de alta qualidade.
            ShowImagePicker(needsHighQuality);
            return _completionSource.Task;
        }

        private PHAsset[] _preselectedAssets;

        public void ShowImagePicker(bool needsHighQuality)
        {
            //Cria Galeria como ImagePicker.
            var picker = new GMImagePickerController
            {
                Title = "Selecione Foto",
                CustomDoneButtonTitle = "Terminado",
                CustomCancelButtonTitle = "Cancelar",
                ColsInPortrait = 4,
                ColsInLandscape = 7,
                MinimumInteritemSpacing = 2.0f,
                DisplaySelectionInfoToolbar = true,
                AllowsMultipleSelection = true,
                ShowCameraButton = true,
                AutoSelectCameraImages = true,
                ModalPresentationStyle = UIModalPresentationStyle.Popover,
                MediaTypes = new[] { PHAssetMediaType.Image },
                CustomSmartCollections = new[]
                {
                    PHAssetCollectionSubtype.SmartAlbumUserLibrary,
                    PHAssetCollectionSubtype.AlbumRegular
                },
                NavigationBarTextColor = Color.White.ToUIColor(),
                NavigationBarBarTintColor = Color.FromHex("#c5dd36").ToUIColor(),
                PickerTextColor = Color.Black.ToUIColor(),
                ToolbarTextColor = Color.FromHex("#c5dd36").ToUIColor(),
                NavigationBarTintColor = Color.White.ToUIColor()
            };

            //Define um limite para o número de fotos que o usuário pode selecionar (12 devido às limitações de memória no iOS)..
            picker.ShouldSelectAsset += (sender, args) => args.Cancel = picker.SelectedAssets.Count > 11;

            //selecionar manipulador de imagens
            GMImagePickerController.MultiAssetEventHandler[] handler = { null };

            //Manipulador de cancelamento
            EventHandler[] cancelHandler = { null };

            //define o manipulador
            handler[0] = (sender, args) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                picker.FinishedPickingAssets -= handler[0];
                picker.Canceled -= cancelHandler[0];
                System.Diagnostics.Debug.WriteLine("Usuário terminou de selecionad as fotos. {0} items seleccionados.", args.Assets.Length);
                var imageManager = new PHImageManager();
                var RequestImageOption = new PHImageRequestOptions();

                if (needsHighQuality)
                {
                    RequestImageOption.DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat;
                }
                else
                {
                    RequestImageOption.DeliveryMode = PHImageRequestOptionsDeliveryMode.FastFormat;
                }

                RequestImageOption.ResizeMode = PHImageRequestOptionsResizeMode.Fast;
                RequestImageOption.NetworkAccessAllowed = true;
                RequestImageOption.Synchronous = false;
                _preselectedAssets = args.Assets;
                if (!_preselectedAssets.Any())
                {
                    //sem imagem selecionada
                    tcs.TrySetResult(null);
                }
                else
                {
                    var images = new List<string>();
                    var documentsDirectory = Environment.GetFolderPath
                                  (Environment.SpecialFolder.Personal);
                    int cnt = 1;
                    foreach (var asset in _preselectedAssets)
                    {
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            //Para cada imagem, cria um caminho
                            imageManager.RequestImageForAsset(asset,
                                PHImageManager.MaximumSize,
                                PHImageContentMode.Default,
                                RequestImageOption,
                                (image, info) =>
                                {
                                    using (NSAutoreleasePool autoreleasePool = new NSAutoreleasePool())
                                    {
                                        System.Diagnostics.Debug.WriteLine("Total memória usada: {0}", GC.GetTotalMemory(false));
                                        var filename = Guid.NewGuid().ToString();
                                        System.Diagnostics.Debug.WriteLine("arquivo: " + filename);
                                        string filepath = Save(image, filename.ToString(), documentsDirectory);
                                        System.Diagnostics.Debug.WriteLine("caminho: " + filepath);
                                        images.Add(filepath);

                                        //Quando for a ultima imagem, envia para o carousel view.
                                        if (cnt == args.Assets.Length)
                                        {
                                            Device.BeginInvokeOnMainThread(() =>
                                            {
                                                MessagingCenter.Send<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectediOS", images);
                                            });
                                        }
                                        cnt++;

                                        //Descarta os objetos e chama o garbage collector.
                                        asset.Dispose();
                                        autoreleasePool.Dispose();
                                        GC.Collect();
                                    }
                                });
                        });
                    }
                    tcs.TrySetResult(images);
                }
            };
            picker.FinishedPickingAssets += handler[0];

            cancelHandler[0] = (sender, args) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                picker.FinishedPickingAssets -= handler[0];
                picker.Canceled -= cancelHandler[0];
                tcs.TrySetResult(null);
            };
            picker.Canceled += cancelHandler[0];

            //Mostra as fotos
            picker.PresentUsingRootViewController();
        }

        string Save(UIImage origImage, string name, string documentsDirectory)
        {
            var compressionQuality = 0.46f;
            string jpgFilename = System.IO.Path.Combine(documentsDirectory, name);
            var resizedImage = MaxResizeImage(origImage, 1920f, 1080f);
            NSData imgData = resizedImage.AsJPEG(compressionQuality);
            NSError err = null;
            if (imgData.Save(jpgFilename, NSDataWritingOptions.Atomic, out err))
            {
                //Descarta objetos
                origImage.Dispose();
                resizedImage.Dispose();
                imgData.Dispose();
                return jpgFilename;
            }
            else
            {
                Console.WriteLine("Nao foi salvo o arquivo " + jpgFilename + " porque " + err.LocalizedDescription);
                return null;
            }
        }

        UIImage MaxResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
            if (maxResizeFactor > 1) return sourceImage;
            var width = maxResizeFactor * sourceSize.Width;
            var height = maxResizeFactor * sourceSize.Height;
            UIGraphics.BeginImageContext(new System.Drawing.SizeF((float)width, (float)height));
            sourceImage.Draw(new System.Drawing.RectangleF(0, 0, (float)width, (float)height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }

        /// <summary>
        ///     Quando deseja excluir os arquivos temporários
        /// </summary>
        void IGMMultiImagePicker.ClearFileDirectory()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var list = Directory.GetFiles(directory, "*");

            if (Directory.Exists(directory))
            {
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        File.Delete(list[i]);
                    }
                }
            }
        }


        /*
       Example of how to call ClearFileDirectory():

           if (Device.RuntimePlatform == Device.Android)
           {
               DependencyService.Get<IMediaService>().ClearFileDirectory();
           }
           if (Device.RuntimePlatform == Device.iOS)
           {
               GMMultiImagePicker.Current.ClearFileDirectory();
           }

       */
    }
}