﻿using CarouselView.FormsPlugin.iOS;
using FFImageLoading;
using FFImageLoading.Forms.Platform;
using Foundation;
using System;
using UIKit;

namespace AppXamarinFotoMultiPlataforma.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            //NuGet Initializations
            CarouselViewRenderer.Init();
            CachedImageRenderer.Init();
            var config = new FFImageLoading.Config.Configuration()
            {
                MaxMemoryCacheSize = (int)(NSProcessInfo.ProcessInfo.PhysicalMemory * 0.0001d),
                ClearMemoryCacheOnOutOfMemory = true,
                DiskCacheDuration = TimeSpan.FromSeconds(2)
            };
            ImageService.Instance.Initialize(config);

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
