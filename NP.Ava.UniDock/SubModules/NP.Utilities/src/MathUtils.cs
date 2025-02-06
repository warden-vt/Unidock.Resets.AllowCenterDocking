﻿// (c) Nick Polyak 2021 - http://awebpros.com/
// License: MIT License (https://opensource.org/licenses/MIT)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.
//
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace NP.Utilities
{
    public static class MathUtils
    {
        public const int Gig = 1000_000_000;


        public const int Million = 1000_000;

        public const int Thousand = 1000;


        public static T NonNegative<T>(this T d)
            where T : INumber<T>
        {
            int sign = T.Sign(d);

            if (sign < 0)
                d = T.Zero;

            return d;
        }

        // returns the value needed the add to d so that it would fit the Interval
        // if d is within the interval, 0 is returned.
        public static double BoundaryUpdate(this double d, double lowerBound, double upperBound)
        {
            if ((lowerBound != double.NaN) && (d < lowerBound))
            {
                return lowerBound - d;
            }
            if ((upperBound != double.NaN) && (d > upperBound))
            {
                return upperBound - d;
            }

            return 0;
        }

        public static bool IsDivisableBy(this int i, int divisor)
        {
            return i % divisor == 0;
        }

        public static string IntToStr(this int i, int divisor, string divisorLetter, Func<int, int, bool> condition)
        {
            if (!condition(i, divisor))
                return null;

            return string.Format("{0:##,#.###}", ((double) i)/ divisor) + divisorLetter;
        }

        public static string IntToStrIfDivisable(this int i, int divisor, string divisorLetter)
        {
            return i.IntToStr(divisor, divisorLetter, (intVal, div) => intVal.IsDivisableBy(div));
        }

        public static string IntToStrIfGreater(this int i, int divisor, string divisorLetter)
        {
            return i.IntToStr(divisor, divisorLetter, (intVal, div) => intVal >= div);
        }

        private static string IntToStr(this int intVal, Func<int, int, bool> condition)
        {
            string str =
                    intVal.IntToStr(MathUtils.Gig, "G", condition) ??
                    intVal.IntToStr(MathUtils.Million, "M", condition) ??
                    intVal.IntToStr(MathUtils.Thousand, "K", condition) ??
                    intVal.IntToStr(1, "", condition);

            return str;
        }

        public static string IntToStrIfDivisable(this int intVal)
        {
            return intVal.IntToStr((i, div) => i.IsDivisableBy(div));
        }

        public static string IntToStrIfGreater(this int intVal)
        {
            return intVal.IntToStr((i, div) => i >= div);
        }

        public static double NormalizeAngle(this double angle)
        {
            int numberCircles = (int)angle / 360;

            double result = angle - angle * numberCircles;

            if (result < 0)
                result += 360;

            return result;
        }

        public static double GetBestNorimalizedAngleAfterAngle(this double angleAfter, double angle)
        {
            angleAfter = angleAfter.NormalizeAngle();

            while(angleAfter < angle)
            {
                angleAfter += 360;
            }

            return angleAfter;
        }

        public static double Max(params double[] vals )
        {
            if (vals.IsNullOrEmpty())
            {
                return double.NegativeInfinity;
            }

            return vals.Max();
        }


        public static double Min(params double[] vals)
        {
            if (vals.IsNullOrEmpty())
            {
                return double.PositiveInfinity;
            }

            return vals.Min();
        }

        public static bool AlmostEquals(this double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 0.00000001;
        }

        public static string ToHex(this int i)
        {
            return i.ToString("X");
        }

        public static int ParseHex(this string hex)
        {
            return int.Parse(hex, NumberStyles.HexNumber);
        }

        public static double ToDouble(this string valStr, string varNameForErrorReporting)
        {
            if (!double.TryParse(valStr, out double d))
            {
                $"{varNameForErrorReporting} is '{valStr}' cannot be converted to a double".ThrowProgError();
            }

            return d;
        }
    }
}
