#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

//Checks if we are in a certain template, as some scripts are template-specific.
[InitializeOnLoad]
public class TemplateEditorDetection : Editor
{
    static TemplateEditorDetection()
    {
        //Get the current definition symbols
        var currentDefineSymbols =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefineSymbols = currentDefineSymbols.Split(';').ToList();

        //Template core namespace classes used for detection
        var platformer = Type.GetType("Platformer.Core.Simulation", false);
        var kart = Type.GetType("KartGame.KartSystems.KartMovement", false);
        var ballgame = Type.GetType("TeamBallGame.Simulation", false);

        //Template definition symbols for use with #if
        var platformerDefine = "UNITY_TEMPLATE_PLATFORMER";
        var kartDefine = "UNITY_TEMPLATE_KART";
        var ballgameDefine = "UNITY_TEMPLATE_BALLGAME";

        //add the define symbols if we are in a specific template, if they don't exist already
        if (platformer != null && !allDefineSymbols.Contains(platformerDefine)) allDefineSymbols.Add(platformerDefine);
        if (kart != null && !allDefineSymbols.Contains(kartDefine)) allDefineSymbols.Add(kartDefine);
        if (ballgame != null && !allDefineSymbols.Contains(ballgameDefine)) allDefineSymbols.Add(ballgameDefine);

        //apply the definition symbols
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefineSymbols.ToArray()));
    }
}
#endif
