using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    [SerializeField] private List<Transform> nodes = new List<Transform>();
    private float totalPathLength = 0f;

    

    void Awake()
    {
        CalculatePathLength();
    }

    void OnValidate()
    {
        CalculatePathLength();
    }

    private void CalculatePathLength()
    {
        totalPathLength = 0f;

        if (nodes.Count < 2 || nodes == null) { return; }

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i] == null || nodes[i + 1] == null) { continue; }

            totalPathLength += Vector3.Distance(nodes[i].position, nodes[i + 1].position);
        }
    }

    public Vector3 GetPointAtDistance(float distance)
    {
        if (nodes.Count < 2 || nodes == null) { return Vector3.zero; }

        distance = Mathf.Clamp(distance, 0f, totalPathLength);
        float accumulatedDistance = 0f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i] == null || nodes[i + 1] == null) { continue; }
            float segmentLength = Vector3.Distance(nodes[i].position, nodes[i + 1].position);
            if (accumulatedDistance + segmentLength >= distance)
            {
                float t = (distance - accumulatedDistance) / segmentLength;
                return Vector3.Lerp(nodes[i].position, nodes[i + 1].position, t);
            }
            accumulatedDistance += segmentLength;
        }
        return nodes[nodes.Count - 1].position;
    }

    public Vector3 GetDirectionAtDistance(float distance)
    {
        if (nodes.Count < 2 || nodes == null) { return Vector3.zero; }
        distance = Mathf.Clamp(distance, 0f, totalPathLength);
        float accumulatedDistance = 0f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i] == null || nodes[i + 1] == null) { continue; }
            float segmentLength = Vector3.Distance(nodes[i].position, nodes[i + 1].position);
            if (accumulatedDistance + segmentLength >= distance)
            {
                return (nodes[i + 1].position - nodes[i].position).normalized;
            }
            accumulatedDistance += segmentLength;
        }
        return Vector3.zero;
    }

    public float GetDistanceAtNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= nodes.Count) { return 0f; }
        if (nodes.Count < 2 || nodes == null) { return 0f; }
        float distance = 0f;
        for (int i = 0; i < nodeIndex; i++)
        {
            if (nodes[i] == null || nodes[i + 1] == null) { continue; }
            distance += Vector3.Distance(nodes[i].position, nodes[i + 1].position);
        }
        return distance;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i] == null || nodes[i + 1] == null) { continue; }
            Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
        }

        Gizmos.color = Color.blue;
        foreach (var node in nodes)
        {
            if (node == null) { continue; }
            Gizmos.DrawSphere(node.position, 0.2f);
        }
    }

    public float GetTotalPathLength()
    {
        return totalPathLength;
    }

    public int GetNodeCount()
    {
        return nodes.Count;
    }

}
