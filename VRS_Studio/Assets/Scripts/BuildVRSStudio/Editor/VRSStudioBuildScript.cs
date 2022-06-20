#if !VRSSTUDIO_INTERNAL
using System.IO;
using UnityEditor;
using UnityEngine;


namespace VRSStudio.Build.Editor
{
    public class VRSStudioBuildScript
    {
        private const string VRSStudio_AutoRunAfterBuild = "VRS Studio/Auto Run after build";
        private const string VRSStudio_BuildVRSStudio32 = "VRS Studio/Build VRS Studio/32-bit";
        private const string VRSStudio_BuildVRSStudio64 = "VRS Studio/Build VRS Studio/64-bit";

        private static string originalCompanyName, originalProductName, originalPackageName, originalVersion;
        private static AndroidArchitecture originalAndroidArchitecture;
        private static ScriptingImplementation originalScriptingBackEnd;
        private static ManagedStrippingLevel originalManagedStrippingLevel;
        private static bool originalStripEngineCode;
        private static int originalBundleVersionCode;

        private static string buildCompanyName = "HTC Corp.";
        private static string buildProductName = "VRS Studio";
        private static string buildPackageName = "com.htc.vrs.vrsstudio";
        private static string buildVersion = "0.0.2";
        private static AndroidArchitecture buildAndroidArchitecture = AndroidArchitecture.None;
        private static ScriptingImplementation buildScriptingBackEnd = ScriptingImplementation.IL2CPP;
        private static ManagedStrippingLevel buildManagedStrippingLevel = ManagedStrippingLevel.Disabled;

        private static bool buildStripEngineCode = false;
        private static int buildBundleVersionCode = 1;

        private static string apkName = "VRSStudio.apk";
        private static string apkBuildDestination = null;

        private static bool autoRun = false;

        [MenuItem(VRSStudio_AutoRunAfterBuild, true)]
        private static bool ToggleAutoRunAfterBuildValidation()
        {
            Menu.SetChecked(VRSStudio_AutoRunAfterBuild, autoRun);

            return true;
        }

        [MenuItem(VRSStudio_AutoRunAfterBuild, false, 2)]
        private static void ToggleAutoRunAfterBuild()
        {
            autoRun = !autoRun;
            Menu.SetChecked(VRSStudio_AutoRunAfterBuild, autoRun);
        }

        [MenuItem(VRSStudio_BuildVRSStudio32, false, 3)]
        private static void BuildApk32()
        {
            buildAndroidArchitecture = AndroidArchitecture.ARMv7;

            if (string.IsNullOrEmpty(apkBuildDestination))
            {
                apkBuildDestination = Path.GetDirectoryName(Application.dataPath);
            }

            string[] scenePathArray = VRSStudioScenes.Instance.pathList.ToArray();
            BuildApk(apkBuildDestination + "/VRSStudio_Builds/armv7", scenePathArray);
        }

        [MenuItem(VRSStudio_BuildVRSStudio64, false, 4)]
        private static void BuildApk64()
        {
            buildAndroidArchitecture = AndroidArchitecture.ARM64;

            if (string.IsNullOrEmpty(apkBuildDestination))
            {
                apkBuildDestination = Path.GetDirectoryName(Application.dataPath);
            }

            string[] scenePathArray = VRSStudioScenes.Instance.pathList.ToArray();
            BuildApk(apkBuildDestination + "/VRSStudio_Builds/arm64", scenePathArray);
        }

        private static void BuildApk(string buildDestinationPath, string[] scenes)
		{
            BackupAndChangePlayerSettings();

            BuildOptions extraFlags = BuildOptions.None;
            BuildOptions buildOptions = (autoRun ? BuildOptions.AutoRunPlayer : BuildOptions.None) | extraFlags;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                options = buildOptions,
                target = BuildTarget.Android,
                scenes = scenes,
                targetGroup = BuildTargetGroup.Android,
                locationPathName = buildDestinationPath + "/" + apkName
            };


            BuildPipeline.BuildPlayer(buildPlayerOptions);

            RestorePlayerSettings();
        }

        //Backup Player Settings of original project and change the to VRS Studio APK settings
        private static void BackupAndChangePlayerSettings()
        {
            originalCompanyName = PlayerSettings.companyName;
            originalProductName = PlayerSettings.productName;
            originalPackageName = PlayerSettings.applicationIdentifier;
            originalVersion = PlayerSettings.bundleVersion;
            originalBundleVersionCode = PlayerSettings.Android.bundleVersionCode;
            originalScriptingBackEnd = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            originalAndroidArchitecture = PlayerSettings.Android.targetArchitectures;

            PlayerSettings.companyName = buildCompanyName;
            PlayerSettings.productName = buildProductName;
            PlayerSettings.applicationIdentifier = buildPackageName;
            PlayerSettings.bundleVersion = buildVersion;
            PlayerSettings.Android.bundleVersionCode = buildBundleVersionCode;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, buildScriptingBackEnd);
            PlayerSettings.Android.targetArchitectures = buildAndroidArchitecture;

            originalManagedStrippingLevel = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, buildManagedStrippingLevel);

            originalStripEngineCode = PlayerSettings.stripEngineCode;
            PlayerSettings.stripEngineCode = buildStripEngineCode;
        }

        //Restore Player Settings of original project
        private static void RestorePlayerSettings()
        {
            PlayerSettings.companyName = originalCompanyName;
            PlayerSettings.productName = originalProductName;
            PlayerSettings.applicationIdentifier = originalPackageName;
            PlayerSettings.bundleVersion = originalVersion;
            PlayerSettings.Android.bundleVersionCode = originalBundleVersionCode;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, originalScriptingBackEnd);
            PlayerSettings.Android.targetArchitectures = originalAndroidArchitecture;

            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, originalManagedStrippingLevel);

            PlayerSettings.stripEngineCode = originalStripEngineCode;
        }
    }
}
#endif