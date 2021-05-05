using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectTrackedPosition
{
    public Transform transform;
    public float distance = 10;
}

[System.Serializable]
public class EffectStaticPosition
{
    public Vector4 position;
    public float distance = 10;
}

[System.Serializable]
public class EffectWalkablePosition
{
    public Vector4 position;
    public float distance = 0;
    public float endDistance = 10;
}

public class ColourShaderUpdater : MonoBehaviour {

    public static ColourShaderUpdater instance;

    [SerializeField]
    private Transform m_player;

    [SerializeField]
    private bool m_drawGizmos = true;

    private int m_maxArraySize = 1000;

    [Space(10)]
    [SerializeField]
    private float m_playerYOffset = 1f;

    [Space(10)]

    [SerializeField]
    private List<EffectTrackedPosition> m_effectTrackedPositions;

    [SerializeField]
    private List<EffectStaticPosition> m_effectStaticPositions;

    [SerializeField]
    private List<EffectWalkablePosition> m_effectWalkablePosition;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // Finding the player object
        if (m_player == null)
            m_player = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Updating all static positions distances
        UpdateStaticPositions();

        // Updating shader values
        List<EffectWalkablePosition> effectPositions = new List<EffectWalkablePosition>();

        // Adding in tracked positions
        for (int i = 0; i < m_effectTrackedPositions.Count; ++i)
        {
            EffectWalkablePosition newPos = new EffectWalkablePosition();

            newPos.position = m_effectTrackedPositions[i].transform.position;
            newPos.distance = m_effectTrackedPositions[i].distance;

            effectPositions.Add(newPos);
        }

        // Adding in static positions
        for (int i = 0; i < m_effectStaticPositions.Count; ++i)
        {
            EffectWalkablePosition newPos = new EffectWalkablePosition();
            newPos.position = m_effectStaticPositions[i].position;
            newPos.distance = m_effectStaticPositions[i].distance;

            effectPositions.Add(newPos);
        }

        // Adding in walkable positions
        for (int i = 0; i < m_effectWalkablePosition.Count; ++i)
        {
            effectPositions.Add(m_effectWalkablePosition[i]);
        }

        // Defining array sizes
        int arraySize = effectPositions.Count;

        if (arraySize > m_maxArraySize)
            arraySize = m_maxArraySize;

        // Creating positions array and distances array
        List<Vector4> posArray = new List<Vector4>();
        List<float> distanceArray = new List<float>();

        for (int i = 0; i < effectPositions.Count; ++i)
        {
            posArray.Add(effectPositions[i].position);
            distanceArray.Add(effectPositions[i].distance);
        }

        // Updating globally
        if (arraySize > 0)
        {
            Shader.SetGlobalInt("_ArraySize", arraySize);
            Shader.SetGlobalVectorArray("_Positions", posArray);
            Shader.SetGlobalFloatArray("_Distances", distanceArray);
        }
    }

    private void UpdateStaticPositions()
    {
        for (int i = m_effectWalkablePosition.Count - 1; i >= 0; --i)
        { 
            // Get the distance from the position to the player with some offsets
            float distToPlayer = Vector3.Distance(m_player.position - (Vector3.up * m_playerYOffset), m_effectWalkablePosition[i].position);

            // If the player is close enough calculate the distance
            if (distToPlayer < m_effectWalkablePosition[i].endDistance)
            {
                m_effectWalkablePosition[i].distance = Mathf.Clamp(-distToPlayer + m_effectWalkablePosition[i].endDistance, m_effectWalkablePosition[i].distance, m_effectWalkablePosition[i].endDistance);
            }

            // If the distance is high enough then remove this node from the list in an attempt to reclaim some performance
            if (m_effectWalkablePosition[i].distance > m_effectWalkablePosition[i].endDistance * 0.75f)
            {
                EffectStaticPosition pos = new EffectStaticPosition();
                pos.position = m_effectWalkablePosition[i].position;
                pos.distance = m_effectWalkablePosition[i].distance;
                m_effectStaticPositions.Add(pos);
                m_effectWalkablePosition.Remove(m_effectWalkablePosition[i]);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!m_drawGizmos)
            return;

        for (int i = 0; i < m_effectStaticPositions.Count; ++i)
        {
            Gizmos.DrawSphere(m_effectStaticPositions[i].position, 0.5f);
        }

        for (int i = 0; i < m_effectWalkablePosition.Count; ++i)
        {
            Gizmos.DrawSphere(m_effectWalkablePosition[i].position, 0.5f);
        }
    }
}
