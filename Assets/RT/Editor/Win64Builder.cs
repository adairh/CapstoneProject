//place this script in the Editor folder within Assets.

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal.VR;
using UnityEngine;

//to be used on the command line:
//$ Unity -quit -batchmode -executeMethod WebGLBuilder.build

internal class Win64Builder
{
    private static string GetProjectName()
    {
        var s = Application.dataPath.Split('/');
        var temp = s[s.Length - 2];
        temp = temp.Replace("TempWin64",
            ""); //I add this when making temp dirs to build in, so this removes it so the filename can be correct
        return temp;
    }

    private static string[] GetScenes()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var enabledScenes = new List<string>();

        foreach (var scene in scenes)
            if (scene.enabled)
                enabledScenes.Add(scene.path);

        return enabledScenes.ToArray();
    }

    private static void BuildForceVR()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Standalone, new[] { "OpenVR", "Oculus" });
        BuildPipeline.BuildPlayer(GetScenes(), "build\\win\\" + GetProjectName() + ".exe",
            BuildTarget.StandaloneWindows64, BuildOptions.None);

        //BuildPipeline.BuildPlayer(GetScenes(), "build\\win", BuildTarget.StandaloneWindows64, BuildOptions.Development);
    }

    private static void BuildBeta()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        RTBuildTools.AddDefine(BuildTargetGroup.Standalone, "RT_BETA");
        BuildPipeline.BuildPlayer(GetScenes(), "build\\win\\" + GetProjectName() + ".exe",
            BuildTarget.StandaloneWindows64, BuildOptions.None);
        RTBuildTools.RemoveDefine(BuildTargetGroup.Standalone, "RT_BETA");
    }

    private static void BuildRelease()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        RTBuildTools.AddDefine(BuildTargetGroup.Standalone, "RT_RELEASE");
        BuildPipeline.BuildPlayer(GetScenes(), "build\\win\\" + GetProjectName() + ".exe",
            BuildTarget.StandaloneWindows64, BuildOptions.None);
        RTBuildTools.RemoveDefine(BuildTargetGroup.Standalone, "RT_RELEASE");
    }
}