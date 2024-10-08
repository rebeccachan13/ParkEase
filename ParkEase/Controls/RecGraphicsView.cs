﻿using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IImage = Microsoft.Maui.Graphics.IImage;
using ParkEase.Core.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SharpCompress.Compressors.Xz;
using Microsoft.Maui;
using Syncfusion.Maui.Core.Internals;
using Microsoft.Maui.Handlers;
using Syncfusion.Maui.Core;
using CommunityToolkit.Maui.Views;
using System.IO;
using Microsoft.Maui.Controls.Compatibility;

namespace ParkEase.Controls
{
    public class RecGraphicsView : GraphicsView
    {
        /// <summary>
        /// Due to Rectangles setter will not be triggered when to collection was added an item, so RecCount must be changed to perform the draw method.
        /// </summary>
        /// 
        private static object drawlock = new object();
        private static RecGraphicsView _currentInstance;
        private double imageWidth = 1134;
        private double imageHeight = 830;

        public IImage ImageSource
        {
            get => (IImage)GetValue(ImageSourceProperty); set { SetValue(ImageSourceProperty, value); }
        }


        public ObservableCollection<Rectangle> ListRectangle
        {
            get => (ObservableCollection<Rectangle>)GetValue(ListRectangleProperty);
            set
            {
                SetValue(ListRectangleProperty, value);
            }
        }

        public ObservableCollection<Rectangle> ListRectangleFill
        {
            get => (ObservableCollection<Rectangle>)GetValue(ListRectangleFillProperty);
            set
            {
                SetValue(ListRectangleFillProperty, value);
            }
        }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(IImage), typeof(RecGraphicsView), propertyChanged: ImageSourcePropertyChanged);

        public static readonly BindableProperty ListRectangleProperty = BindableProperty.Create(nameof(ListRectangle), typeof(ObservableCollection<Rectangle>), typeof(RecGraphicsView), propertyChanged: ListRectanglePropertyChanged);

        public static readonly BindableProperty ListRectangleFillProperty = BindableProperty.Create(nameof(ListRectangleFill), typeof(ObservableCollection<Rectangle>), typeof(RecGraphicsView), propertyChanged: ListRectangleFillPropertyChanged);

        public RecGraphicsView()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                //PanGestureRecognizer panGesture = new PanGestureRecognizer();
                //PinchGestureRecognizer pinchGesture = new PinchGestureRecognizer();
                //panGesture.PanUpdated += OnPanUpdated;
                //pinchGesture.PinchUpdated += OnPinchUpdated;
                //GestureRecognizers.Add(pinchGesture);
                //GestureRecognizers.Add(panGesture);

                /*TapGestureRecognizer tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += OnTapGestureRecognizerTapped;
                GestureRecognizers.Add(tapGesture);*/
            }

        }

        private static async void ImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not RecGraphicsView { Drawable: RectDrawable drawable } view)
            {
                return;
            }
           

            drawable.ImageSource = (IImage)newValue;
            reRender(view);
            view.TranslationX = 0;
            view.TranslationY = 0;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                view.getDrawingInfo(drawable.ImageSource);
            }
        }

        private static void ListRectangle_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Triger reRender
            if (sender is ObservableCollection<Rectangle> listRectangle && listRectangle.Count >= 0)
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    reRender(_currentInstance);
                }
            }
        }

        private static void ListRectanglePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not RecGraphicsView { Drawable: RectDrawable drawable } view)
            {
                return;
            }
            ObservableCollection<Rectangle> listRectangle = (ObservableCollection<Rectangle>)newValue;
            listRectangle.CollectionChanged += ListRectangle_CollectionChanged;
            drawable.ListRectangle = listRectangle;
            _currentInstance = view;
            reRender(view);
        }

        // Mobile Rectangle list
        private static void ListRectangleFill_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Triger reRender
            if (sender is ObservableCollection<Rectangle> listRectangleFill && listRectangleFill.Count >= 0)
            {
                reRender(_currentInstance);
            }
        }

        private static void ListRectangleFillPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not RecGraphicsView { Drawable: RectDrawable drawable } view)
            {
                return;
            }
            ObservableCollection<Rectangle> listRectangleFill = (ObservableCollection<Rectangle>)newValue;
            listRectangleFill.CollectionChanged += ListRectangleFill_CollectionChanged;
            drawable.ListRectangleFill = listRectangleFill;
            _currentInstance = view;
            reRender(view);
        }

        private static void reRender(RecGraphicsView view)
        {
            lock (drawlock)
            {
                try
                {
                    view.Invalidate();
                }
                catch (Exception)
                {

                }
            }
        }

        private void getDrawingInfo(IImage image)
        {
            if (image == null) return;
            float viewRatio = 1134 / 830;
            float imageRatio = image.Width / image.Height;
            float offsetX, offsetY, drawWidth, drawHeight;

            if (imageRatio <= viewRatio)
            {
                drawHeight = 830;
                drawWidth = drawHeight / imageRatio;

                offsetY = 0;
                offsetX = (1134 - drawWidth) / 2;
            }
            else
            {
                drawWidth = 1134;
                drawHeight = drawWidth / imageRatio;

                offsetX = 0;
                offsetY = (830 - drawHeight) / 2;
            }

            imageWidth = drawWidth;
            imageHeight = drawHeight;

            double scaleX = Width / imageWidth;
            double scaleY = Height / imageHeight;
            if (scaleX < scaleY)
            {
                Scale = scaleX;
                TranslationX = -imageWidth * scaleX/4 - 30.5;
            }
            else
            {
                Scale = scaleY;
            }
        }

    }
}
