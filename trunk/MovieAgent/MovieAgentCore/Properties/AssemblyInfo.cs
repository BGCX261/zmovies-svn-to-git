﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ScriptCoreLib;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MovieAgentCore")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MovieAgentCore")]
[assembly: AssemblyCopyright("Copyright ©  2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2ae7dacb-b0c1-4ae5-aa8a-1650ee63afcc")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly:
	Script,

	ScriptTypeFilter(ScriptType.JavaScript, typeof(global::MovieAgent.Shared.SharedExtensions)),
	ScriptTypeFilter(ScriptType.ActionScript, typeof(global::MovieAgent.Shared.SharedExtensions)),

	ScriptTypeFilter(ScriptType.PHP, typeof(global::MovieAgent.Server.Library.BasicWebCrawler)),
	ScriptTypeFilter(ScriptType.PHP, typeof(global::MovieAgent.Server.Services.BasicFileNameParser)),
	ScriptTypeFilter(ScriptType.PHP, typeof(global::MovieAgent.Shared.SharedExtensions)),
]