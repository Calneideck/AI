using UnityEngine;

public class DebugHelper : MonoBehaviour
{
    [SerializeField]
    private bool _paths;
    [SerializeField]
    private bool _sightRanges;
    [SerializeField]
    private bool _gunSoundRange;
    [SerializeField]
    private bool _wander;

    public static bool paths;
    public static bool sightPaths;
    public static bool gunSoundRange;
    public static bool wander;

    void OnValidate()
    {
        paths = _paths;
        sightPaths = _sightRanges;
        gunSoundRange = _gunSoundRange;
        wander = _wander;
    }
}
