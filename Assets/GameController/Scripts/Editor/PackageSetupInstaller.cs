using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class PackageSetupInstaller
{
    private static string MarkerPath =>
        Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".package_setup_done");

    private static string BackupAbsPath =>
        Path.Combine(Application.dataPath, "GameController", "Settings", "InputManagerBackup.txt");

    private const string BackupAssetPath = "Assets/GameController/Settings/InputManagerBackup.txt";

    private static readonly string[] RequiredTags =
    {
        "Action",
        "SitObject",
        "AutoCrouch",
        "LadderTrigger"
    };

    static PackageSetupInstaller()
    {
        if (File.Exists(MarkerPath)) return;
        if (!File.Exists(BackupAbsPath)) return;

        EditorApplication.delayCall += ShowSetupDialog;
    }

    static void ShowSetupDialog()
    {
        if (File.Exists(MarkerPath)) return;

        bool apply = EditorUtility.DisplayDialog(
            "GameController Setup",
            "Apply Input Manager settings, create required tags and disable New Input System?\n\nUnity will restart after applying.",
            "Apply", "Skip"
        );

        File.WriteAllText(MarkerPath, "done");

        if (!apply)
        {
            Debug.LogWarning("[PackageSetup] Setup skipped. Input may not work correctly.");
            return;
        }

        bool inputOk = ApplyInputManagerSettings();
        bool projectOk = DisableNewInputSystem();
        CreateTags();

        if (inputOk && projectOk)
        {
            bool restart = EditorUtility.DisplayDialog(
                "Restart Required",
                "Settings applied.\nUnity must restart to apply changes.",
                "Restart Now", "Later"
            );

            if (restart)
                EditorApplication.OpenProject(Directory.GetParent(Application.dataPath).FullName);
        }
    }

    static bool ApplyInputManagerSettings()
    {
        TextAsset backup = AssetDatabase.LoadAssetAtPath<TextAsset>(BackupAssetPath);
        if (backup == null)
        {
            Debug.LogError("[PackageSetup] InputManagerBackup.txt not found at: " + BackupAssetPath);
            return false;
        }

        string destPath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            "ProjectSettings", "InputManager.asset"
        );

        File.WriteAllText(destPath, backup.text);
        Debug.Log("[PackageSetup] InputManager.asset overwritten.");
        return true;
    }

    static bool DisableNewInputSystem()
    {
        // Use SerializedObject instead of regex — same approach as CreateTags()
        SerializedObject projectSettings = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset")
        );

        SerializedProperty activeInputHandler = projectSettings.FindProperty("activeInputHandler");

        if (activeInputHandler == null)
        {
            Debug.LogWarning("[PackageSetup] activeInputHandler property not found in ProjectSettings.");
            return false;
        }

        // 0 = old only, 1 = new only, 2 = both
        activeInputHandler.intValue = 0;
        projectSettings.ApplyModifiedProperties();

        Debug.Log("[PackageSetup] New Input System disabled.");
        return true;
    }

    static void CreateTags()
    {
        // SerializedObject gives access to TagManager without writing raw YAML
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset")
        );

        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        foreach (string tag in RequiredTags)
        {
            // Check if tag already exists
            bool exists = false;
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (tagsProperty.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                Debug.Log("[PackageSetup] Tag already exists, skipping: " + tag);
                continue;
            }

            // Append new tag
            tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
            tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = tag;
            Debug.Log("[PackageSetup] Tag created: " + tag);
        }

        tagManager.ApplyModifiedProperties();
    }
}