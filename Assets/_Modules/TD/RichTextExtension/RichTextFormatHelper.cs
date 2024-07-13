using System;
using System.Globalization;
using UnityEngine;

namespace TD.Utilities.RichTextExtension
{
    public static class RichTextFormatHelper
    {
        public static string RichTextFormat(int targetAmount, int maxDigits, Color frontDigitsColor, Color backDigitsColor)
        {
            if (maxDigits == 0)
            {
                return targetAmount.ToString();
            }

            string amountString = targetAmount.ToString("D" + maxDigits);
            int digitsToShow = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log10(targetAmount)) + 1, 0, maxDigits);
            int frontDigitsToShow = maxDigits - digitsToShow;

            string frontDigits = amountString.Substring(0, frontDigitsToShow);
            string remainingDigits = amountString.Substring(frontDigitsToShow);

            string formattedAmount = $"<color=#{ColorUtility.ToHtmlStringRGBA(frontDigitsColor)}>{frontDigits}</color>" +
                                     $"<color=#{ColorUtility.ToHtmlStringRGBA(backDigitsColor)}>{remainingDigits}</color>";

            return formattedAmount;
        }

        public static string RichTextFormat(int targetAmount, int maxDigits)
        {
            if (maxDigits == 0)
            {
                return targetAmount.ToString();
            }

            string amountString = targetAmount.ToString("D" + maxDigits);
            int digitsToShow = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log10(targetAmount)) + 1, 0, maxDigits);
            int frontDigitsToShow = maxDigits - digitsToShow;

            string frontDigits = amountString.Substring(0, frontDigitsToShow);
            string remainingDigits = amountString.Substring(frontDigitsToShow);

            return frontDigits + remainingDigits;
        }

        public static string RichTextFormat(int targetAmount, int maxDigits, string delimiter)
        {
            if (maxDigits == 0)
            {
                return targetAmount.ToString("N0", CultureInfo.InvariantCulture).Replace(",", delimiter);
            }

            int digitsToShow = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log10(targetAmount)) + 1, 0, maxDigits);

            if (digitsToShow > maxDigits)
            {
                targetAmount = (int)Mathf.Pow(10, maxDigits) - 1;
            }

            string amountString = targetAmount.ToString("N0", CultureInfo.InvariantCulture).Replace(",", delimiter);

            return amountString;
        }
        public static string RichTextFormat<T>(T targetAmount, int maxDigits, string delimiter) where T : IConvertible
        {
            double amount = Convert.ToDouble(targetAmount);

            if (maxDigits == 0)
            {
                return amount.ToString("N0", CultureInfo.InvariantCulture).Replace(",", delimiter);
            }

            int digitsToShow = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log10((float)amount)) + 1, 0, maxDigits);

            if (digitsToShow > maxDigits)
            {
                amount = Mathf.Pow(10, maxDigits) - 1;
            }

            string amountString = amount.ToString("N0", CultureInfo.InvariantCulture).Replace(",", delimiter);

            return amountString;
        }
    }

}
