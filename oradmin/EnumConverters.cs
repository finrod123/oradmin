using System;
using System.Windows.Data;
using System.ComponentModel;

namespace oradmin
{
    [ValueConversion(typeof(EDbaPrivileges), typeof(string))]
    public class DbaPrivilegesEnumConverter : IValueConverter
    {
        public DbaPrivilegesEnumConverter() { }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(EDbaPrivileges), value))
                return value.ToString();
            
            switch ((EDbaPrivileges)value)
            {
                case EDbaPrivileges.Normal:
                    return "Normální";
                case EDbaPrivileges.SYSDBA:
                    return "SYSDBA";
                case EDbaPrivileges.SYSOPER:
                    return "SYSOPER";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.Parse(typeof(EDbaPrivileges), value.ToString());
        }

        #endregion
    }

    [ValueConversion(typeof(ENamingMethod),typeof(string))]
    public class ENamingMethodEnumConverter : IValueConverter
    {

        public ENamingMethodEnumConverter() { }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(ENamingMethod), value))
                return value.ToString();

            switch ((ENamingMethod)value)
            {
                case ENamingMethod.ConnectDesctiptor:
                    return "Přímé zadání údajů";
                case ENamingMethod.TnsNaming:
                    return "TNS identifikátor";
                default:
                    return value.ToString();
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.Parse(typeof(ENamingMethod), value.ToString());
        }

        #endregion
    }
}
