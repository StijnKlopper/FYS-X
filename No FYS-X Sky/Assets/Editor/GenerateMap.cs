using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapDisplay))]
public class GenerateMap : Editor
{
    public override void OnInspectorGUI()
    {
        MapDisplay mapdisp = (MapDisplay)target;

        if (DrawDefaultInspector())
        {
            if (mapdisp.autoUpdate)
            {
                mapdisp.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapdisp.GenerateMap();
        }
        if (GUILayout.Button("Copy Curve to clipboard"))
        {
            mapdisp.CopyCurve();
        }


    }

}
