//place this script in the Editor folder within Assets.

using System.Collections.Generic;
using UnityEditor;

//to be used on the command line:
//$ Unity -quit -batchmode -executeMethod WebGLBuilder.build

internal class WebGLBuilder
{
    private static string[] GetScenes()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var enabledScenes = new List<string>();

        foreach (var scene in scenes)
            if (scene.enabled)
                enabledScenes.Add(scene.path);

        return enabledScenes.ToArray();
    }

    private static void Build()
    {
        BuildPipeline.BuildPlayer(GetScenes(), "build\\web", BuildTarget.WebGL, BuildOptions.None);
        //BuildPipeline.BuildPlayer(GetScenes(), "build\\web", BuildTarget.WebGL, BuildOptions.Development);
    }

    private static void BuildBeta()
    {
//       SetVirtualRealitySDKs
        RTBuildTools.AddDefine(BuildTargetGroup.WebGL, "RT_BETA");
        BuildPipeline.BuildPlayer(GetScenes(), "build\\web", BuildTarget.WebGL, BuildOptions.None);
        RTBuildTools.RemoveDefine(BuildTargetGroup.WebGL, "RT_BETA");
    }
}