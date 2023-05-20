using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConeObjectPlacement : EditorWindow {
    
    private string conePath = "Assets/CONE/Assets/Cone/Models/Cone/lejek_nowy_skomplikowany_2 (1).fbx";

    private GameObject cone;
    public bool isOn;
    private float targetConeScale = 10000;
    public float angle = 45;
    public float offset = -60;
    private float height;
    private float diameter;
    private float bottom;

    [MenuItem("Window/Cone object placement")]
    static void Init()
    {
        
        ConeObjectPlacement window = (ConeObjectPlacement)EditorWindow.GetWindow(typeof(ConeObjectPlacement));
        window.Show();
    }

    void OnGUI()
    {
        if (cone == null)
        {
            FindCone();
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Cone placement system", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.Space(40);
        conePath = EditorGUILayout.TextField("Cone path", conePath);
        targetConeScale = EditorGUILayout.FloatField("Cone scale", targetConeScale);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Enabled: ", EditorStyles.boldLabel);
        isOn = GUILayout.Toggle(isOn, "");
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Angle: ", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        angle = EditorGUILayout.FloatField(angle, GUILayout.Width(50));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Offset: ", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        offset = EditorGUILayout.FloatField(offset, GUILayout.Width(50));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.Space(30);


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Important: only objects tagged with", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("'ConeObject' or 'ConeRunner' tag", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("will can be snapped using this system", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
    private void FindCone()
    {
        GameObject t = (GameObject)AssetDatabase.LoadAssetAtPath(conePath, typeof(GameObject));
        cone = t;
    }
    private void Update()
    {
        if(!EditorApplication.isPlaying)
        {
            RepositionObject();
        }
    }
    void RepositionObject()
    {
        if (!isOn)
        {
            return;
        }

        if (Selection.activeGameObject != null && (Selection.activeGameObject.tag == "ConeObject" || Selection.activeGameObject.tag == "ConeRunner") && cone != null)
        {
            CalculateHeight();
            CalculateDiameter();
            CalculateBottom();
            
            foreach(GameObject go in Selection.gameObjects)
            {
                float ang = CalculateAngle(go.transform.position);

                CalculatePositionResponse cpr = CalculatePosition(go.transform, ang);
                Vector3 upVector = Quaternion.AngleAxis(angle, cpr.crossVector) * Vector3.up;
                Bounds bounds = new Bounds();

                if (go.GetComponent<BoxCollider>() != null)
                {
                    bounds = go.GetComponent<BoxCollider>().bounds;
                }
                else if (go.GetComponent<MeshRenderer>() != null)
                {
                    bounds = go.GetComponent<MeshRenderer>().bounds;
                }

                Vector3 downVector = Quaternion.AngleAxis(135, cpr.crossVector) * Vector3.up;
                Vector3 parallelVector = Quaternion.AngleAxis(45, cpr.crossVector) * Vector3.up;
                float forwardAngle = Vector3.SignedAngle(downVector, go.transform.forward, parallelVector);
                Vector3 lookRotVector = Quaternion.AngleAxis(forwardAngle, parallelVector)*downVector;

                go.transform.rotation = Quaternion.LookRotation(lookRotVector, upVector);
                go.transform.position = cpr.posVector;
            }
            
        }
    }
    void CalculateHeight()
    {
        height = cone.GetComponent<MeshRenderer>().bounds.size.y * targetConeScale/cone.transform.localScale.x;
    }
    void CalculateDiameter()
    {
        diameter = cone.GetComponent<MeshRenderer>().bounds.size.x * targetConeScale / cone.transform.localScale.x - offset;
    }
    void CalculateBottom()
    {
        bottom = cone.GetComponent<MeshRenderer>().bounds.min.y * targetConeScale / cone.transform.localScale.x + (offset * Mathf.Sqrt(2) / 2);
    }
    public CalculatePositionResponse CalculatePosition(Transform tr, float a)
    {
        float h = tr.position.y;
        float radius = ((h - bottom) / height) * diameter / 2;
        float x = cone.transform.position.x + radius * Mathf.Cos(a);
        float y = cone.transform.position.y + radius * Mathf.Sin(a);
        float dist = (tr.position - new Vector3(x, h, y)).magnitude;



        float radius1 = ((h-dist/2 - bottom) / height) * diameter / 2;
        float x1 = cone.transform.position.x + radius1 * Mathf.Cos(a);
        float y1 = cone.transform.position.y + radius1 * Mathf.Sin(a);

        CalculatePositionResponse cpr = new CalculatePositionResponse();
        cpr.posVector = new Vector3(x1, h - dist / 2, y1);

        cpr.crossVector = Vector3.Cross(cpr.posVector - new Vector3(cone.transform.position.x, 0, cone.transform.position.z), Vector3.up);
        return cpr;
    }
    public CalculatePositionResponse CalculatePosition(float h, float a)
    {
        float radius = ((h - bottom) / height) * diameter / 2;
        float x = cone.transform.position.x + radius * Mathf.Cos(a);
        float y = cone.transform.position.y + radius * Mathf.Sin(a);
        CalculatePositionResponse cpr = new CalculatePositionResponse();
        cpr.posVector = new Vector3(x, h, y);

        cpr.crossVector = Vector3.Cross(cpr.posVector - new Vector3(cone.transform.position.x, 0, cone.transform.position.z), Vector3.up);
        return cpr;
    }

    public float CalculateAngle(Vector3 position)
    {
        float ang = Vector3.SignedAngle((new Vector3(position.x, 0, position.z) - new Vector3(cone.transform.position.x, 0, cone.transform.position.z)), cone.transform.right, Vector3.up);
        ang = ang * Mathf.Deg2Rad;
        return ang;
    }
}
