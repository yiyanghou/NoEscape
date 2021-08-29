#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

namespace PuzzleGame
{
    public static class DumpGameText
    {
        [MenuItem("PuzzleGame/DumpGameText")]
        static void DoDumpGameText()
        {
            string[] promptPaths = AssetDatabase.FindAssets($"t: {typeof(PromptDef).Name}", 
                new string[] { "Assets/Sos" });

            string[] dialoguePaths = AssetDatabase.FindAssets($"t: { typeof(DialogueDef).Name}",
                new string[] { "Assets/Sos" }); 

            Debug.Log($"found {promptPaths.Length + dialoguePaths.Length} assets");

            if(!Directory.Exists(Application.dataPath + "/../dumpedTexts"))
            {
                Directory.CreateDirectory(Application.dataPath + "/../dumpedTexts");
            }

            using (StreamWriter file = new StreamWriter(Application.dataPath + "/../dumpedTexts/dialogues.txt", false))
            {
                foreach (var path in dialoguePaths)
                {
                    DialogueDef def = AssetDatabase.LoadAssetAtPath<DialogueDef>(AssetDatabase.GUIDToAssetPath(path));

                    file.WriteLine(def.name);
                    file.WriteLine("dialogues:");

                    foreach (var dialogue in def.dialogues)
                    {
                        file.WriteLine(dialogue);
                    }

                    file.WriteLine();
                }
            }

            using (StreamWriter file = new StreamWriter(Application.dataPath + "/../dumpedTexts/prompts.txt", false))
            {
                foreach (var path in promptPaths)
                {
                    PromptDef def = AssetDatabase.LoadAssetAtPath<PromptDef>(AssetDatabase.GUIDToAssetPath(path));

                    file.WriteLine(def.name);
                    file.WriteLine("prompt text:");
                    file.WriteLine(def.prompt);

                    if(def.options.Length > 0)
                    {
                        file.WriteLine("prompt options:");
                        foreach (var optionDesc in def.options)
                        {
                            file.WriteLine(optionDesc.optionName);
                        }
                    }

                    file.WriteLine();
                }
            }
        }
    }
}

#endif
