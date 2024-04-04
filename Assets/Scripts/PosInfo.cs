using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosInfo : MonoBehaviour
{
    [SerializeField] private bool occupied;
    public Vector2 position;
    private GameObject posParent;

    private List<PosInfo> closePoints;
    private List<PosInfo> allPoints;

    private void Start()
    {
        posParent = GameObject.FindWithTag("Position Parent");
        closePoints = new List<PosInfo>();
        allPoints = new List<PosInfo>();
        posParent.GetComponentsInChildren(allPoints);
        /*foreach (var MARKER in posParent.GetComponentsInChildren<PosInfo>())
        {
            Debug.Log($"Found new PosInfo: {MARKER.name}");
        }*/
        
        position = transform.position;
        StartCoroutine(slowStart());
    }

    private IEnumerator slowStart()
    {
        yield return null;
        foreach (var POINT in allPoints)
        {
            if ((position - POINT.position).magnitude <= 1f)
            {
                closePoints.Add(POINT);
            }
        }
    }

    public void setOccupied(bool input)
    {
        occupied = input;
        foreach (var POINT in closePoints)
        {
            POINT.occupied = input;
        }
    }

    public bool getOccupied()
    {
        return occupied;
    }
}
