using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class FractalTree : MonoBehaviour {

    public Dictionary<char, string> rules = new Dictionary<char, string>();
    [Range(0,6)]
    public int iterations = 4;
    public string input = "F";
    private string output;

    public string result;

    

    List<point> points = new List<point>();
    List<GameObject> branches = new List<GameObject>();
    
    public GameObject woodPrefab;
    public GameObject leafPrefab;

    public Material treeMaterial;
    public Material leafMaterial;

    private GameObject leaves;
    private GameObject wood;

    Vector3 newPosition;

    // Use this for initialization
    void Start () {
        leaves = new GameObject("leaves");
        wood = new GameObject("wood");
        leaves.transform.parent = this.gameObject.transform;
        wood.transform.parent = this.gameObject.transform;
        generate(this.transform.position);

        
    }

    public void generate(Vector3 newPosition) {
        this.newPosition = newPosition;
        GenerateTree(newPosition);


    }

    public void GenerateTree(Vector3 startPosition)
    {

        rules.Clear();
        points.Clear();
        branches.Clear();

        //F[*F]F[%F][+F]F[-F][F]
        rules.Add('F', "F[*F]F[%F][+F]F[-F][F]");

        // Apply rules for i interations
        output = input;
        for (int i = 0; i < iterations; i++)
        {
            output = applyRules(output);
        }
        result = output;
        determinePoints(output);
        CreateCylinders(startPosition, this.gameObject);
        //combineMeshes(wood, treeMaterial);
        //combineMeshes(leaves, leafMaterial);
    }

    public void combineMeshes(GameObject gameObject, Material material)
    {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        Matrix4x4 myTransform = transform.worldToLocalMatrix;

        for(int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = myTransform * meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

        }

        gameObject.GetComponent<MeshRenderer>().material = material;
        gameObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        gameObject.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        gameObject.transform.GetComponent<MeshCollider>().sharedMesh = wood.transform.GetComponent<MeshFilter>().mesh;

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

    struct point
    {
        public point(Vector3 rP, Vector3 rA, float rL) { Point = rP; Angle = rA; BranchLength = rL; }
        public Vector3 Point;
        public Vector3 Angle;
        public float BranchLength;
    }

    void determinePoints(string p_input)
    {
        Stack<point> returnValues = new Stack<point>();
        point lastPoint = new point(Vector3.zero, Vector3.zero, 1f);
        returnValues.Push(lastPoint);

        foreach (char c in p_input)
        {
            switch (c)
            {
                case 'F': // Draw line of length lastBranchLength, in direction of lastAngle
                    points.Add(lastPoint);

                    point newPoint = new point(lastPoint.Point + new Vector3(0, lastPoint.BranchLength, 0), lastPoint.Angle, 1f);
                    newPoint.BranchLength = lastPoint.BranchLength - 0.02f;
                    if (newPoint.BranchLength <= 0.0f) newPoint.BranchLength = 0.001f;

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
                case '%': // Rotate -30
                    lastPoint.Angle.z += -60.0f + Random.Range(0, 20);
                    break;
                case '*': // Rotate -30
                    lastPoint.Angle.z += 60.0f + Random.Range(0, 20);
                    break;
            }
        }
    }

    void CreateCylinders(Vector3 startPosition, GameObject treeObject)
    {

        float trunkSize = 0.4f;
        float prevTrunkSize = 0.4f;
        float branchSize = 1f;
        float oldBranchSize = 10f;


        for (int i = 0; i < points.Count; i += 2)
        {

            if (points[i].Point.x == 0 && points[i + 1].Point.x == 0)
            {
                trunkSize = trunkSize - 0.003f;
                CreateCylinder(points[i], points[i + 1], 3.6f, startPosition, treeObject, prevTrunkSize, trunkSize, i, false);
                prevTrunkSize = trunkSize;
                oldBranchSize = prevTrunkSize;
            }

            else if (points[i + 1].Point != points[i + 2].Point)
            {
                CreateCylinder(points[i], points[i + 1], 0.1f, startPosition, treeObject, oldBranchSize, oldBranchSize /2, i, true);
                CreateLeaf(points[i], points[i + 1]);
            }

            else
            {
                if (oldBranchSize == prevTrunkSize)
                {
                    oldBranchSize = trunkSize / 2;
                }

                branchSize = oldBranchSize - 0.01f;

                CreateCylinder(points[i], points[i + 1], 0.1f, startPosition, treeObject, branchSize, oldBranchSize, i, false);
                oldBranchSize = branchSize;
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

    void CreateCylinder(point point1, point point2, float radius, Vector3 startPosition, GameObject treeObject, float oldSize, float newSize, int name, bool lastTrunk)
    {
        GameObject newCylinder = (GameObject)Instantiate(woodPrefab, startPosition, Quaternion.identity);
        newCylinder.name = "point 1 " + point1.Point + "point 2" + point2.Point;
        newCylinder.GetComponent<Cone>().GenerateCone(oldSize, newSize, lastTrunk);
        newCylinder.SetActive(true);

        float length = Vector3.Distance(point2.Point, point1.Point);
        radius = radius * length;

        Vector3 scale = new Vector3(1, 1, length + 0.01f);
        
        newCylinder.transform.localScale = scale;
        newCylinder.transform.position = newCylinder.transform.position + point1.Point;
        newCylinder.transform.Rotate(point2.Angle);
        newCylinder.transform.parent = wood.transform;

        branches.Add(newCylinder);
    }

    void CreateLeaf(point point1, point point2)
    {
        GameObject leaf = (GameObject)Instantiate(leafPrefab, point2.Point, Quaternion.identity);
        //leaf.transform.rotation = Quaternion.Euler(point2.Angle);
        /*point2.Angle.x = 270;
        point2.Angle.y = point2.Angle.y + 180;
        point2.Angle.z = point2.Angle.z + 180;*/

        point2.Angle = Vector3.RotateTowards(point2.Angle, new Vector3(0, 0, 0), 1, 0.0f);
        point2.Angle.y = point2.Angle.y + 180;
        point2.Angle.x = -80;

        leaf.transform.rotation = Quaternion.Euler(point2.Angle);
        //leaf.transform.parent = leaves.transform;

    }
}
