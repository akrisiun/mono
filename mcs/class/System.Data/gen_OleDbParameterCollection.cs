//------------------------------------------------------------------------------
// <copyright file="DbParameterCollectionBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public sealed partial class OleDbParameterCollection : DbParameterCollection {
        private List<OleDbParameter> _items; // the collection of parameters

        override public int Count {
            get {
                // NOTE: we don't construct the list just to get the count.
                return ((null != _items) ? _items.Count : 0);
            }
        }

        private List<OleDbParameter> InnerList {
            get {
                List<OleDbParameter> items = _items;

                if (null == items) {
                    items = new List<OleDbParameter>();
                    _items = items;
                }
                return items;
            }
        }

        override public bool IsFixedSize {
            get {
                return ((System.Collections.IList)InnerList).IsFixedSize;
            }
        }

        override public bool IsReadOnly {
            get {
                return ((System.Collections.IList)InnerList).IsReadOnly;
            }
        }

        override public bool IsSynchronized {
            get {
                return ((System.Collections.ICollection)InnerList).IsSynchronized;
            }
        }

        override public object SyncRoot {
            get {
                return ((System.Collections.ICollection)InnerList).SyncRoot;
            }
        }

        [
        EditorBrowsableAttribute(EditorBrowsableState.Never)
        ]
        override public int Add(object value) {
            OnChange();  // fire event before value is validated
            ValidateType(value);
            Validate(-1, value);
            InnerList.Add((OleDbParameter)value);
            return Count-1;
        }

        override public void AddRange(System.Array values) {
            OnChange();  // fire event before value is validated
            if (null == values) {
                throw ADP.ArgumentNull("values");
            }
            foreach(object value in values) {
                ValidateType(value);
            }
            foreach(OleDbParameter value in values) {
                Validate(-1, value);
                InnerList.Add((OleDbParameter)value);
            }
        }

        private int CheckName(string parameterName) {
            int index = IndexOf(parameterName);
            if (index < 0) {
                throw ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            return index;
        }

        override public void Clear() {
            OnChange();  // fire event before value is validated
            List<OleDbParameter> items = InnerList;

            if (null != items) {
                foreach(OleDbParameter item in items) {
                    item.ResetParent();
                }
                items.Clear();
            }
        }

        override public bool Contains(object value) {
            return (-1 != IndexOf(value));
        }

        override public void CopyTo(Array array, int index) {
            ((System.Collections.ICollection)InnerList).CopyTo(array, index);
        }

        override public System.Collections.IEnumerator GetEnumerator() {
            return ((System.Collections.ICollection)InnerList).GetEnumerator();
        }

        override protected DbParameter GetParameter(int index) {
            RangeCheck(index);
            return InnerList[index];
        }

        override protected DbParameter GetParameter(string parameterName) {
            int index = IndexOf(parameterName);
            if (index < 0) {
                throw ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            return InnerList[index];
        }

        private static int IndexOf(System.Collections.IEnumerable items, string parameterName) {
            if (null != items) {
                int i = 0;
                // first case, kana, width sensitive search
                foreach(OleDbParameter parameter in items) {
                    if (0 == ADP.SrcCompare(parameterName, parameter.ParameterName)) {
                        return i;
                    }
                    ++i;
                }
                i = 0;
                // then insensitive search
                foreach(OleDbParameter parameter in items) {
                    if (0 == ADP.DstCompare(parameterName, parameter.ParameterName)) {
                        return i;
                    }
                    ++i;
                }
            }
            return -1;
        }

        override public int IndexOf(string parameterName) {
            return IndexOf(InnerList, parameterName);
        }

        override public int IndexOf(object value) {
            if (null != value) {
                ValidateType(value);

                List<OleDbParameter> items = InnerList;

                if (null != items) {
                    int count = items.Count;

                    for (int i = 0; i < count; i++) {
                        if (value == items[i]) {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        override public void Insert(int index, object value) {
            OnChange();  // fire event before value is validated
            ValidateType(value);
            Validate(-1, (OleDbParameter)value);
            InnerList.Insert(index, (OleDbParameter)value);
        }

        private void RangeCheck(int index) {
            if ((index < 0) || (Count <= index)) {
                throw ADP.ParametersMappingIndex(index, this);
            }
        }

        override public void Remove(object value) {
            OnChange();  // fire event before value is validated
            ValidateType(value);
            int index = IndexOf(value);
            if (-1 != index) {
                RemoveIndex(index);
            }
            else if (this != ((OleDbParameter)value).CompareExchangeParent(null, this)) {
                throw ADP.CollectionRemoveInvalidObject(ItemType, this);
            }
        }

        override public void RemoveAt(int index) {
            OnChange();  // fire event before value is validated
            RangeCheck(index);
            RemoveIndex(index);
        }

        override public void RemoveAt(string parameterName) {
            OnChange();  // fire event before value is validated
            int index = CheckName(parameterName);
            RemoveIndex(index);
        }

        private void RemoveIndex(int index) {
            List<OleDbParameter> items = InnerList;
            Debug.Assert((null != items) && (0 <= index) && (index < Count), "RemoveIndex, invalid");
            OleDbParameter item = items[index];
            items.RemoveAt(index);
            item.ResetParent();
        }

        private void Replace(int index, object newValue) {
            List<OleDbParameter> items = InnerList;
            Debug.Assert((null != items) && (0 <= index) && (index < Count), "Replace Index invalid");
            ValidateType(newValue);
            Validate(index, newValue);
            OleDbParameter item = items[index];
            items[index] = (OleDbParameter)newValue;
            item.ResetParent();
        }

        override protected void SetParameter(int index, DbParameter value) {
            OnChange();  // fire event before value is validated
            RangeCheck(index);
            Replace(index, value);
        }

        override protected void SetParameter(string parameterName, DbParameter value) {
            OnChange();  // fire event before value is validated
            int index = IndexOf(parameterName);
            if (index < 0) {
                throw ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            Replace(index, value);
        }

        private void Validate(int index, object value) {
            if (null == value) {
                throw ADP.ParameterNull("value", this, ItemType);
            }
            // Validate assigns the parent - remove clears the parent
            object parent = ((OleDbParameter)value).CompareExchangeParent(this, null);
            if (null != parent) {
                if (this != parent) {
                    throw ADP.ParametersIsNotParent(ItemType, this);
                }
                if (index != IndexOf(value)) {
                    throw ADP.ParametersIsParent(ItemType, this);
                }
            }
            // generate a ParameterName
            String name = ((OleDbParameter)value).ParameterName;
            if (0 == name.Length) {
                index = 1;
                do {
                    name = ADP.Parameter + index.ToString(CultureInfo.CurrentCulture);
                    index++;
                } while (-1 != IndexOf(name));
                ((OleDbParameter)value).ParameterName = name;
            }
        }

        private void ValidateType(object value) {
            if (null == value) {
                throw ADP.ParameterNull("value", this, ItemType);
            }
            else if (!ItemType.IsInstanceOfType(value)) {
                throw ADP.InvalidParameterType(this, ItemType, value);
            }
        }

    };
}

