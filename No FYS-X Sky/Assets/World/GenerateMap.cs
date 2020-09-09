using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapDisplay))]
public class GenerateMap : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapDisplay mapdisp = (MapDisplay)target;

        if (GUILayout.Button("Generate")) {
            mapdisp.GenerateMap();
        }

    }

}
