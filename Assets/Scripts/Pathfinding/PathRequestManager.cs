using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour {

    Queue<PathResult> results = new Queue<PathResult>();

    static PathRequestManager instance = null;
    static public PathRequestManager Instance
    {
        get { return instance; }
    }
    
    Pathfinding pathfinding;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        pathfinding = GetComponent<Pathfinding>();
    }

    private void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock(results)
            {
                for (int i = 0; i < itemsInQueue; ++i)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequest a_request)
    {
        ThreadStart _threadStart = delegate
        {
            instance.pathfinding.FindPath(a_request, instance.FinishedProcessingPath);
        };
        _threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult a_result)
    {
        lock (results)
        {
            results.Enqueue(a_result);
        }
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] a_path, bool a_success, Action<Vector3[], bool> a_callback)
    {
        this.path = a_path;
        this.success = a_success;
        this.callback = a_callback;
    }
}

public class PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callBack;

    public PathRequest(Vector3 _Start, Vector3 _End, Action<Vector3[], bool> _CallBack)
    {
        pathStart = _Start;
        pathEnd = _End;
        callBack = _CallBack;
    }
}