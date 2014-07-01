using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.Common.Utils;

// Version Compatibility
// http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/18_Contribute/6_Plugins/Plugin_Related_Changes/1.6.0_to_1.7.0
[assembly: CompatibleVersion("1.7.0.0", "1.7.0.0")]

[assembly: UsesSubsystem("MP.SkinEngine")]
[assembly: UsesSubsystem("MP.Players.Video")]
[assembly: UsesSubsystem("MP.Players.Music")]
[assembly: UsesSubsystem("MP.Config")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("RadioTimePlugin")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("RadioTimePlugin")]
[assembly: AssemblyCopyright("Copyright © 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8e38b4bb-d183-4491-a879-2b0ec0d5d1e5")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.2.5.$WCREV$")]
[assembly: AssemblyFileVersion("1.2.5.$WCREV$")]
