using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace System.Linq.Dynamic.Core
{
    /// <summary>
    /// Provides a base class for dynamic objects.
    /// 
    /// In addition to the methods defined here, the following items are added using reflection:
    /// - default constructor
    /// - constructor with all the properties as parameters (if not linq-to-entities)
    /// - all properties (also with getter and setters)
    /// - ToString() method
    /// - Equals() method
    /// - GetHashCode() method
    /// </summary>
    public abstract class DynamicClass : DynamicObject
    {
        private Dictionary<string, object> _propertiesDictionary;

        private Dictionary<string, object> Properties
        {
            get
            {
                if (_propertiesDictionary == null)
                {
                    _propertiesDictionary = new Dictionary<string, object>();

                    foreach (PropertyInfo pi in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        int parameters = pi.GetIndexParameters().Length;
                        if (parameters > 0)
                        {
                            // The property is an indexer, skip this.
                            continue;
                        }

                        _propertiesDictionary.Add(pi.Name, pi.GetValue(this, null));
                    }
                }

                return _propertiesDictionary;
            }
        }

        /// <summary>
        /// Gets the dynamic property by name.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>T</returns>
        public T GetDynamicPropertyValue<T>(string propertyName)
        {
            var type = GetType();
            var propInfo = type.GetProperty(propertyName);

            return (T)propInfo.GetValue(this, null);
        }

        /// <summary>
        /// Gets the dynamic property value by name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>value</returns>
        public object GetDynamicPropertyValue(string propertyName)
        {
            return GetDynamicPropertyValue<object>(propertyName);
        }

        /// <summary>
        /// Sets the dynamic property value by name.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public void SetDynamicPropertyValue<T>(string propertyName, T value)
        {
            var type = GetType();
            var propInfo = type.GetProperty(propertyName);

            propInfo.SetValue(this, value, null);
        }

        /// <summary>
        /// Sets the dynamic property value by name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public void SetDynamicPropertyValue(string propertyName, object value)
        {
            SetDynamicPropertyValue<object>(propertyName, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="object"/> with the specified name.
        /// </summary>
        /// <value>The <see cref="object"/>.</value>
        /// <param name="name">The name.</param>
        /// <returns>Value from the property.</returns>
        public object this[string name]
        {
            get
            {
                if (Properties.TryGetValue(name, out object result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                if (Properties.ContainsKey(name))
                {
                    Properties[name] = value;
                }
                else
                {
                    Properties.Add(name, value);
                }
            }
        }
         
    }
}
