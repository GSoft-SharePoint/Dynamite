﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("GSoft.Dynamite")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("GSoft.Dynamite")]
[assembly: AssemblyCopyright("Copyright © GSoft 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d377829a-3b91-4ac9-8569-694ee8f48fb7")]

// Keep AssemblyVersion info in each project so that the TeamCity assembly version patcher works properly
// A DLL used as third-party reference in a SharePoint context is easier to patch if its AssemblyVersion
// never changes (otherwise, runtime assembly resolution will break every time you redeploy and newly
// version-bumped assembly of the third-party).
[assembly: AssemblyVersion("15.0.0.0")]

// The AssemblyFileVersion is used to track the current version.
// The version 0.0.0.0 flags a developement machine-build artifact.
[assembly: AssemblyFileVersion("0.0.0.0")]
