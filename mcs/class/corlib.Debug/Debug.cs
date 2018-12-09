//
// Debug.cs: Private corlib debug implememtation.
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2018  Microsoft Corporation
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

namespace System.Diagnostics
{
    using System.Diagnostics.Private;

    public static class DebugMono2
    {
        public static bool IsDebug { get; set; }

        public static void Break()
        {

            Debugger.Break();
        }
    }
}

namespace System.Diagnostics.Private
{
    
	//
	// The type is renamed to DebugInternal in the post processing to avoid conficts in IVT assemblies. The proper
	// solution is to have support for IVT for members
	//
	static class Debug2
	{
        /// <summary>
        /// System.Diagnostics.Security.Debug.IsDebug
        /// </summary>
        public static bool IsDebug {
            get => DebugMono2.IsDebug;
            set { DebugMono2.IsDebug = value; }
        }


		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition, string message)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition, string message, string detailMessage)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition, string message, string detailMessageFormat, params object[] args)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Fail (string message)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Fail (string message, string detailMessage)
		{
		}
	}
}
