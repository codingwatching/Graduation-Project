﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cube : MonoBehaviour
{
    [SerializeField] public List<Cube> neighbors = new List<Cube>();
    [SerializeField] public Transform platform;
    [SerializeField] public PathRequester pathRequester;
    public float groundHeight;
    public float neighborMaxDistance = 1.1f;
    
    [SerializeField] public List<Path> _paths; // Dictionary<destination, Path>
    public void Reset()
    {
        neighbors.Clear();
        neighbors = GetNeighbors();

        pathRequester = FindObjectOfType<PathRequester>();
        platform = transform.Find("Platform");
        groundHeight = platform.position.y;

        _paths.Clear();
        _paths.AddRange(pathRequester.Request(this));
    }

    public void SetBlink(float intensity)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.material.SetFloat("_ColorIntensity", intensity);
    }

    public void StopBlink()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.material.SetFloat("_ColorIntensity", 0f);
    }

    public void ClearNeighbor() => neighbors.Clear();

    public List<Cube> GetNeighbors()
    {
        List<Cube> neighbors = new List<Cube>();
        Cube[] cubes = FindObjectsOfType<Cube>();
        foreach (var c in cubes)
            if (NeighborCondition(c))
                neighbors.Add(c);

        return neighbors;
    }

    public bool NeighborCondition(Cube candidate)
    {
        Vector2 registererPlanePos = new Vector2(candidate.transform.position.x, candidate.transform.position.z);
        Vector2 myPlanePos = new Vector2(transform.position.x, transform.position.z);

        return Vector2.Distance(registererPlanePos, myPlanePos) < neighborMaxDistance && 
            candidate != this;
    }

    public void ClearPaths() => _paths.Clear();

}