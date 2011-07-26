using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace oradmin
{
    [ValueConversion(typeof(string), typeof(bool))]
    class StringToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = value.ToString();
            EStringToBoolConverterOption option = (EStringToBoolConverterOption)parameter;

            switch (option)
            {
                case EStringToBoolConverterOption.YesNo:

                    if (strValue.Equals("YES"))
                        return true;
                    else
                        return false;

                case EStringToBoolConverterOption.RequiredNull:

                    if (strValue.Equals("REQUIRED"))
                        return true;
                    else
                        return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolValue = (bool)value;
            EStringToBoolConverterOption option = (EStringToBoolConverterOption)parameter;

            switch (option)
            {
                case EStringToBoolConverterOption.YesNo:

                    if (boolValue)
                        return "YES";
                    else
                        return "NO";
                
                case EStringToBoolConverterOption.RequiredNull:

                    if (boolValue)
                        return "REQUIRED";
                    else
                        return DBNull.Value;
                    
            }
        }

        #endregion
    }

    public enum EStringToBoolConverterOption
    {
        YesNo,
        RequiredNull
    }
}
