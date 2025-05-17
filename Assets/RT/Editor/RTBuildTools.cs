//place this script in the Editor folder within Assets.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//to be used on the command line:
//$ Unity -quit -batchmode -executeMethod WebGLBuilder.build

internal class RTBuildTools
{
    public static void AddDefine(BuildTargetGroup buildGroup, string newDefine)
    {
        Debug.Log("Adding define: '" + newDefine + "'");

        string defines;
        defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup);
        defines = AddCompilerDefines(defines, newDefine);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, defines);
    }

    public static void RemoveDefine(BuildTargetGroup buildGroup, string newDefine)
    {
        Debug.Log("Removing define: '" + newDefine + "'");

        string defines;
        defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup);
        defines = RemoveCompilerDefines(defines, newDefine);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, defines);
    }


    public static string AddCompilerDefines(string defines, params string[] toAdd)
    {
        var splitDefines = new List<string>(defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        foreach (var add in toAdd)
            if (!splitDefines.Contains(add))
                splitDefines.Add(add);

        return string.Join(";", splitDefines.ToArray());
    }

    public static string RemoveCompilerDefines(string defines, params string[] toRemove)
    {
        var splitDefines = new List<string>(defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
        foreach (var remove in toRemove)
            splitDefines.Remove(remove);

        return string.Join(";", splitDefines.ToArray());
    }
}