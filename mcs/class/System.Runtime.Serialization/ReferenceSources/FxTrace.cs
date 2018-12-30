//
// bare-bones based implementation based on the references
// from the Microsoft reference source code to get things to build
//
using System.Runtime;
// using System.Runtime.Diagnostics;

namespace System.Runtime.Serialization 
{

    internal class Fx {
        internal class Tag {
            public class SecurityNote : Attribute
            {
                public SecurityNote() { // string critical, string safe) {
                    //Critical = critical;
                    //Safe = safe;
                }

                // (Critical
                public string Critical { get; set; }
                public string Safe { get; set; }

                public string Miscellaneous { get; set; }
            }
        }
    }


    internal static class FxTrace {
		//public static EtwDiagnosticTrace Trace {
		//	get {
		//		return Fx.Trace;
		//	}
		//}

		//public static bool ShouldTraceError = true;
		//public static bool ShouldTraceVerbose = true;

		//public static ExceptionTrace Exception {
		//	get {
		//		return new ExceptionTrace ("System.Runtime.Serialization", Trace);
		//	}
		//}

		public static bool IsEventEnabled (int index)
		{
			return false;
		}

		//public static void UpdateEventDefinitions (EventDescriptor [] ed, ushort [] events) {}
	}
}

