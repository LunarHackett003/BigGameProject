using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Clamping
{
    public static Vector2 ClampThis(this Vector2 value, Vector2 min, Vector2 max)
    {
        return new Vector2(value.x.ClampThis(min.x, max.x), value.y.ClampThis(min.y, max.y));
    }
    public static float ClampThis(this float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }
    public static Vector3 ClampThis(this Vector3 value, Vector3 min, Vector3 max)
    {
        return new Vector3(
            value.x.ClampThis(min.x, max.x),
            value.y.ClampThis(min.y, max.y),
            value.z.ClampThis(min.z, max.z));
    }
}
