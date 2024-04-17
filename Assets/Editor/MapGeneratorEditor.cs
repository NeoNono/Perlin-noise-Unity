using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;
        
        if (GUILayout.Button("Generate") || DrawDefaultInspector() && mapGenerator.autoUpdate) 
            mapGenerator.GenerateMap();
    }
}
