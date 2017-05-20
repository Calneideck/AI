using UnityEngine;
using System;

public class DebugHelper : MonoBehaviour
{
    [SerializeField]
    private bool _paths;
    [SerializeField]
    private bool _sightRanges;

    public static bool paths;
    public static bool sightPaths;

    void OnValidate()
    {
        paths = _paths;
        sightPaths = _sightRanges;
    }
}
