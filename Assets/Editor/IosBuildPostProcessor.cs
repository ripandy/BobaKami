#if UNITY_IOS
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace BobaKami.EditorTools
{
    /// <summary>
    /// Patches the exported Xcode project's Info.plist after every iOS build.
    /// <para>
    /// This exists because the export must be a <b>Replace</b> build, which regenerates the
    /// pbxproj and Info.plist from scratch — any change made by hand in Xcode is lost on the next
    /// export. Anything that has to survive belongs here.
    /// </para>
    /// </summary>
    public class IosBuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;

            var plistPath = report.summary.outputPath + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Declares that the app uses no non-exempt encryption. Without it, App Store Connect
            // asks the export-compliance question on every single upload. BobaKami ships no
            // cryptography of its own and makes no network calls at all.
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            plist.WriteToFile(plistPath);
        }
    }
}
#endif
