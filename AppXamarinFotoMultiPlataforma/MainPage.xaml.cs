using AppXamarinFotoMultiPlataforma.ImagePickers.Android;
using AppXamarinFotoMultiPlataforma.ImagePickers.iOS;
using AppXamarinFotoMultiPlataforma.ImagePickers.UWP;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppXamarinFotoMultiPlataforma
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Device.BeginInvokeOnMainThread(async () => await AskForPermissions());
        }

        private async Task<bool> AskForPermissions()
        {
            try
            {
                await CrossMedia.Current.Initialize();
                var storagePermissions = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                var photoPermissions = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);
                if (storagePermissions != PermissionStatus.Granted || photoPermissions != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage, Permission.Photos });
                    storagePermissions = results[Permission.Storage];
                    photoPermissions = results[Permission.Photos];
                }

                if (storagePermissions != PermissionStatus.Granted || photoPermissions != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permissão Negada!", "Por favor verifique as configurações do seu dispositvo para liberar o uso!", "Ok");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("erro. Permissão não liberada. Stacktrace: \n" + ex.StackTrace);
                return false;
            }
        }

        private async void SelectImagesButton_Clicked(object sender, EventArgs e)
        {
            //verifica as permissões do usuário
            var storagePermissions = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            var photoPermissions = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);
            if (storagePermissions == PermissionStatus.Granted && photoPermissions == PermissionStatus.Granted)
            {
                //se for iOS, usa o GMMultiImagePicker.
                if (Device.RuntimePlatform == Device.iOS)
                {
                    //caso imagem seja modificado pelo usuário precisa mudar o modo para HighQualityFormat
                    bool imageModifiedWithDrawings = false;
                    if (imageModifiedWithDrawings)
                    {
                        await GMMultiImagePicker.Current.PickMultiImage(true);
                    }
                    else
                    {
                        await GMMultiImagePicker.Current.PickMultiImage();
                    }

                    MessagingCenter.Unsubscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectediOS");
                    MessagingCenter.Subscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectediOS", (s, images) =>
                    {
                        //Se tem imagens, joga no carroseul e mostra
                        if (images.Count > 0)
                        {
                            ImgCarouselView.ItemsSource = images;
                        }
                    });
                }
                //se for no android, executa IMediaService.
                else if (Device.RuntimePlatform == Device.Android)
                {
                    DependencyService.Get<IMediaService>().OpenGallery();

                    MessagingCenter.Unsubscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid");
                    MessagingCenter.Subscribe<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid", (s, images) =>
                    {
                        //Se tem imagens, joga no carroseul e mostra
                        if (images.Count > 0)
                        {
                            ImgCarouselView.ItemsSource = images;
                        }
                    });
                }

                //se for no uwp, executa IMediaService.
                else if (Device.RuntimePlatform == Device.UWP)
                {

                }
            }
            else
            {
                await DisplayAlert("Permissão negada!", "\n Por favor configure a liberação em seu dispositivo.", "Ok");
            }
        }

    }
}
