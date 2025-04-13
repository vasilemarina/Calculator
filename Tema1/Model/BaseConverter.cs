using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema1.Model
{
    public class BaseConverter
    {
        static string Digits = "0123456789ABCDEF";

        public static int ConvertToDecimal(string number, int sourceBase)
        {
            int result = 0;
            foreach (char digit in number.ToUpper())
                result = result * sourceBase + Digits.IndexOf(digit);
            
            return result;
        }
        public static string ConvertFromDecimal(int number, int targetBase)
        {
            if (number == 0) 
                return "0";

            string result = "";
            while (number > 0)
            {
                result = Digits[number % targetBase] + result;
                number /= targetBase;
            }
            return result;
        }
        public static string ConvertBase(string number, int sourceBase, int targetBase)
        {
            int decimalValue = ConvertToDecimal(number, sourceBase);
            return ConvertFromDecimal(decimalValue, targetBase);
        }
        public static bool IsValidDigit(string input, byte CurrentBase)
        {
            if (CurrentBase == 16)
                return int.TryParse(input, System.Globalization.NumberStyles.HexNumber, null, out _) || input == ".";
            else
                return int.TryParse(input, out _) || input == ".";
        }
        public static bool IsValidKey(string digit, int currentBase)
        {
            if (currentBase <= 10)
                return digit.Length == 1 && digit[0] >= '0' && digit[0] < ('0' + currentBase);
            else
                return digit.Length == 1 &&
                       ((digit[0] >= '0' && digit[0] <= '9') ||
                        (digit[0] >= 'A' && digit[0] < ('A' + (currentBase - 10))));
        }

    }
}
