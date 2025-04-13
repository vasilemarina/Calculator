using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using Tema1.Model;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Tema1.ViewModel
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        private readonly CalculatorModel Model;
        private string displayText;
        private string previousText;
        private bool standardModeOn;
        private bool digitGroupingOn;
        private bool operationsOrderOn;
        private static CultureInfo currentCulture;
        private ObservableCollection<double> memoryValues = new ObservableCollection<double>();
        private bool showMemoryPanel;
        private Visibility memoryListVisibility;
        private byte currentBase;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Properties
        public string DisplayText
        {
            get
            {
                return displayText;
            }
            set
            {
                displayText = value;
                NotifyPropertyChanged("DisplayText");
            }
        }
        public string PreviousText
        {
            get
            {
                return previousText;
            }
            set
            {
                previousText = value;
                NotifyPropertyChanged("PreviousText");
            }
        }
        public string CurrentInput { get; set; }
        public static List<List<string>> StandardKeyLabels { get; }
        public static List<string> MemoryLabels { get; }
        public static List<List<string>> ProgrammerKeyLabels { get; }
        public static List<byte> Bases { get; set; }
        public bool StandardModeOn
        {
            get
            {
                return standardModeOn;
            }
            set
            {
                if (StandardModeOn != value)
                {
                    standardModeOn = value;
                    NotifyPropertyChanged("StandardModeOn");
                    NotifyPropertyChanged("StandardModeVisibility");
                    NotifyPropertyChanged("ProgrammerModeVisibility");
                    NotifyPropertyChanged("ModeName");

                    Properties.Settings.Default.StandardModeOn = StandardModeOn;
                    Properties.Settings.Default.Save();
                }
                if (StandardModeOn) CurrentBase = 10;
            }
        }
        public bool DigitGroupingOn
        {
            get 
            {
                return digitGroupingOn;
            }
            set
            {
                digitGroupingOn = value;
                FormatNumberDisplay();

                Properties.Settings.Default.DigitGroupingOn = digitGroupingOn;
                Properties.Settings.Default.Save();
            }
        }
        public bool OperationsOrder
        {
            get
            {
                return operationsOrderOn;
            }
            set
            {
                FormatNumberDisplay();
                
                Properties.Settings.Default.OperationsOrder = value;
                Properties.Settings.Default.Save();
                
            }
        }
        public Visibility StandardModeVisibility
        {
            get
            {
                return StandardModeOn ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility MemoryListVisibility
        {
            get
            {
                return memoryListVisibility;
            }
            set
            {
                memoryListVisibility = value;
                NotifyPropertyChanged("MemoryListVisibility");
            }
        }
        public Visibility ProgrammerModeVisibility
        {
            get
            {
                return StandardModeOn ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public string ModeName => StandardModeOn ? "Standard" : "Programmer";
        public static CultureInfo CurrentCulture
        { 
            get
            {
                return currentCulture;
            }
            set
            {
                currentCulture = value;
            }
        }
        public ObservableCollection<double> MemoryValues
        {
            get { return memoryValues; }
            set 
            { 
                memoryValues = value;
                NotifyPropertyChanged(nameof(MemoryValues));
            }
        }
        public bool ShowMemoryPanel
        {
            get 
            {
                return showMemoryPanel;
            }
            set 
            { 
                showMemoryPanel = value; 
                NotifyPropertyChanged(nameof(ShowMemoryPanel));
            }
        }
        public byte CurrentBase
        {
            get => currentBase;
            set
            {
                byte previousBase = currentBase;
                currentBase = value;

                NotifyPropertyChanged(nameof(CurrentBase));
                ConvertToSelectedBase(previousBase);

                Properties.Settings.Default.CurrentBase = currentBase;
                Properties.Settings.Default.Save();
            }
        }
        private string Expression { get; set; }
        #endregion

        static CalculatorViewModel()
        {
            StandardKeyLabels = new List<List<string>>() {
                new List<string> { "%",   "ce",  "c",       "<=" },
                new List<string> { "1/x", "x^2", "sqrt(x)", "/" },
                new List<string> { "7",   "8",   "9",       "*" },
                new List<string> { "4",   "5",   "6",       "-" },
                new List<string> { "1",   "2",   "3",       "+" },
                new List<string> { "+/-", "0",   ".",       "=" },
            };

            MemoryLabels = new List<string> { "MC", "MR", "M+", "M-", "MS", "M>" };

            ProgrammerKeyLabels = new List<List<string>>
            {
                new List<string> { "A", "ce",  "c", "<=", "sqrt(x)" },
                new List<string> { "B", "(",   ")", "%",  "/" },
                new List<string> { "C", "7",   "8", "9",  "*" },
                new List<string> { "D", "4",   "5", "6",  "-" },
                new List<string> { "E", "1",   "2", "3",  "+" },
                new List<string> { "F", "+/-", "0", ".",  "=" },

            };

            CurrentCulture = CultureInfo.CurrentCulture;
            Bases = new List<byte>{ 16, 10, 8, 2}; 
        }
        public CalculatorViewModel()
        {
            Model = new CalculatorModel();
  
            DisplayText = "0";
            PreviousText = "";

            StandardModeOn = Properties.Settings.Default.StandardModeOn;
            DigitGroupingOn = Properties.Settings.Default.DigitGroupingOn;
            OperationsOrder = Properties.Settings.Default.OperationsOrder;

            if (!StandardModeOn) 
                CurrentBase = Properties.Settings.Default.CurrentBase;
            else 
                CurrentBase = 10;

            MemoryListVisibility = Visibility.Collapsed;
            operationsOrderOn = Properties.Settings.Default.OperationsOrder;
        }
        private void FormatNumberDisplay()
        {
            if (StandardModeOn)
            {
                string rawNumber = DisplayText.Replace(CurrentCulture.NumberFormat.NumberGroupSeparator, "");

                if (decimal.TryParse(rawNumber, out decimal number))
                    DisplayText = DigitGroupingOn
                        ? number.ToString("#,0.####", CurrentCulture)
                        : number.ToString("G", CurrentCulture);

                if (PreviousText != "")
                {
                    string[] parts = PreviousText.Split(' ');
                    for (int i = 0; i < parts.Length; i++)
                        if (decimal.TryParse(parts[i], out decimal currentNum))
                        {
                            parts[i] = DigitGroupingOn
                                ? currentNum.ToString("#,0.####", CurrentCulture)
                                : currentNum.ToString("G", CurrentCulture);
                        }
                    
                    PreviousText = string.Join(" ", parts);
                }
            }
        }
        public void OnButtonPressed(string buttonLabel)
        {
            decimal result;
            switch(buttonLabel)
            {
                case "c":
                    Clear();                 
                    break;
                case "ce":
                    ClearExpression();
                    break;
                case "=":
                    if (CurrentInput == "" && Model.CurrentOperator != null)
                        Model.SecondOperand = Model.FirstOperand;
                    
                    PreviousText = $"{Model.FirstOperand} {Model.CurrentOperator} {Model.SecondOperand} = ";
                    if (Model.CurrentOperator == "/" && Model.SecondOperand == 0)
                    {
                        DisplayText = "Cannot divide by zero";
                        break;
                    }
                    if(CurrentBase != 16 && CurrentBase != 10)
                    {
                        Model.FirstOperand = BaseConverter.ConvertToDecimal(Model.FirstOperand.ToString(), CurrentBase);
                        Model.SecondOperand = BaseConverter.ConvertToDecimal(Model.SecondOperand.ToString(), CurrentBase);
                    }

                    result = Model.Compute();
                    DisplayText = BaseConverter.ConvertFromDecimal((int)result, CurrentBase);
                    CurrentInput = "";
                    FormatNumberDisplay();

                    if (CurrentBase == 16)
                    {
                        string[] parts = PreviousText.Split(' ');
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (decimal.TryParse(parts[i], out decimal currentNum))
                            {
                                parts[i] = BaseConverter.ConvertFromDecimal((int)currentNum, CurrentBase);
                            }
                        }
                        PreviousText = string.Join(" ", parts);
                    }
                    break;
                case "+/-":
                    Model.ChangeSign();
                    if (Model.CurrentOperator != null)
                        DisplayText = Model.SecondOperand.ToString();
                    else
                        DisplayText = Model.FirstOperand.ToString();
                        if (DisplayText[0] == '-')
                        CurrentInput = CurrentInput.Insert(0, "-");
                    break;
                case "x^2":
                    if (Model.CurrentOperator == null)
                        PreviousText = $"sqr( {Model.FirstOperand} )";
                    else
                        PreviousText = $"sqr( {Model.SecondOperand} )";
                    result = Model.Square();
                    DisplayText = result.ToString();
                    FormatNumberDisplay();
                    break;
                case "sqrt(x)":
                    result = Model.SquareRoot();
                    if(result == -1)
                        DisplayText = "Invalid input";
                    else
                    {
                        DisplayText = result.ToString();
                        PreviousText = $"sqrt( {CurrentInput} )";
                    }
                    FormatNumberDisplay();
                    break;
                case "<=":
                    DisplayText = DisplayText[..^1];
                    if (DisplayText == "")
                    {
                        DisplayText = "0";
                        CurrentInput = null;
                    }
                    if (Model.CurrentOperator == null)
                    {
                        if (CurrentBase == 16)
                            Model.FirstOperand = BaseConverter.ConvertToDecimal(CurrentInput, 16);
                        else
                        Model.FirstOperand = decimal.Parse(DisplayText);
                    }
                    else
                        Model.SecondOperand = decimal.Parse(DisplayText);
                    break;
                case "1/x":
                    if (Model.CurrentOperator == null)
                        PreviousText += $"1/( {Model.FirstOperand} ) ";
                    else
                    {
                        if (CurrentInput == "")
                            Model.SecondOperand = Model.FirstOperand;
                        else
                            Model.SecondOperand = decimal.Parse(CurrentInput);
                        PreviousText += $"1/( {Model.SecondOperand} ) ";
                    }
                    if (Model.CurrentOperator == null && Model.FirstOperand == 0
                        || Model.CurrentOperator != null && Model.SecondOperand == 0)
                        DisplayText = "Cannot divide by zero";
                    else
                    {
                        result = Model.Invert();
                        DisplayText = result.ToString();
                    }
                    FormatNumberDisplay();
                    break;
                case "%":
                    if(Model.CurrentOperator != null)
                    {
                        Model.SecondOperand = decimal.Parse(CurrentInput);     
                        Model.SecondOperand = Model.Percentage();                   
                    }
                    else
                    {
                        result = decimal.Parse(CurrentInput);
                        result /= 100;
                    }
                    PreviousText += $" {Model.SecondOperand} ";
                    DisplayText = Model.SecondOperand.ToString();
                    FormatNumberDisplay();
                    break;
                default:
                    if ((decimal.TryParse(buttonLabel, out _) || buttonLabel == "."))
                    {
                        if (buttonLabel != "." && !BaseConverter.IsValidKey(buttonLabel, CurrentBase)) 
                            break;

                        CurrentInput += buttonLabel;

                        if (Model.CurrentOperator == null)
                            if (CurrentBase == 16)
                                Model.FirstOperand = BaseConverter.ConvertToDecimal(CurrentInput, 16);
                            else
                                Model.FirstOperand = decimal.Parse(CurrentInput);
                        else
                            Model.SetOperand(decimal.Parse(CurrentInput));

                        DisplayText = CurrentInput;
                    }
                    else if (BaseConverter.IsValidDigit(buttonLabel, CurrentBase))
                    {
                        if (!BaseConverter.IsValidKey(buttonLabel, CurrentBase))
                            break;

                        CurrentInput += buttonLabel;

                        if (Model.CurrentOperator == null)
                            Model.FirstOperand = BaseConverter.ConvertToDecimal(CurrentInput, CurrentBase);
                        else
                            Model.SetOperand(BaseConverter.ConvertToDecimal(CurrentInput, CurrentBase));

                        DisplayText = CurrentInput;
                    }
                    else if(buttonLabel == "(" || buttonLabel == ")")
                        break;
                    else
                    { 
                        Model.SetOperator(buttonLabel);
                        if(currentBase != 16)
                         PreviousText = $" {Model.FirstOperand} {Model.CurrentOperator} ";
                        else
                            PreviousText = $" {BaseConverter.ConvertFromDecimal((int)Model.FirstOperand, 16)} {Model.CurrentOperator} ";

                        DisplayText = Model.FirstOperand.ToString();
                        CurrentInput = "";
                    }
                    FormatNumberDisplay();
                    break;
            }
        }
        public void OnButtonPressedOrder(string buttonLabel)
        {
            decimal result;
            switch (buttonLabel)
            {
                case "c":
                    Clear();
                    break;
                case "ce":
                    ClearExpression();
                    break;
                default:
                    if (buttonLabel == "=")
                    {
                        DisplayText = CalculatorModel.EvaluateExpression(Expression).ToString();
                        Expression = "";
                    }
                    else
                    {
                        if (!new List<string>{"+", "-", "*", "/", "=", "." }.Contains(buttonLabel)  && !BaseConverter.IsValidKey(buttonLabel, CurrentBase)) break;
                        Expression += buttonLabel;
                        DisplayText = Expression;
                    }
                    break;
            }
        }
        public void OnMemoryButtonPressed(string buttonLabel)
        {
            switch (buttonLabel)
            {
                case "MC":
                    MemoryValues.Clear();
                    break;
                case "M+":
                    if (MemoryValues.Count != 0 && double.TryParse(DisplayText, out double addValue))
                        MemoryValues[^1] += (addValue);
                    break;
                case "M-":
                    if (MemoryValues.Count != 0 && double.TryParse(DisplayText, out double subValue))
                        MemoryValues[^1] -= subValue;
                    break;
                case "MR":
                    if (MemoryValues.Count > 0)
                        DisplayText = MemoryValues.Last().ToString();
                    break;
                case "MS":
                    if (double.TryParse(DisplayText, out double storeValue))
                        MemoryValues.Add(storeValue); 
                    break;
                case "M>":
                    MemoryListVisibility = MemoryListVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }

            NotifyPropertyChanged(nameof(DisplayText));
            NotifyPropertyChanged(nameof(ShowMemoryPanel));
        }
        public void CutText()
        {
            if(DisplayText != "")
            {
                Clipboard.SetText(DisplayText);
                DisplayText = "";
                CurrentInput = "";
                if (Model.CurrentOperator == null)
                    Model.FirstOperand = 0;
                else
                    Model.SecondOperand = 0;
                NotifyPropertyChanged("DisplayText");
            }
        }
        public void CopyText()
        {
            if (CurrentInput != "")
                Clipboard.SetText(DisplayText);
        }
        public void PasteText()
        {
            if (Clipboard.ContainsText())
            {
                DisplayText = Clipboard.GetText();
                CurrentInput = Clipboard.GetText();
                if (Model.CurrentOperator == null)
                    Model.FirstOperand = decimal.Parse(CurrentInput);
                else
                    Model.SecondOperand = decimal.Parse(CurrentInput);
                NotifyPropertyChanged("DisplayText");
            }
        }
        public void GetOperandFromMemory(decimal memoryNumber)
        {
            Model.SetOperand(memoryNumber);
            DisplayText = memoryNumber.ToString();
            CurrentInput = memoryNumber.ToString();
            FormatNumberDisplay();
        }
        public void ConvertToSelectedBase(byte previousBase)
        {
            if (int.TryParse(CurrentInput, out _))
            {
                CurrentInput = BaseConverter.ConvertBase(CurrentInput, previousBase, CurrentBase);
                DisplayText = CurrentInput;
                NotifyPropertyChanged("DisplayText");
            }
        }
        private void Clear()
        {
            Model.FirstOperand = 0;
            Model.SecondOperand = 0;
            Model.CurrentOperator = null;
            DisplayText = "0";
            PreviousText = "";
            CurrentInput = null;
        }
        private void ClearExpression()
        {
            if (Model.CurrentOperator == null)
                Model.FirstOperand = 0;
            else
                Model.SecondOperand = 0;
            DisplayText = "0";
            CurrentInput = null;
        }
    }
}
