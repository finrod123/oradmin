using System;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Generic;

namespace oradmin
{
    #region Value converters

    /// <summary>
    /// Base class for all enum converters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumValueConverter<T> : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(T), value))
                return value.ToString();

            EnumToStringConverter<T> converter = EnumConverterMapper.GetConverter(typeof(T)) as EnumToStringConverter<T>;
            return converter.EnumValueToString((T)value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.Parse(typeof(T), value.ToString());
        }

        #endregion
    }

    #region Custom value converters
    [ValueConversion(typeof(EDbaPrivileges), typeof(string))]
    public class EDbaPrivilegesEnumConverter : EnumValueConverter<EDbaPrivileges>
    { }

    [ValueConversion(typeof(ENamingMethod), typeof(string))]
    public class ENamingMethodEnumConverter : EnumValueConverter<ENamingMethod>
    { }

    [ValueConversion(typeof(EServerType), typeof(string))]
    public class EServerTypeEnumConverter : EnumValueConverter<EServerType>
    { }

    #endregion

    #endregion
    /// <summary>
    /// Interface for enum to string converters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface EnumToStringConverter<T>
    {
        string EnumValueToString(T value, object parameter);
    }

    #region Enum to string converters
    public class EDbaPrivilegesToStringConverter : EnumToStringConverter<EDbaPrivileges>
    {
        public string EnumValueToString(EDbaPrivileges value, object parameter)
        {
            switch (value)
            {
                case EDbaPrivileges.Normal:
                    return "Normální";
                case EDbaPrivileges.SYSDBA:
                    return "SYSDBA";
                case EDbaPrivileges.SYSOPER:
                    return "SYSOPER";
                default:
                    return value.ToString();
            }
        }
    }
    public class EServerTypeToStringConverter : EnumToStringConverter<EServerType>
    {
        #region EnumToStringConverter<EServerType> Members

        public string EnumValueToString(EServerType value, object parameter)
        {
            switch (value)
            {
                case EServerType.Dedicated:
                    return "Vyhrazaný server";
                case EServerType.Shared:
                    return "Sdílený server";
                case EServerType.Pooled:
                    return "Pooled server";
                default:
                    return value.ToString();
            }
        }

        #endregion
    }
    public class ENamingMethodToStringConverter : EnumToStringConverter<ENamingMethod>
    {

        #region EnumToStringConverter<ENamingMethod> Members

        public string EnumValueToString(ENamingMethod value, object parameter)
        {
            switch (value)
            {
                case ENamingMethod.ConnectDescriptor:
                    return "Přímé zadání údajů";
                case ENamingMethod.TnsNaming:
                    return "TNS identifikátor";
                default:
                    return value.ToString();
            }
        }

        #endregion
    }
    #endregion


    /// <summary>
    /// Class to store type binding to enum converters
    /// </summary>
    public static class EnumConverterMapper
    {
        private static Dictionary<Type, object> specs;

        static EnumConverterMapper()
        {
            specs = new Dictionary<Type, object>();

            specs.Add(typeof(EServerType), new EServerTypeToStringConverter());
            specs.Add(typeof(EDbaPrivileges), new EDbaPrivilegesToStringConverter());
            specs.Add(typeof(ENamingMethod), new ENamingMethodToStringConverter());
        }

        public static object GetConverter(Type type)
        {
            return specs[type];
        }
    }
}
