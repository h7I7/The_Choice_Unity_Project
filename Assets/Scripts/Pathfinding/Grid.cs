using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

    public static Grid instance;

    public bool displayGridGizmos = false;
    public LayerMask walkableMask;
    public Vector2 gridWorldSize;
    public Vector3 gridWorldPosition;
    public float nodeRadius;
    public float gridHeightOffset;

    // This is going to be used like a two dimensional array however I prefer to have it with just one dimension to speed things up a little
    // I don't suppose you would ever see large improvements on modern machines however it makes me feel better
    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Awake()
    {
        OnValidate();
    }

    public void OnValidate()
	{
        if (instance == null)
            instance = this;

		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		CreateGrid();
	}

	public int maxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    public void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBotomLeft = gridWorldPosition - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        worldBotomLeft.y += gridHeightOffset;

        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                Vector3 worldPoint = worldBotomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = (Physics.CheckBox(worldPoint, new Vector3(nodeRadius * 0.5f, nodeRadius * 0.5f, nodeRadius * 0.5f), Quaternion.identity, walkableMask));
                    grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public List<Node> GetNeighbours(Node a_node)
    {
        List<Node> neighbours = new List<Node>();

        // Cross pathfinding
        for (int x = -1; x <= 1; ++x)
        {
            int checkX = a_node.gridX + x;

            if (checkX >= 0 && checkX < gridSizeX)
                neighbours.Add(grid[checkX, a_node.gridY]);
        }

        for (int y = -1; y <= 1; ++y)
        {
            int checkY = a_node.gridY + y;

            if (checkY >= 0 && checkY < gridSizeY)
                neighbours.Add(grid[a_node.gridX, checkY]);
        }

        return neighbours;

        // Square pathfinding
        //for (int x = -1; x <= 1; ++x)
        //{
        //    for (int y = -1; y <= 1; ++y)
        //    {
        //        if (x == 0 && y == 0)
        //            continue;
        //
        //        int checkX = a_node.gridX + x;
        //        int checkY = a_node.gridY + y;
        //
        //        if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
        //        {
        //            neighbours.Add(grid[checkX, checkY]);
        //        }
        //    }
        //}
        //
        //return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 a_worldPos)
    {
        float percentX = (a_worldPos.x - gridWorldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (a_worldPos.z - gridWorldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + gridWorldPosition, new Vector3(gridWorldSize.x, 1f, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

}
