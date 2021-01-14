using LibNoise.Generator;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FractalTree : MonoBehaviour
{

    public Dictionary<char, string> rules = new Dictionary<char, string>();
    [Range(0, 4)]
    public int iterations;
    public string input = "F";
    private string output;

    public string result;

    Perlin perlin;

    List<point> points = new List<point>();
    List<GameObject> branches = new List<GameObject>();

    [Header("THESE ARE STANDARD VALUES NOT CHANGEABLE")]
    [Tooltip("THESE ARE STANDARD VALUES. NOT CHANGEABLE")]
    public GameObject woodPrefab;
    public GameObject plantLeafPrefab;
    public Material plantLeafMaterial;

    private Material treeMaterial;
    private Material leafMaterial;
    private GameObject leafPrefab;
    private Color plantColor;

    private GameObject tree;
    private GameObject leaves;
    private GameObject trunks;
    private GameObject plants;
    private GameObject stamps;

    [Header("When adding a Biome also add in code!")]
    public biomePref[] biomePref; 
   
    Vector3 newPosition;

    // Use this for initialization
    void Start()
    {
        float frequency = 1f;
        float persistence = 1f;
        float lacunarity = 2f;
        int octaves = 1;
        perlin = new Perlin(frequency, lacunarity, persistence, octaves, GameObject.Find("Level").GetComponent<TerrainGenerator>().Seed, LibNoise.QualityMode.High);
    }

    public GameObject GenerateTree(Vector3 startPosition , BiomeType biome)
    {
        (treeMaterial,leafPrefab,leafMaterial,plantColor) = getBiomePrefabs(biome);
        // Initalize parents for combining meshes
        tree = new GameObject("tree " + startPosition);
        leaves = new GameObject("leaves " + startPosition);
        trunks = new GameObject("trunks " + startPosition);

        tree.transform.parent = this.gameObject.transform;
        leaves.transform.parent = tree.transform;
        trunks.transform.parent = tree.transform;

        tree.transform.position  = startPosition;
        leaves.transform.position = startPosition;
        trunks.transform.position = startPosition;

        this.newPosition = startPosition;
        rules.Clear();
        points.Clear();
        branches.Clear();

        // Add rule to list
        rules.Add('F', "F[+F]F[-F][F]");
        // Apply rules for i interations
        output = input;
        for (int i = 0; i < iterations; i++)
        {
            output = applyRules(output);
        }
        result = output;

        determinePointsTree(output);

        CreateCylinders(startPosition);

        combineMeshes(leaves, leafMaterial, false);
        combineMeshes(trunks, treeMaterial, false);
        return tree;
    }
    public GameObject GeneratePlants(Vector3 startPosition, BiomeType biome)
    {
        (treeMaterial, leafPrefab, leafMaterial, plantColor) = getBiomePrefabs(biome);

        // Initalize parents for combining meshes
        plants = new GameObject("Plants" + startPosition);
        plants.transform.parent = this.gameObject.transform;
        plants.transform.position = startPosition;

        stamps = new GameObject("Stamps" + startPosition);
        stamps.transform.position = startPosition;
        stamps.transform.parent = plants.gameObject.transform;

        leaves = new GameObject("leaves " + startPosition);
        leaves.transform.position = startPosition;
        leaves.transform.parent = plants.transform;

        this.newPosition = startPosition;
        rules.Clear();
        points.Clear();
        branches.Clear();

        // Add Rule to list
        rules.Add('F', "F[+F]F[-F][F]");

        // Apply rules for 1 iteration 
        output = input;
        output = applyRules(output);
        result = output;
        determinePointsPlant(output);
        CreatePlants(startPosition);
        combineMeshes(leaves, plantLeafMaterial, true);
        combineMeshes(stamps, treeMaterial, false);
        return plants;
    }

    // Combine Meshes and add the Material to the mesh to improve performance
    public void combineMeshes(GameObject gameObject, Material material, bool color)
    {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        Matrix4x4 myTransform = gameObject.transform.worldToLocalMatrix;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = myTransform * meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            Destroy(meshFilters[i].gameObject);

        }
      
        gameObject.GetComponent<MeshRenderer>().material = material;
        if (color)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = plantColor;
        }
        gameObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        gameObject.transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        gameObject.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        gameObject.transform.gameObject.SetActive(true);
    }
    string applyRules(string p_input)
    {
        StringBuilder sb = new StringBuilder();
        // Loop through characters in the input string
        foreach (char c in p_input)
        {
            // If character matches key in rules, then replace character with rhs of rule
            if (rules.ContainsKey(c))
            {
                sb.Append(rules[c]);
            }
            // If not, keep the character
            else
            {
                sb.Append(c);
            }
        }
        // Return string with rules applied
        return sb.ToString();
    }
    // Create Point with a Position, Angle and BranchLength
    struct point
    {
        public point(Vector3 rP, Vector3 rA, float rL) { Point = rP; Angle = rA; BranchLength = rL; }
        public Vector3 Point;
        public Vector3 Angle;
        public float BranchLength;
    }
    // Determine Points using a String input
    void determinePointsTree(string p_input)
    {
        float scale = 100.777f;

        Stack<point> returnValues = new Stack<point>();
        point lastPoint = new point(Vector3.zero, Vector3.zero, 0.7f); 
        returnValues.Push(lastPoint);

        foreach (char c in p_input)
        {
            double sampleX = (newPosition.x) / scale;
            double sampleY = (newPosition.y) / scale;

            float perlinValue = (float)(perlin.GetValue(sampleY + lastPoint.Angle.y, 0, sampleX + lastPoint.Angle.y) + 1) / 2 * 100;
            //perlinValue = perlinValue * 10f;
            //Debug.Log(perlinValue);
            switch (c)
            {
                case 'F': // Draw line of length lastBranchLength, in direction of lastAngle
                    points.Add(lastPoint);

                    point newPoint = new point(lastPoint.Point + new Vector3(0, lastPoint.BranchLength, 0), lastPoint.Angle, 1f);
                    newPoint.BranchLength = lastPoint.BranchLength - 0.02f;
                    if (newPoint.BranchLength <= 0.0f) newPoint.BranchLength = 0.1f;

                    newPoint.Angle.y = lastPoint.Angle.y + UnityEngine.Random.Range(-30, 30);
                    newPoint.Point = pivot(newPoint.Point, lastPoint.Point, new Vector3(newPoint.Angle.x, 0, 0));
                    newPoint.Point = pivot(newPoint.Point, lastPoint.Point, new Vector3(0, 0, newPoint.Angle.z));
                    newPoint.Point = pivot(newPoint.Point, lastPoint.Point, new Vector3(0, newPoint.Angle.y, 0));

                    points.Add(newPoint);
                    lastPoint = newPoint;
                    break;
                case '+': // Rotate +30
                    lastPoint.Angle.x += 60.0f + Random.Range(0, 40);
                    break;
                case '[': // Save State
                    returnValues.Push(lastPoint);
                    break;
                case '-': // Rotate -30
                    lastPoint.Angle.x += -60.0f + Random.Range(0, 40);
                    break;
                case ']': // Load Saved State
                    lastPoint = returnValues.Pop();
                    break;
            }          
        }
    }


    void determinePointsPlant(string p_input)
    {
        float scale = 100.777f;
        double sampleX = (newPosition.x) / scale;
        double sampleY = (newPosition.y) / scale;

        // Use Perlin to Pseudo Randomize Angles 
        float perlinValue = (float)((perlin.GetValue(sampleY, 0, sampleX) + 1) / 2) * 20f;
        float valuetoAdd = (2 % (perlinValue / 20)) / 10f;
        float biggerValue = valuetoAdd * 100;

        Stack<point> returnValues = new Stack<point>();
        point lastPoint = new point(new Vector3(0, 0, 0), Vector3.zero, 0.2f + valuetoAdd);
        
        returnValues.Push(lastPoint);

        foreach (char c in p_input)
        {
            switch (c)
            {
                case 'F': // Draw line of length lastBranchLength, in direction of lastAngle
                    points.Add(lastPoint);

                    point newPoint = new point(lastPoint.Point + new Vector3(0, lastPoint.BranchLength, 0), lastPoint.Angle, 1f);
                    newPoint.BranchLength = lastPoint.BranchLength - 0.02f;
                    if (newPoint.BranchLength <= 0.0f) newPoint.BranchLength = 0.1f;
           
                    newPoint.Angle.y = lastPoint.Angle.y + perlinValue - biggerValue;
    
                    newPoint.Point = pivot(newPoint.Point, lastPoint.Point, new Vector3(newPoint.Angle.x, 0, 0));
                    newPoint.Point = pivot(newPoint.Point, lastPoint.Point, new Vector3(0, 0, newPoint.Angle.z));
                    newPoint.Point = pivot(newPoint.Point, lastPoint.Point, new Vector3(0, newPoint.Angle.y, 0));

                    points.Add(newPoint);
                    lastPoint = newPoint;
                    break;
                case '+': // Rotate +30
                    lastPoint.Angle.x += 60.0f + perlinValue + biggerValue;
                    break;
                case '[': // Save State
                    returnValues.Push(lastPoint);
                    break;
                case '-': // Rotate -30
                    lastPoint.Angle.x += -60.0f + perlinValue + biggerValue;
                    break;
                case ']': // Load Saved State
                    lastPoint = returnValues.Pop();
                    break;
            }
        }
    }

    // Create Cylinders using a startPositiong
   
    void CreateCylinders(Vector3 startPosition)
    {

        float trunkSize = 0.3f;
        float prevTrunkSize = 0.3f;
        float branchSize = 1f;
        float oldBranchSize = 10f;

        for (int i = 0; i < points.Count; i += 2)
        {
            // Prevent Branchsize/oldBrancheSize from going under 0
            if (branchSize < 0f)
            {
                branchSize = 0.01f;
            }
            if (oldBranchSize < 0f)
            {
                oldBranchSize = 0.01f;
            }
            // If Start Point
            if (points[i].Point.x == 0 && points[i + 1].Point.x == 0)
            {
                trunkSize = trunkSize - 0.003f;
                CreateCylinder(points[i], points[i + 1], 3.6f, startPosition, prevTrunkSize, trunkSize, false, false);
                prevTrunkSize = trunkSize;
                oldBranchSize = prevTrunkSize;
            }
            else if (points[i + 1].Point != points[i + 2].Point)
            {
                CreateCylinder(points[i], points[i + 1], 0.1f, startPosition, oldBranchSize, oldBranchSize / 2, true, false);
                CreateLeaf(points[i], points[i + 1], branchSize, false);
            }
            else
            {
                if (oldBranchSize == prevTrunkSize)
                {
                    oldBranchSize = trunkSize / 2;
                }
                branchSize = oldBranchSize - 0.01f;

                CreateCylinder(points[i], points[i + 1], 0.1f, startPosition, branchSize, oldBranchSize, false, false);
                oldBranchSize = branchSize;
                CreateLeaf(points[i], points[i + 1], branchSize, false);
            }
        }
    }

    // Create Plants using a startPoisition
    void CreatePlants(Vector3 startPosition)
    {

        float trunkSize = 0.1f;
        float prevTrunkSize = 0.1f;
        float branchSize = 1f;
        float oldBranchSize = 1f;
        float startSize = 1f;

        for (int i = 0; i < points.Count; i += 2)
        {
            // Prevent Branchsize/oldBrancheSize from going under 0
            if (branchSize < 0f)
            {
                branchSize = 0.1f;
            }
            if (oldBranchSize < 0f)
            {
                oldBranchSize = 0.1f;
            }
            // Start Point
            if (points[i].Point.x == 0 && points[i + 1].Point.x == 0)
            {
                trunkSize = trunkSize - 0.003f;
                CreateCylinder(points[i], points[i + 1], startSize, startPosition, prevTrunkSize, trunkSize, false, true);
                prevTrunkSize = trunkSize;
                oldBranchSize = prevTrunkSize;
                CreateLeaf(points[i], points[i + 1], branchSize, true);
            }
            else if (points[i + 1].Point != points[i + 2].Point)
            {
                CreateCylinder(points[i], points[i + 1], 0.1f, startPosition, oldBranchSize, oldBranchSize / 2, true, true);
                CreateLeaf(points[i], points[i + 1], branchSize, true);
            }
            else
            {
                if (oldBranchSize == prevTrunkSize)
                {
                    oldBranchSize = trunkSize / 2;
                }
                branchSize = oldBranchSize - 0.01f;

                CreateCylinder(points[i], points[i + 1], 0.1f, startPosition, branchSize, oldBranchSize, false, true);
                oldBranchSize = branchSize;
                CreateLeaf(points[i], points[i + 1], branchSize, true);
            }
        }
    }
    // Pivot point1 around point2 by angles
    Vector3 pivot(Vector3 point1, Vector3 point2, Vector3 angles)
    {
        Vector3 dir = point1 - point2;
        dir = Quaternion.Euler(angles) * dir;
        point1 = dir + point2;
        return point1;
    }

    // Cylinders are being placed between 2 points with variating sizes to create trunks/branches
    void CreateCylinder(point point1, point point2, float radius, Vector3 startPosition, float oldSize, float newSize, bool lastTrunk, bool isPlant)
    {
        GameObject newCylinder = (GameObject)Instantiate(woodPrefab, startPosition, Quaternion.identity);
        newCylinder.name = "point 1 " + point1.Point + "point 2" + point2.Point;
        newCylinder.GetComponent<Cone>().GenerateCone(oldSize, newSize, lastTrunk);
        newCylinder.SetActive(true);

        float length = Vector3.Distance(point2.Point, point1.Point);
        if (isPlant)
        {
            Vector3 scale = new Vector3(0.1f, length, 0.1f);
            newCylinder.transform.localScale = scale;
            newCylinder.transform.position = newCylinder.transform.position + point1.Point;
            newCylinder.transform.Rotate(point2.Angle);
            branches.Add(newCylinder);
            newCylinder.transform.parent = stamps.transform;

        }
        else
        {
            radius = radius * length;
            Vector3 scale = new Vector3(point1.BranchLength, length, point1.BranchLength - 0.01f);
            newCylinder.transform.localScale = scale;
            newCylinder.transform.position = newCylinder.transform.position + point1.Point;
            newCylinder.transform.Rotate(point2.Angle);
            branches.Add(newCylinder);
            newCylinder.transform.parent = trunks.transform;
        }
        
    }
    // Create a Leaf based on Points position
    void CreateLeaf(point point1, point point2, float scale, bool isPlant)
    {

        if (isPlant)
        {
            GameObject leaf = (GameObject)Instantiate(plantLeafPrefab, point2.Point + newPosition, Quaternion.identity);
            // Scale to variate Leaves sizes
            leaf.transform.localScale = new Vector3(leaf.transform.localScale.x + scale, leaf.transform.localScale.y + scale, leaf.transform.localScale.z + scale);
            leaf.transform.rotation = Quaternion.Euler(point2.Angle);
            leaf.transform.parent = leaves.transform;
        }
        else
        {
            GameObject leaf = (GameObject)Instantiate(leafPrefab, point2.Point + newPosition, Quaternion.identity);
            // Scale to variate Leaves sizes
            leaf.transform.localScale = new Vector3(leaf.transform.localScale.x + scale, leaf.transform.localScale.y + scale, leaf.transform.localScale.z + scale);

            // Add value to angle to fix a certain bug where the point angle starts rotating around axis
            point2.Angle = Vector3.RotateTowards(point2.Angle, new Vector3(0, 0, 0), 1, 0.0f);
            point2.Angle.y = point2.Angle.y + 180;
            point2.Angle.x = -80;
            leaf.transform.rotation = Quaternion.Euler(point2.Angle);
            leaf.transform.parent = leaves.transform;
        }
    }

    // Use Values from Editor to get different Tree,Leaves and Plants per biome
    (Material TreeMaterial, GameObject LeavesPrefab, Material LeavesMaterial,Color plantColor) getBiomePrefabs(BiomeType biome)
    {      
        switch (biome)
        {
            case DefaultBiomeType r:
                return (biomePref[0].TreeMaterial, biomePref[0].LeavesPrefab, biomePref[0].LeavesMaterial, biomePref[0].plantColor);
            case PlainsBiomeType p:
                return (biomePref[1].TreeMaterial, biomePref[1].LeavesPrefab, biomePref[1].LeavesMaterial, biomePref[1].plantColor);
            case ForestBiomeType f:
                return (biomePref[2].TreeMaterial, biomePref[2].LeavesPrefab, biomePref[2].LeavesMaterial, biomePref[2].plantColor);
            default:
                return (biomePref[0].TreeMaterial, biomePref[0].LeavesPrefab, biomePref[0].LeavesMaterial, biomePref[0].plantColor);
            case null:
                return (biomePref[0].TreeMaterial, biomePref[0].LeavesPrefab, biomePref[0].LeavesMaterial, biomePref[0].plantColor);
        }
    }

}

[System.Serializable]
public struct biomePref
{
    public enumBiome biomeType;
    public Material TreeMaterial;
    public GameObject LeavesPrefab;
    public Material LeavesMaterial;
    public Color plantColor;
}

// Create Dropdown with Biomes
public enum enumBiome
{
    DefaultBiomeType,
    PlainsBiomeType,
    ForestBiomeType
}