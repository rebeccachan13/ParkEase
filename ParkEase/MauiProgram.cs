﻿using Camera.MAUI;
using CommunityToolkit.Maui;
using epj.RouteGenerator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkEase.Contracts.Services;
using ParkEase.Core.Contracts.Services;
using ParkEase.Core.Model;
using ParkEase.Core.Services;
using ParkEase.Page;
using ParkEase.Services;
using ParkEase.ViewModel;
using Syncfusion.Maui.Core.Hosting;
using System.Reflection;
using The49.Maui.BottomSheet;
using UraniumUI;
using ZXing.Net.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui.Storage;
using Plugin.LocalNotification;

namespace ParkEase
{
	[AutoRoutes("Page")]
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            bool developerMode = true;
            var builder = MauiApp.CreateBuilder();

            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("ParkEase.appsettings.json");

            var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();

            builder.Configuration.AddConfiguration(config);

            builder.UseMauiApp<App>()
				.UseSkiaSharp(true)
				.UseUraniumUI()
                .UseUraniumUIMaterial()
                .UseLocalNotification()
                .UseMauiCommunityToolkit()
                .UseBottomSheet()
                .UseMauiCameraView()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddMaterialIconFonts(); /* https://enisn-projects.io/docs/en/uranium/latest/theming/Icons#material-icons*/
                })
                .ConfigureEssentials(essentials =>
                {
                    essentials.UseMapServiceToken(config["BingAPIKey"].ToString());
                })
                .UseBarcodeReader();

            builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
            builder.Services.AddTransient<PrivateMapViewModel>();

            #region page

            builder.Services.AddTransient<SignUpViewModel>();
            builder.Services.AddTransient(provider => new SignUpPage
            {
                BindingContext = provider.GetRequiredService<SignUpViewModel>()
            });

            builder.Services.AddTransient<ForgotPasswordViewModel>();
            builder.Services.AddTransient(provider => new ForgotPasswordPage
            {
                BindingContext = provider.GetRequiredService<ForgotPasswordViewModel>()
            });

            builder.Services.AddTransient<ResetPasswordViewModel>();
            builder.Services.AddTransient(provider => new ResetPasswordPage
            {
                BindingContext = provider.GetRequiredService<ResetPasswordViewModel>()
            });

            builder.Services.AddSingleton<LogInViewModel>();
            builder.Services.AddSingleton(provider => new LogInPage
            {
                BindingContext = provider.GetRequiredService<LogInViewModel>()
            });

        
            builder.Services.AddSingleton<MapViewModel>();
            builder.Services.AddSingleton(provider => new MapPage
            {
                BindingContext = provider.GetRequiredService<MapViewModel>()
            });

            builder.Services.AddSingleton<CreateMapViewModel>();
            builder.Services.AddSingleton(provider => new CreateMapPage
            {
                BindingContext = provider.GetRequiredService<CreateMapViewModel>()
            });

            builder.Services.AddSingleton<UserMapViewModel>();
            builder.Services.AddSingleton(provider => new UserMapPage
            {
                BindingContext = provider.GetRequiredService<UserMapViewModel>()
            });
            
            builder.Services.AddSingleton<PrivateMapViewModel>();
            builder.Services.AddSingleton(provider => new PrivateMapPage
            {
                BindingContext = provider.GetRequiredService<PrivateMapViewModel>()
            });

            builder.Services.AddSingleton<PrivateSearchViewModel>();
            builder.Services.AddSingleton(provider => new PrivateSearchPage
            {
                BindingContext = provider.GetRequiredService<PrivateSearchViewModel>()
            });


            builder.Services.AddSingleton<AnalysisViewModel>();
            builder.Services.AddSingleton(provider => new AnalysisPage()
            {
                BindingContext = provider.GetRequiredService<AnalysisViewModel>()
            });

            builder.Services.AddSingleton<PrivateStatusViewModel>();
            builder.Services.AddSingleton(provider => new PrivateStatusPage()
            {
                BindingContext = provider.GetRequiredService<PrivateStatusViewModel>()
            });
            #endregion

            builder.Services.AddSingleton(provider => new ParkEaseModel(developerMode));

            #region service
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<IAWSService, AWSService>();
            builder.Services.AddSingleton<IMongoDBService>(provider => new MongoDBService(provider.GetRequiredService<IAWSService>(), DeviceInfo.Platform, false));
            builder.Services.AddSingleton<IGeolocatorService, GeolocatorService>();
            builder.Services.AddSingleton<IGeocodingService, GeocodingService>();
            #endregion

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
