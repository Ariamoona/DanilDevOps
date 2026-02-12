using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public static class BuildScript
{
    private const string BuildFolder = "Builds";

    [MenuItem("Build/Build All")]
    public static void BuildAll()
    {
        if (!Directory.Exists(BuildFolder))
            Directory.CreateDirectory(BuildFolder);

        BuildWindows();
        BuildAndroid();

        Debug.Log("=== BUILD ALL FINISHED ===");
    }

    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        string path = Path.Combine(BuildFolder, "Windows");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = Path.Combine(path, "Game.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        PrintReport(report);
    }

    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        string path = Path.Combine(BuildFolder, "Android");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = Path.Combine(path, "Game.apk"),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        PrintReport(report);
    }

    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    private static void PrintReport(BuildReport report)
    {
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize / 1024 / 1024} MB");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed!");
        }
    }
}
