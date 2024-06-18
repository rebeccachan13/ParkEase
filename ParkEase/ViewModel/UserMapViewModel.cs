﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Platform;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using ParkEase.Contracts.Services;
using ParkEase.Controls;
using ParkEase.Core.Contracts.Services;
using ParkEase.Core.Data;
using ParkEase.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ParkEase.ViewModel
{
    public partial class UserMapViewModel : ObservableObject
    {
        private List<MapLine> dbMapLines;

        [ObservableProperty]
        private ObservableCollection<MapLine> mapLines; 

        [ObservableProperty]
        private MapLine selectedMapLine;

        [ObservableProperty]
        private string availableSpots;

        [ObservableProperty]
        private string selectedRadius;

        [ObservableProperty]
        private double radius;

        private readonly IMongoDBService mongoDBService;
        private readonly IDialogService dialogService;
        public UserMapViewModel(IMongoDBService mongoDBService, IDialogService dialogService)
        {
            this.mongoDBService = mongoDBService;
            this.dialogService = dialogService;
        }
        public ICommand LoadedEventCommand => new RelayCommand<EventArgs>(async e =>
        {
            await LoadMapDataAsync();
        });

        // Fetches parking data from the database and displays it on the map
        private async Task LoadMapDataAsync()
        {
            try
            {
                var parkingDatas = await mongoDBService.GetData<ParkingData>(CollectionName.ParkingData);
                if (parkingDatas == null || !parkingDatas.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No parking data found.");
                    return;
                }

                var lines = new ObservableCollection<MapLine>(); // Collection of lines to be displayed on the map

                foreach (var pd in parkingDatas)
                {
                    if (pd.Points.Count > 1)
                    {
                        var color = await GetLineColorAsync(pd.Id); // Get the color of the line based on the availability of parking spots
                        lines.Add(new MapLine(pd.Points, color)); // Add the line to the collection
                    }
                }

                dbMapLines = new List<MapLine>(lines);
                //MapLines = lines;
                await LoadAvailableSpotsAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading map data: {ex.Message}");
            }
        }

        // Gets the color of the line based on the availability of parking spots
        private async Task<string> GetLineColorAsync(string parkingDataId)
        {
            var statuses = await mongoDBService.GetData<PublicStatus>(CollectionName.PublicStatus); // Get the statuses of the parking spots
            var availableSpots = statuses.Count(status => status.AreaId == parkingDataId && !status.Status); // Count the number of available spots where the status is false
            return availableSpots > 0 ? "green" : "red"; // Return green if there are available spots, red otherwise
        }

        // Loads the total number of available spots
        private async Task LoadAvailableSpotsAsync(string parkingDataId)
        {
            try
            {
                var statuses = await mongoDBService.GetData<PublicStatus>(CollectionName.PublicStatus);
                var count = string.IsNullOrEmpty(parkingDataId) // If the parkingDataId is null, count the total number of available spots
                    ? statuses.Count(status => !status.Status)  // Otherwise, count the number of available spots for the specific parkingDataId
                    : statuses.Count(status => status.AreaId == parkingDataId && !status.Status); // Count the number of available spots where the status is false

                AvailableSpots = count.ToString(); // Set the total number of available spots
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading available spots: {ex.Message}");
            }
        }

        // Handles the event when a line is clicked
        public async Task OnLineClickedAsync(MapLine selectedLine)
        {
            if (selectedLine == null) return;

            try
            {
                var filter = Builders<ParkingData>.Filter.Eq(pd => pd.Points, selectedLine.Points); // Filter the parking data based on the selected line
                var parkingDataList = await mongoDBService.GetDataFilter<ParkingData>(CollectionName.ParkingData, filter); // Get the parking data based on the filter

                if (parkingDataList == null || !parkingDataList.Any()) // If no parking data is found, show an alert
                {
                    await dialogService.ShowAlertAsync("No Data Found", "No parking data found for the selected line.");
                    return;
                }
                
                // Get the address, parking fee, limited hour, parking data id, latitude, and longitude
                var parkingData = parkingDataList.First();
                var address = parkingData.ParkingSpot;
                var parkingFee = parkingData.ParkingFee;
                var limitedHour = parkingData.ParkingTime;
                var parkingDataId = parkingData.Id;
                var lat = parkingData.Points[1].Lat;
                var lng = parkingData.Points[1].Lng;

                // Load the available spots and show the bottom sheet
                await LoadAvailableSpotsAsync(parkingDataId);
                
                // Show the bottom sheet with the address, parking fee, limited hour, available spots, and a button to show the directions
                await dialogService.ShowBottomSheet(address, parkingFee, limitedHour, $"{AvailableSpots} Available Spots", true, lat, lng);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving parking data: {ex.Message}");
            }
        }

        public ICommand UpdateRangeCommand => new RelayCommand(async() =>
        {
            // parse selectedRadius to int 200 meters > 0.2
            if (string.IsNullOrEmpty(SelectedRadius))
            {
                Debug.WriteLine("SelectedRadius is null or empty");
                return;
            }

            // Parse selected radius to double (meters to kilometers)
            if (!double.TryParse(SelectedRadius.Split(' ')[0], out double radius_out))
            {
                Debug.WriteLine("Failed to parse SelectedRadius");
                return;
            }

            radius_out /= 1000.0;
            var location = await Geolocation.GetLocationAsync();

            List<MapLine> linesInRange = dbMapLines.Where(line => isPointInCircle(line.Points, location.Latitude, location.Longitude, radius_out)).ToList();
            MapLines = new ObservableCollection<MapLine>(linesInRange);
            Radius = radius_out;
        });


        private bool isPointInCircle(List<MapPoint> points, double centerLat, double centerLng, double radius)
        {
            foreach(MapPoint point in points)
            {
                double pointLat = double.Parse(point.Lat);
                double pointLng = double.Parse(point.Lng);

                var distance = Math.Sqrt(Math.Pow(pointLat - centerLat, 2) + Math.Pow(pointLng - centerLng, 2));
                if(distance <= (radius / 111))
                {
                    return true;
                }
            }

            return false;
        }

    }
}