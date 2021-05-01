using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    [SerializeField]
    private Transform origin = null;

    public Transform target;
    [SerializeField]
    private float pathUpdateDistance;
    [SerializeField]
    private float waypointCompletionDistance = 0.2f;
    Vector3 targetOldPos;
    [SerializeField]
    private float moveDirectlyToTargetThreshold = 1.5f;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float turnSpeed;

    public bool stopMoving = false;

    private Vector3[] path;
    public Vector3[] Path
    {
        get { return path; }
    }

    int targetIndex;

    void Awake()
    {
        if (origin == null)
        {
            origin = transform;
        }
    }

    void Start()
    {
        targetIndex = 0;
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
        targetOldPos = target.transform.position;
    }

	private void Update()
	{
        Vector3 pos = target.position;

        // Looking towards position
        Quaternion dir = transform.rotation;
        pos.y = transform.position.y;
        dir.SetLookRotation((pos - transform.position).normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, dir, turnSpeed * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, target.position);

        // If we get too close to the target then we don't need to follow a path we can just move directly towards it
        if (dist < moveDirectlyToTargetThreshold && dist > 1.5f)
        {
            MoveDirectlyTowards(target.position);
        }

        // If there is no path then we need to request one
        if (path != null)
        {
            if (path.Length == 0)
            {
                targetIndex = 0;
                PathRequestManager.RequestPath(new PathRequest(origin.position, target.position, OnPathFound));
                return;
            }
        }

        if (pathUpdateDistance > 0)
        {
            // If the target strays too far away from the end of the current path then we need a new one
            if (Vector3.Distance(targetOldPos, target.transform.position) > pathUpdateDistance)
            {
                targetIndex = 0;
                PathRequestManager.RequestPath(new PathRequest(origin.position, target.position, OnPathFound));
                targetOldPos = target.transform.position;
            }
        }
    }

    public void UpdatePath()
    {
        PathRequestManager.RequestPath(new PathRequest(origin.position, target.position, OnPathFound));
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccess)
    {
        if (pathSuccess)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    private void MoveDirectlyTowards(Vector3 a_trans)
    {
        if (stopMoving)
            return;

        Vector3 pos = a_trans;

        // Moving towards position
        transform.position = Vector3.MoveTowards(transform.position, pos, speed * Time.deltaTime);
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWayPoint = target.position;

        if (path.Length != 0)
        {
            currentWayPoint = path[0];
        }

        while (true)
        {
            if (!stopMoving)
            {
                Vector3 currentWP = currentWayPoint;
                currentWP.y = transform.position.y;
                if (Vector3.Distance(transform.position, currentWP) <= waypointCompletionDistance)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }
                    currentWayPoint = path[targetIndex];
                }

                MoveDirectlyTowards(currentWayPoint);
            }

            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            Vector3 gizmoSize = new Vector3(Grid.instance.nodeRadius, Grid.instance.nodeRadius, Grid.instance.nodeRadius);

            for (int i = targetIndex; i < path.Length; ++i)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], gizmoSize);
                
                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
