using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema1.Model
{
    class CalculatorModel
    {
        public decimal FirstOperand { get; set; }
        public decimal SecondOperand { get; set; }
        public string CurrentOperator { get; set; }
        private static readonly Dictionary<int, string> ValidCharactersByBase = new()
        {
            { 2, "01" },
            { 8, "01234567" },
            { 10, "0123456789" },
            { 16, "0123456789ABCDEF" }
        };

        public static bool IsValidKey(string key, int currentBase)
        {
            return ValidCharactersByBase.TryGetValue(currentBase, out var validChars) && validChars.Contains(key);
        }
        public void SetOperand(decimal value)
        {
            if (CurrentOperator == null)
                FirstOperand = value;
            else
                SecondOperand = value;
        }
        public void SetOperator(string newOperator)
        {
            if (CurrentOperator != null && newOperator != "%")
                Compute();
            CurrentOperator = newOperator;
        }
        public decimal Compute()
        {
            decimal result = -1;
            switch (CurrentOperator)
            {
                case "+":
                    result = FirstOperand + SecondOperand;
                    break;
                case "-":
                    result = FirstOperand - SecondOperand;
                    break;
                case "*":
                    result = FirstOperand * SecondOperand;
                    break;
                case "/":
                    result = FirstOperand / SecondOperand;
                    break;
            }

            FirstOperand = result;
            SecondOperand = 0;
            CurrentOperator = null;

            return result;
        }
        public void ChangeSign()
        {
            if (CurrentOperator == null)
                FirstOperand = -FirstOperand;
            else
                SecondOperand = -SecondOperand;
        }

        public decimal SquareRoot()
        {
            decimal result;
            if (CurrentOperator == null)
            {
                if (FirstOperand < 0)
                    return -1;
                FirstOperand = (decimal)Math.Sqrt((double)FirstOperand);
                result = FirstOperand;
            }
            else
            {
                if (SecondOperand < 0)
                    return -1;
                SecondOperand = (decimal)Math.Sqrt((double)SecondOperand);
                result = SecondOperand;
            }

            FirstOperand = result;
            SecondOperand = 0;
            CurrentOperator = null;

            return result;
        }

        public decimal Percentage()
        {
            decimal result = -1;
            if (CurrentOperator == "*" || CurrentOperator == "/")
                result = SecondOperand < 0 ? -(-SecondOperand / 100)  : (SecondOperand / 100);
            else
                result = FirstOperand < 0 ? FirstOperand * -(-SecondOperand / 100) :  FirstOperand * (SecondOperand / 100);
            
            SecondOperand = result;
            return result;
        }
        public decimal Invert()
        {
            if (CurrentOperator == null)
            {
                FirstOperand = 1 / FirstOperand;
                return FirstOperand;
            }
            else
            {
                SecondOperand = 1 / SecondOperand;
                return SecondOperand;
            }
        }
        public decimal Square()
        {
            if (CurrentOperator == null)
            {
                FirstOperand *= FirstOperand;
                return FirstOperand;
            }
            else
            {
                SecondOperand *= SecondOperand;
                return SecondOperand;
            }
        }
        public static double EvaluateExpression(string expr)
        {
                return Convert.ToDouble(new System.Data.DataTable().Compute(expr, null));
        }
    }
}
