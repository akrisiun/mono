using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mono.Entity
{
    public static class SqlAttributes
    { 
        public static string FieldAttribute<T>(object value) where T : IFieldAttribute
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            IFieldAttribute[] attrs = fi.GetCustomAttributes(typeof(IFieldAttribute), false) as IFieldAttribute[];
            if (attrs.Length > 0)
            {
                foreach (IFieldAttribute attr in attrs)
                    if (attr.GetType().Equals(typeof(T)))
                    {
                        output = attr.Value == null ? null : attr.Value.ToString();
                        break;
                    }
            }
            return output;
        }
    }

    public interface IFieldAttribute
    {
        object Value { get; }
    }


   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
   public class FieldCaption : Attribute, IFieldAttribute
    {
        private string _value;

        public FieldCaption(string caption)
        {
            _value = caption;
        }

        object IFieldAttribute.Value { get { return _value; } }
        public string Value
        {
            get { return _value; }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class FieldLength : Attribute, IFieldAttribute
    {
        private int _value;

        public FieldLength(int length)
        {
            _value = length;
        }

        object IFieldAttribute.Value { get { return _value; } }
        public int Length
        {
            get { return _value; }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class CaptionWidth : Attribute, IFieldAttribute
    {
        private int _value;

        public CaptionWidth(int length)
        {
            _value = length;
        }

        object IFieldAttribute.Value { get { return _value; } }
        public int Length
        {
            get { return _value; }
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MaxLengthAttribute : System.Attribute, IFieldAttribute // ValidationAttribute
    {
        private const int MaxAllowableLength = -1;

        public int Length { get; private set; }

        public MaxLengthAttribute(int length = -1)
        {
            this.Length = length;
        }

        object IFieldAttribute.Value { get { return Length; } }

        #region Validation 
        public string ErrorMessageString { get; private set; }

        public bool IsValid(object value)
        {
            this.EnsureLegalLengths();
            if (value == null)
                return true;
            string str = value as string;
            int num = str == null ? ((Array)value).Length : str.Length;
            if (-1 != this.Length)
                return num <= this.Length;
            else
                return true;
        }

        public string FormatErrorMessage(string name)
        {
            return string.Format((IFormatProvider)CultureInfo.CurrentCulture, this.ErrorMessageString, new object[2]
              {
                (object) name,
                (object) this.Length
              });
        }

        private void EnsureLegalLengths()
        {
            if (this.Length == 0 || this.Length < -1)
                throw new InvalidOperationException(string.Format("MaxLengthAttribute_InvalidMaxLength"));
        }
        #endregion

    }
}
