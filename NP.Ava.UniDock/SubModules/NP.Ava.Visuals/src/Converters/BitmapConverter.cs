﻿using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;

namespace NP.Ava.Visuals.Converters
{
    public class BitmapConverter : IValueConverter
    {
        public static BitmapConverter Instance { get; } = new BitmapConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            Uri uri = new Uri(value.ToString(), UriKind.Absolute);

            if (AssetLoader.Exists(uri))
            {
                return new Bitmap(AssetLoader.Open(uri));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
