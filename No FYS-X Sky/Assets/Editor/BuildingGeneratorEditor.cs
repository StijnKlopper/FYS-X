using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BuildingGenerator buildingGenerator = (BuildingGenerator)target;

        if (GUILayout.Button("Generate a house"))
        {
            buildingGenerator.GeneratePreviewHouse();
        }
    }
}
