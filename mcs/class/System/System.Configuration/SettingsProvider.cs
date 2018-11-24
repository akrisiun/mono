//
// System.Web.UI.WebControls.SettingsProvider.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Specialized;

#if !CONFIGURATION_DEP

namespace System.Configuration.Provider.Base
{
    // internal 
    public abstract class ProviderBase
	{
		protected bool alreadyInitialized;
		
		protected ProviderBase ()
		{
		}
		
		public virtual void Initialize (string name, NameValueCollection config)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("Provider name cannot be null or empty.", "name");
			if (alreadyInitialized)
				throw new InvalidOperationException ("This provider instance has already been initialized.");
			alreadyInitialized = true;
			
			_name = name;

			if (config != null) {
				_description = config ["description"];
				config.Remove ("description");
			}
			if (String.IsNullOrEmpty (_description))
				_description = _name;
		}
		
		public virtual string Name { 
			get { return _name; }
            set { _name = value; }
		}

		public virtual string Description {
			get { return _description; }
            set { _description = value; }
		}

		string _description;
		string _name;
	}
}
#endif

namespace System.Configuration
{
#if CONFIGURATION_DEP
    using System.Configuration.Provider;
#else
    using System.Configuration.Provider.Base;
#endif

    public abstract class SettingsProvider
// #if (CONFIGURATION_DEP)
		: ProviderBase
// #endif
    {
        protected SettingsProvider()
        {
        }

        public virtual void DoInitialize(object parm1, object parm2) {}

		public abstract SettingsPropertyValueCollection GetPropertyValues (SettingsContext context,
										   SettingsPropertyCollection collection);

		public abstract void SetPropertyValues (SettingsContext context,
							SettingsPropertyValueCollection collection);

		public abstract string ApplicationName { get; set; }
	}

}

