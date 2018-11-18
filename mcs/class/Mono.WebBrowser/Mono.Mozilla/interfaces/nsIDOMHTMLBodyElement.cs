// THIS FILE AUTOMATICALLY GENERATED BY xpidl2cs.pl
// EDITING IS PROBABLY UNWISE
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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
// using System.Text;
#pragma warning disable CS0108

namespace Mono.Mozilla {

	[Guid ("a6cf908e-15b3-11d2-932e-00805f8add32")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface nsIDOMHTMLBodyElement : nsIDOMHTMLElement {
#region nsIDOMNode
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNodeName (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNodeValue (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setNodeValue ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNodeType ( out ushort ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getParentNode ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getChildNodes ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMNodeList ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getFirstChild ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getLastChild ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getPreviousSibling ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNextSibling ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAttributes ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMNamedNodeMap ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getOwnerDocument ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMDocument ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int insertBefore (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMNode newChild,
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMNode refChild,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int replaceChild (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMNode newChild,
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMNode oldChild,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int removeChild (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMNode oldChild,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int appendChild (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMNode newChild,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int hasChildNodes ( out bool ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int cloneNode (
				   bool deep,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int normalize ();

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int isSupported (
				   /*DOMString*/ HandleRef feature,
				   /*DOMString*/ HandleRef version, out bool ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNamespaceURI (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getPrefix (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setPrefix ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getLocalName (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int hasAttributes ( out bool ret);

#endregion

#region nsIDOMElement
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getTagName (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAttribute (
				   /*DOMString*/ HandleRef name,  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setAttribute (
				   /*DOMString*/ HandleRef name,
				   /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int removeAttribute (
				   /*DOMString*/ HandleRef name);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAttributeNode (
				   /*DOMString*/ HandleRef name,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMAttr ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setAttributeNode (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMAttr newAttr,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMAttr ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int removeAttributeNode (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMAttr oldAttr,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMAttr ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getElementsByTagName (
				   /*DOMString*/ HandleRef name,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMNodeList ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAttributeNS (
				   /*DOMString*/ HandleRef namespaceURI,
				   /*DOMString*/ HandleRef localName,  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setAttributeNS (
				   /*DOMString*/ HandleRef namespaceURI,
				   /*DOMString*/ HandleRef qualifiedName,
				   /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int removeAttributeNS (
				   /*DOMString*/ HandleRef namespaceURI,
				   /*DOMString*/ HandleRef localName);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAttributeNodeNS (
				   /*DOMString*/ HandleRef namespaceURI,
				   /*DOMString*/ HandleRef localName,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMAttr ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setAttributeNodeNS (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMAttr newAttr,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMAttr ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getElementsByTagNameNS (
				   /*DOMString*/ HandleRef namespaceURI,
				   /*DOMString*/ HandleRef localName,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMNodeList ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int hasAttribute (
				   /*DOMString*/ HandleRef name, out bool ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int hasAttributeNS (
				   /*DOMString*/ HandleRef namespaceURI,
				   /*DOMString*/ HandleRef localName, out bool ret);

#endregion

#region nsIDOMHTMLElement
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getId (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setId ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getTitle (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setTitle ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getLang (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setLang ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getDir (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setDir ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getClassName (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setClassName ( /*DOMString*/ HandleRef value);

#endregion

#region nsIDOMHTMLBodyElement
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getALink (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setALink ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getBackground (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setBackground ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getBgColor (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setBgColor ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getLink (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setLink ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getText (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setText ( /*DOMString*/ HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getVLink (  /*DOMString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setVLink ( /*DOMString*/ HandleRef value);

#endregion
	}


	internal class nsDOMHTMLBodyElement {
		public static nsIDOMHTMLBodyElement GetProxy (Mono.WebBrowser.IWebBrowser control, nsIDOMHTMLBodyElement obj)
		{
			object o = Base.GetProxyForObject (control, typeof(nsIDOMHTMLBodyElement).GUID, obj);
			return o as nsIDOMHTMLBodyElement;
		}
	}
}
