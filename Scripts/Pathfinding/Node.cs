using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Nodes will be used for calculating the pathfinding
public class Node : IHeapItem<Node>
{

    // Can we traverse this node
    public bool walkable;
    // The position in the world
    public Vector3 worldPosition;

    public int gridX, gridY;

    public int gCost, hCost;
    public int fCost
    {
        get { return gCost + hCost; }
    }

    public Node parent;

    int heapIndex;

    // Node constructor
    public Node(bool a_walkable, Vector3 a_worldPos, int a_gridX, int a_gridY)
    {
        walkable = a_walkable;
        worldPosition = a_worldPos;
        gridX = a_gridX;
        gridY = a_gridY;
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int CompareTo(Node a_nodeToCompare)
    {
        int compare = fCost.CompareTo(a_nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(a_nodeToCompare.hCost);
        }
        return -compare;
    }
}
