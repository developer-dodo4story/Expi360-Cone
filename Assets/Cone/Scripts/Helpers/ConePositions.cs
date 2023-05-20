using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculatePositionResponse
{
    public Vector3 posVector;
    public Vector3 crossVector;
}

public class ConePositions : MonoBehaviour {

    [SerializeField]
    private Transform cone;
    public float angle;


    private float height;
    private float diameter;
    private float bottom;
    private float offset = 0;
    private float targetConeScale = 10;

    #region Boundaries
    private float buildingMinHeight = 0;
    private float buildingMaxHeight = 4000;
    private float buildingMaxOffsetAngle;
    private float buildingMaxOffsetScale;
    #endregion


    public static ConePositions Instance;
    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
        }
        CalculateHeight();
        CalculateDiameter();
        CalculateBottom();
    }
    void CalculateHeight()
    {
        height = cone.GetComponent<MeshRenderer>().bounds.extents.y;
        //height *= 10 / 10000;
    }
    void CalculateDiameter()
    {
        diameter = cone.GetComponent<MeshRenderer>().bounds.extents.x - offset;
        //diameter *= 10 / 10000;
    }
    void CalculateBottom()
    {
        bottom = cone.GetComponent<MeshRenderer>().bounds.min.y + (offset * Mathf.Sqrt(2) / 2);
        //bottom *= 10 / 10000;
    }
    public CalculatePositionResponse CalculatePosition(Transform tr, float objectAngle)
    {
        float h = tr.position.y;
        float radius = ((h - bottom) / height) * diameter / 2;
        float x = cone.transform.position.x + radius * Mathf.Cos(objectAngle);
        float y = cone.transform.position.y + radius * Mathf.Sin(objectAngle);
        float dist = (tr.position - new Vector3(x, h, y)).magnitude;

        float radius1 = ((h - dist / 2 - bottom) / height) * diameter / 2;
        float x1 = cone.transform.position.x + radius1 * Mathf.Cos(objectAngle);
        float y1 = cone.transform.position.y + radius1 * Mathf.Sin(objectAngle);

        CalculatePositionResponse cpr = new CalculatePositionResponse();
        cpr.posVector = new Vector3(x1, h - dist / 2, y1);
        cpr.crossVector = Vector3.Cross(cpr.posVector - new Vector3(cone.transform.position.x, 0, cone.transform.position.z), Vector3.up);

        return cpr;
    }

    public CalculatePositionResponse CalculatePosition(float height, float objectAngle)
    {
        float radius = ((height - bottom) / this.height) * diameter / 2;
        float x = cone.position.x + radius * Mathf.Cos(objectAngle);
        float y = cone.position.y + radius * Mathf.Sin(objectAngle);
        CalculatePositionResponse cpr = new CalculatePositionResponse();
        cpr.posVector = new Vector3(x, height, y);
        cpr.crossVector = Vector3.Cross(cpr.posVector - new Vector3(cone.position.x, 0, cone.position.z), Vector3.up);

        return cpr;
    }

    public float CalculateAngle(Vector3 position)
    {
        float ang = Vector3.SignedAngle((new Vector3(position.x, 0, position.z) - new Vector3(cone.position.x, 0, cone.position.z)), cone.right, Vector3.up);
        ang = ang * Mathf.Deg2Rad;
        return ang;
    }

    public float BuildingOffset(float height)
    {
        return 0.2f * (((buildingMaxHeight - height) / (buildingMaxHeight - buildingMinHeight)));
    }

    public float GetHeight()
    {
        return height;
    }
}
