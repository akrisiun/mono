//
// System.Web.Configuration.CompilationSection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
//

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
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace System.Web.Configuration
{
	public sealed class CompilationSection : ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty compilersProp;
		static ConfigurationProperty tempDirectoryProp;
        static string debugProp; // ConfigurationProperty debugProp;
        static string defaultLanguage; //  ConfigurationProperty defaultLanguageProp;
        static string buildProviders; // ConfigurationProperty buildProvidersProp;

        static ConfigurationProperty strictProp;
		static ConfigurationProperty explicitProp;
		static ConfigurationProperty batchProp;
		static ConfigurationProperty batchTimeoutProp;
		static ConfigurationProperty maxBatchSizeProp;
		static ConfigurationProperty maxBatchGeneratedFileSizeProp;
		static ConfigurationProperty numRecompilesBeforeAppRestartProp;

        static ConfigurationProperty assembliesProp;
		static ConfigurationProperty assemblyPostProcessorTypeProp;
		static ConfigurationProperty expressionBuildersProp;
		static ConfigurationProperty urlLinePragmasProp;
		// static ConfigurationProperty codeSubDirectoriesProp;
		static ConfigurationProperty optimizeCompilationsProp;
		static ConfigurationProperty targetFrameworkProp;

        static CompilationSection()
        {
        }

        public static Exception LoadError { get; set; }
        public static void Load()
        {
            defaultLanguage = "cs";
            buildProviders = "buildProviders"; // ConfigurationProperty buildProvidersProp;
            properties = new ConfigurationPropertyCollection();
            debugProp = "debug";
            baseData = new Dictionary<string, object>();

            try
            {
                assembliesProp = new ConfigurationProperty("assemblies", typeof(AssemblyCollection), null,
                                        null, PropertyHelper.DefaultValidator,
                                        ConfigurationPropertyOptions.None);
                assemblyPostProcessorTypeProp = new ConfigurationProperty("assemblyPostProcessorType", typeof(string), "");
                batchProp = new ConfigurationProperty("batch", typeof(bool), true);
                //buildProvidersProp = new ConfigurationProperty("buildProviders", typeof(BuildProviderCollection), null,
                //                        null, PropertyHelper.DefaultValidator,
                //                        ConfigurationPropertyOptions.None);

                batchTimeoutProp = new ConfigurationProperty("batchTimeout", typeof(TimeSpan), new TimeSpan(0, 15, 0),
                                          PropertyHelper.TimeSpanSecondsOrInfiniteConverter,
                                          PropertyHelper.PositiveTimeSpanValidator,
                                          ConfigurationPropertyOptions.None);
                //codeSubDirectoriesProp = new ConfigurationProperty("codeSubDirectories", typeof(CodeSubDirectoriesCollection), null,
                //                            null, PropertyHelper.DefaultValidator,
                //                            ConfigurationPropertyOptions.None);
                compilersProp = new ConfigurationProperty("compilers", typeof(CompilerCollection), null,
                                       null, PropertyHelper.DefaultValidator,
                                       ConfigurationPropertyOptions.None);
                // debugProp = new ConfigurationProperty("debug", typeof(bool), false);
                // defaultLanguageProp = new ConfigurationProperty("defaultLanguage", typeof(string), "vb");
                expressionBuildersProp = new ConfigurationProperty("expressionBuilders", typeof(ExpressionBuilderCollection), null,
                                            null, PropertyHelper.DefaultValidator,
                                            ConfigurationPropertyOptions.None);
                explicitProp = new ConfigurationProperty("explicit", typeof(bool), true);
                maxBatchSizeProp = new ConfigurationProperty("maxBatchSize", typeof(int), 1000);
                maxBatchGeneratedFileSizeProp = new ConfigurationProperty("maxBatchGeneratedFileSize", typeof(int), 3000);
                numRecompilesBeforeAppRestartProp = new ConfigurationProperty("numRecompilesBeforeAppRestart", typeof(int), 15);
                strictProp = new ConfigurationProperty("strict", typeof(bool), false);
                tempDirectoryProp = new ConfigurationProperty("tempDirectory", typeof(string), "");
                urlLinePragmasProp = new ConfigurationProperty("urlLinePragmas", typeof(bool), false);

                // This is a 4.0 property but it is also supported in 3.5 with
                // this hotfix: http://support.microsoft.com/kb/961884
                optimizeCompilationsProp = new ConfigurationProperty("optimizeCompilations", typeof(bool), false);

                // Mono ignores this as there is no way to switch the runtime version
                // dynamically while application is running
                targetFrameworkProp = new ConfigurationProperty("targetFramework", typeof(string), null);

            }
            catch (Exception ex) {
                LoadError = ex;
            }


            try {

                baseData["buildProviders"] = new BuildProviderCollection();
                baseData["expressionBuilders"] = new ExpressionBuilderCollection();
                AssemblyCollection a = new AssemblyCollection();
                baseData["assemblies"] = a;

                baseData["codeSubDirectories"] = new CodeSubDirectoriesCollection();

                // AssemblyInfo a1 = null;
                // a.Add(a1); // TODO typeof(System.Action).Assembly.Location)

                properties.Add (assembliesProp);

			    properties.Add (assemblyPostProcessorTypeProp);
			    properties.Add (batchProp);
			    // properties.Add (buildProvidersProp);
			    properties.Add (batchTimeoutProp);
			    // properties.Add (codeSubDirectoriesProp);
			    properties.Add (compilersProp);
			    // properties.Add (debugProp);
			    // properties.Add (defaultLanguageProp);
			    properties.Add (expressionBuildersProp);
			    properties.Add (explicitProp);
			    properties.Add (maxBatchSizeProp);
			    properties.Add (maxBatchGeneratedFileSizeProp);
			    properties.Add (numRecompilesBeforeAppRestartProp);
			    properties.Add (strictProp);
			    properties.Add (tempDirectoryProp);
			    properties.Add (urlLinePragmasProp);
			    properties.Add (optimizeCompilationsProp);
			    properties.Add (targetFrameworkProp);
            }
            catch (Exception ex)
            {
                LoadError = ex;
            }
        }

        public static Dictionary<string, object> baseData;

        public CompilationSection ()
		{
            if (properties == null)
                Load();

            Debug = true;
        }

        protected override void PostDeserialize ()
		{
			base.PostDeserialize ();
		}

		[MonoTODO ("why override this?")]
		public new // internal override
			object GetRuntimeObject ()
		{
			return this;
		}

		[ConfigurationProperty ("assemblies")]
		public AssemblyCollection Assemblies {
			get { return (AssemblyCollection)baseData["assemblies"]; }
		}

		[ConfigurationProperty ("assemblyPostProcessorType", DefaultValue = "")]
		public string AssemblyPostProcessorType {
			get { return (string) baseData["assemblyPostProcessorType"]; }
			set { baseData["assemblyPostProcessorType"] = value; }

        }

		[ConfigurationProperty ("batch", DefaultValue = "True")]
		public bool Batch {
			get { return (bool)baseData["batch"]; }
            set { baseData["batch"] = value; }
        }

		[TypeConverter (typeof (TimeSpanSecondsOrInfiniteConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00")]
		[ConfigurationProperty ("batchTimeout", DefaultValue = "00:15:00")]
		public TimeSpan BatchTimeout {
			get { return (TimeSpan)baseData["batchTimeout"]; }

            set { baseData["batchTimeout"] = value; }

        }

		[ConfigurationProperty ("buildProviders")]
		public BuildProviderCollection BuildProviders {
			get { return (BuildProviderCollection)baseData["buildProviders"];
            }

        }


        [ConfigurationProperty("expressionBuilders")]
        public ExpressionBuilderCollection ExpressionBuilders {
            get { return (ExpressionBuilderCollection)baseData["expressionBuilders"]; }

        }

        [ConfigurationProperty ("codeSubDirectories")]
		public CodeSubDirectoriesCollection CodeSubDirectories {
			get { return (CodeSubDirectoriesCollection)baseData["codeSubDirectories"]; }

        }

		[ConfigurationProperty ("compilers")]
		public CompilerCollection Compilers {
			get { return (CompilerCollection)baseData["compilers"]; }

        }

		[ConfigurationProperty ("debug", DefaultValue = "False")]
		public bool Debug {
			get { return (bool)baseData["debug"]; }

            set { baseData["debug"] = value; }

        }

		[ConfigurationProperty ("defaultLanguage", DefaultValue = "cs")]
		public string DefaultLanguage {
			get { return (string)baseData[defaultLanguage]; }
			set { baseData[defaultLanguage] = value; }
		}

		[ConfigurationProperty ("explicit", DefaultValue = "True")]
		public bool Explicit {
			get { return (bool?)baseData["explicit"] ?? true; }
			set { baseData["explicit"] = value; }

        }


		[ConfigurationProperty ("maxBatchGeneratedFileSize", DefaultValue = "1000")]
		public int MaxBatchGeneratedFileSize {
			get { return (int?)baseData["maxBatchGeneratedFileSize"] ?? 1000; }

            set { baseData["maxBatchGeneratedFileSize"] = value; }

        }

		[ConfigurationProperty ("maxBatchSize", DefaultValue = "1000")]
		public int MaxBatchSize {
			get { return (int?)baseData["maxBatchSize"] ?? 1000; }

            set { baseData["maxBatchSize"] = value; }

        }

		[ConfigurationProperty ("numRecompilesBeforeAppRestart", DefaultValue = "15")]
		public int NumRecompilesBeforeAppRestart {
			get { return (int?)baseData["numRecompilesBeforeAppRestart"] ?? 15; }

            set { baseData["numRecompilesBeforeAppRestart"] = value; }

        }

		[ConfigurationProperty ("optimizeCompilations", DefaultValue = "False")]
		public bool OptimizeCompilations {
			get { return (baseData["optimizeCompilations"] as bool? ?? (bool?)false).Value; }

            set { baseData["optimizeCompilations"] = value; }

        }
		
		[ConfigurationProperty ("strict", DefaultValue = "False")]
		public bool Strict {
			get { return (baseData["strict"] as bool? ?? (bool?)false).Value; }

            set { baseData["strict"] = value; }

        }

		[ConfigurationProperty ("targetFramework", DefaultValue = null)]
		public string TargetFramework {
			get { return baseData["targetFramework"] as string ?? "net462"; }

            set { baseData["targetFramework"] = value; }

        }
		
		[ConfigurationProperty ("tempDirectory", DefaultValue = "")]
		public string TempDirectory {
			get { return baseData["tempDirectory"] as string ?? Path.GetTempPath(); }
			set { baseData["tempDirectory"] = value; }

        }

		[ConfigurationProperty ("urlLinePragmas", DefaultValue = "False")]
		public bool UrlLinePragmas {
			get { return (bool)baseData["urlLinePragmas"]; }

            set { baseData["urlLinePragmas"] = value; }

        }

		public new // internal override
			 ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

