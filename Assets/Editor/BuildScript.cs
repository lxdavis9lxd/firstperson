using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System;

public class BuildScript
{
    [MenuItem("Build/Android")]
    public static void BuildAndroid()
    {
        try
        {
            // Set the Android SDK path - update this if needed
            string androidSdkPath = Environment.GetEnvironmentVariable("ANDROID_HOME");
            if (string.IsNullOrEmpty(androidSdkPath))
            {
                androidSdkPath = "/usr/lib/android-sdk";
            }
            EditorPrefs.SetString("AndroidSdkRoot", androidSdkPath);
            
            Debug.Log("Setting up Android build with SDK path: " + androidSdkPath);
            
            // Configure Android player settings
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel31; // Android 12.0
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // Set company and product name
            PlayerSettings.companyName = "YourCompanyName";
            PlayerSettings.productName = "FPS Game";
            
            // Create build directory if it doesn't exist
            string buildPath = "Builds/Android";
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            
            // Define build options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = FindAllScenes();
            buildPlayerOptions.locationPathName = buildPath + "/FPSGame.apk";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.Development;
            
            // Build the project
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Build error: " + e.Message);
        }
    }
    
    private static string[] FindAllScenes()
    {
        // Find all scene files in the Assets folder
        string[] scenes = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
        
        // Log found scenes
        Debug.Log("Found " + scenes.Length + " scenes:");
        foreach (string scene in scenes)
        {
            Debug.Log("  - " + scene);
        }
        
        return scenes;
    }
}
