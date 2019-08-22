using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Compressibility")]
[assembly: AssemblyDescription("Fiddler4 Extension to check compressibility of resources.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Compressibility")]
[assembly: AssemblyCopyright("Copyright ©2019 Eric Lawrence")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: System.Resources.NeutralResourcesLanguage("en-US")]
[assembly: Fiddler.RequiredVersion("4.6.2.0")]
[assembly: AssemblyVersion("1.1.0.0")]


/* -= PREFERENCES =-
 FiddlerApplication.Prefs.GetBoolPref("extensions.compressibility.AlwaysOn", false))
 FiddlerApplication.Prefs.GetInt32Pref("extensions.compressibility.Zopfli.MaxSize", (int)MAX_ZOPFLI_SIZE);
 FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.Zopfli.Args", String.Empty),
 FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.Brotli.Args", String.Empty));
 FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.WebPLossless.Args", "-m 6")
 FiddlerApplication.Prefs.GetStringPref("extensions.compressibility.WebPLossy.Args", "-m 6");
 FiddlerApplication.Prefs.GetBoolPref("extensions.compressibility.SkipAudioVideo", true))
*/

// 1.1.0.0
// Update code to actually look for tools in per-user location (doh!)
// Show warning in Log and footer if a tool is missing
// Remove PNGDistill and Brotli from installer now that Fiddler carries these.

// 1.0.4.0 
// Update installer to install to per-user location

// 1.0.2.0 [Ship 2/19/2016]
// Add CTRL+C to copy rows
// Add "Server"/current to Summation Info view
// Rev Brotli to 0.3.0.2

// 1.0.1.0
// Add webp to Imageview MenuExt tools
// Skip audio/video by default (extensions.compressibility.SkipAudioVideo)
// Add preference for Zopfli.MaxSize

// 1.0.0.1 [2016-01-27]
// Initial release
