using Microsoft.CSharp;
using Mono.Entity;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mono.Reflection
{
    // http://stackoverflow.com/questions/10914484/use-dlr-to-run-code-generated-with-compileassemblyfromsource

    public interface ICalc : ILastError
    {
        // public Exception LastError { get; set; }
        object Calc();
    }

    public class DynCompile : ILastError
    {
        public class CalcEmpty : ICalc
        {
            public Exception LastError { get; set; }
            public object Calc() { return null; }
        }

        public static ICalc GetCalc(string csCode)
        {
            using (Microsoft.CSharp.CSharpCodeProvider csProvider = new Microsoft.CSharp.CSharpCodeProvider())
            {
                var prm = new System.CodeDom.Compiler.CompilerParameters();
                prm.GenerateInMemory = true;
                prm.GenerateExecutable = false;
#if NET451
                prm.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
#endif

                counter++;
                // Implement the interface in the dynamic code
                var res = csProvider.CompileAssemblyFromSource(prm,
                        String.Format(@"public class CompiledCalc{0} : ICalc { public Exception LastError { get; set; }
                            public object Calc() { {1} }}", counter, csCode));
                var type = res.CompiledAssembly.GetType(string.Format("CompiledCalc{0}", counter));

                ICalc obj = null;
                try
                {
                    obj = Activator.CreateInstance(type) as ICalc;
                }
                catch (Exception ex)
                {
                    obj = obj ?? new CalcEmpty();
                    obj.LastError = ex;
                }
                return obj;
            }
        }

        static int counter = 0;
        public Exception LastError { get; set; }
        public Assembly Assembly { get; protected set; }
        public string ClassName { get; protected set; }

        public static string DynClass { get { return string.Format("DynClass{0}", counter); } }
        public static string GenCalc(string code)
        {
            var sourceCode =
            @"class DynClass{0} {
                public static object Calc() {
                  {1}
              } }";
            counter++;
            return string.Format(sourceCode, counter, code);
        }

        public static DynCompile CompileCSCode(string csCode, string className, string[] refAssemblies = null)
        {
            if (refAssemblies == null)
                refAssemblies = new string[] { "System.dll" };

            CodeDomProvider csharpCodeProvider = new CSharpCodeProvider();
            var cp = new CompilerParameters();
            foreach (string asmName in refAssemblies)
                cp.ReferencedAssemblies.Add(asmName);

            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            var dyn = new DynCompile() { ClassName = className };
            try
            {
                CompilerResults cr = csharpCodeProvider.CompileAssemblyFromSource(cp, csCode);

                dyn.Assembly = cr.CompiledAssembly;
            }
            catch (Exception ex) { dyn.LastError = ex; }
            return dyn;
        }

        public object Calculate(string method = "Calc")
        {
            var type = Assembly.GetType(ClassName);
            object result = type.InvokeMember(method, BindingFlags.InvokeMethod, null, Assembly, args: null);
            return result;
        }

        //static void Test() {
        //    var dyn = DynCompile.CompileCSCode(
        //              DynCompile.GenCalc(@"return ""Hello cs world"";"), DynCompile.DynClass, null);
        //    Console.WriteLine(dyn.Calculate("Calc"));
        //    var dyn2 = DynCompile.GetCalc(@"return ""Hello Calc object"";");
        //}
    }
}