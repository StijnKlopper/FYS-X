using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BuildingGenerator buildingGenerator = (BuildingGenerator)target;

        if (GUILayout.Button("Random values"))
        {
            buildingGenerator.RandomValues();
        }

        if (GUILayout.Button("Generate"))
        {
            buildingGenerator.Generate();
        }

    }

}
