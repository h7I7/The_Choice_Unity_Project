using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AverageMeshNormals : MonoBehaviour {

    private List<MeshFilter> m_converted;

    private static readonly Vector3 zeroVec = Vector3.zero;

    private int m_objectCount;

    // Use this for initialization
    void Start() {
        // Initialise the list
        m_converted = new List<MeshFilter>();
        m_objectCount = 0;

        //yield return new WaitForSeconds(0.1f);

        // Convert all meshes of this object children
        ConvertMeshes(transform);

        Debug.Log("Averaged Mesh Normals\n" + "Total unique meshes calculated: " + m_converted.Count + ". Total meshes: " + m_objectCount);
    }

    private void ConvertMeshes(Transform a_meshObject)
    {
        // Check for object children, if so then convert their meshes too
        if (a_meshObject.childCount > 0)
        {
            for(int i = 0; i < a_meshObject.childCount; ++i)
            {
                ConvertMeshes(a_meshObject.GetChild(i));
            }
        }

        MeshFilter meshSource = a_meshObject.GetComponent<MeshFilter>();

        // Early out if this object doesn't have a mesh filter
        if (meshSource == null)
            return;

        m_objectCount++;

        // Check if an object with the same model has already been converted
        foreach (MeshFilter m in m_converted)
        {
            // If there is a model that has already been converted then set
            // this meshfilter to the converted one
            if (m.sharedMesh == meshSource.sharedMesh)
            {
                meshSource = m;
                return;
            }
        }

        Vector3[] verts = meshSource.sharedMesh.vertices;
        Vector3[] normals = meshSource.sharedMesh.normals;
        VertInfo[] vertInfo = new VertInfo[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            vertInfo[i] = new VertInfo()
            {
                vert = verts[i],
                origIndex = i,
                normal = normals[i]
            };
        }

        var groups = vertInfo.GroupBy(x => x.vert);
        VertInfo[] processedVertInfo = new VertInfo[vertInfo.Length];
        int index = 0;

        foreach (IGrouping<Vector3, VertInfo> group in groups)
        {
            Vector3 avgNormal = zeroVec;

            foreach (VertInfo item in group)
            {
                avgNormal += item.normal;
            }

            avgNormal = avgNormal / group.Count();

            foreach (VertInfo item in group)
            {
                processedVertInfo[index] = new VertInfo()
                {
                    vert = item.vert,
                    origIndex = item.origIndex,
                    normal = item.normal,
                    averagedNormal = avgNormal
                };

                index++;
            }
        }

        Color[] colors = new Color[verts.Length];

        for (int i = 0; i < processedVertInfo.Length; i++)
        {
            VertInfo info = processedVertInfo[i];

            int origIndex = info.origIndex;
            Vector3 normal = info.averagedNormal;
            Color normColor = new Color(normal.x, normal.y, normal.z, 1);
            colors[origIndex] = normColor;
        }

        meshSource.sharedMesh.colors = colors;

        meshSource.sharedMesh.RecalculateNormals();

        // Add the converted mesh to the converted list
        m_converted.Add(meshSource);
    } 

    private struct VertInfo
    {
        public Vector3 vert;
        public int origIndex;
        public Vector3 normal;
        public Vector3 averagedNormal;
    }
}
