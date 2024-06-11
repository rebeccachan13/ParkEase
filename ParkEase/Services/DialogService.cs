﻿using ParkEase.Contracts.Services;
using ParkEase.Controls;
using ParkEase.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkEase.Services
{
    public class DialogService : IDialogService
    {
        private MyBottomSheet currentBottomSheet;

        public Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            return Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public async Task ShowPrivateMapBottomSheet(string address, string parkingFee, string limitHour)
        {
            // Close the existing bottom sheet if it exists
            if (currentBottomSheet != null)
            {
                await currentBottomSheet.DismissAsync();
            }

            var bottomSheetViewModel = new BottomSheetViewModel
            {
                Address = address,
                ParkingFee = parkingFee,
                LimitHour = limitHour,
                
            };

            currentBottomSheet = new MyBottomSheet(bottomSheetViewModel)
            {
                HasHandle = true,
                HandleColor = Colors.Black
            };

            await currentBottomSheet.ShowAsync();
        }
    }
}
