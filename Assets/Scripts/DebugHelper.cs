using UnityEngine;
using System;

public class DebugHelper : MonoBehaviour
{
    [SerializeField]
    private bool _paths;
    [SerializeField]
    private bool _sightRanges;
    [SerializeField]
    private bool _gunSoundRange;

    public static bool paths;
    public static bool sightPaths;
    public static bool gunSoundRange;

    void OnValidate()
    {
        paths = _paths;
        sightPaths = _sightRanges;
        gunSoundRange = _gunSoundRange;
    }
}
