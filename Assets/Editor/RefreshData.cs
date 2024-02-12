using UnityEngine;
using UnityEditor;
using System.Collections;

public class RefreshData : AssetPostprocessor
{
    private static float _startTime;
    private static bool _newMemory = false;
    [MenuItem("MemoryCreator/Reimport Data and Start Memory")]
    static void ReimportData()
    {
        FlowCanvas.Nodes.MemoryStartNode.instance.processAndSerializeNodes();
        _newMemory = true;
        AssetDatabase.Refresh();
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (_newMemory)
        {
            Debug.Log("Assets refreshed...");

            foreach (string str in importedAssets)
            {
                Debug.Log("Reimported Asset: " + str);
            }
            foreach (string str in deletedAssets)
            {
                Debug.Log("Deleted Asset: " + str);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            }

            EditorApplication.isPlaying = true;
            /*
            _newMemory = false;
            _startTime = Time.realtimeSinceStartup;
            EditorApplication.update += OnEditorUpdate;
            */
        }
    }
    /*
    static void OnEditorUpdate()
    {
        float deltaTime = Time.realtimeSinceStartup - _startTime;
        // we wait an aribtrary amount of time for asset import to actually be done.
        if (deltaTime > 1)
        {
            Debug.Log("Starting Memory : " + FlowCanvas.Nodes.MemoryStartNode.instance.memoryId);
            EditorApplication.update -= OnEditorUpdate;
            UnityEditor.EditorApplication.isPlaying = true;
        }
    }
    */
}