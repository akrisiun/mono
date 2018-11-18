//
// System.Web.UI.WebControls.ProfileProviderCollection.cs
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
using System.Collections;
using System.Configuration;
using System.Configuration.Provider;

namespace System.Web.Profile
{
    public interface ICollection2 : ICollection
    {
        void Remove(string name);
        void Clear();
        void CopyTo(ProfileProvider[] array, int index);
        void SetReadOnly();
        ProfileProvider this[string name] { get; }
    }

	public // ANKR: sealed 
        class ProfileProviderCollection : SettingsProviderCollection, ICollection2, IEnumerable
	{
		public ProfileProviderCollection ()
		{
            readOnly = true;
            lookup = new Hashtable();
            values = new ArrayList();
		}

		public 
            // ANRK: override 
            void AddProvider(ProviderBase provider)
		{
            // base.Add (provider);
            values.Add(provider);
		}

        // public new ProfileProvider this[string name] {
		public new ProfileProvider this[string name] {
			get {
                return Value(name);
				// return (ProfileProvider) base [name];
			}
		}

        void ICollection2.Clear()
        {
            if (readOnly)
                throw new NotSupportedException();
            values.Clear();
            lookup.Clear();
        }

        void ICollection2.CopyTo(ProfileProvider[] array, int index)
        {
            values.CopyTo(array, index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            values.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        void ICollection2.Remove(string name)
        {
            if (readOnly)
                throw new NotSupportedException();

            object position = lookup[name];

            if (position == null || !(position is int))
                throw new ArgumentException();

            int pos = (int)position;
            if (pos >= values.Count)
                throw new ArgumentException();

            values.RemoveAt(pos);
            lookup.Remove(name);

            ArrayList changed = new ArrayList();
            foreach (DictionaryEntry de in lookup) {
                if ((int)de.Value <= pos)
                    continue;
                changed.Add(de.Key);
            }

            foreach (string key in changed)
                lookup[key] = (int)lookup[key] - 1;
        }

        void ICollection2.SetReadOnly()
        {
            readOnly = true;
        }

        int ICollection.Count { get { return values.Count; } }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return this; } }

        ProfileProvider ICollection2.this[string name] { get => Value(name); }

        public ProfileProvider Value(string name) {
            object pos = lookup[name];
            if (pos == null)
                return null;

            return values[(int)pos] as ProfileProvider;
        }

        Hashtable lookup;
        bool readOnly;
        ArrayList values;
	}
	
}

