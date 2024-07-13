using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraExtension
{
    public static void GetViewMinMax(this Camera cam, out Vector3 min, out Vector3 max)
    {
        Transform trans = cam.transform;
        min = Vector3.zero;
        max = Vector3.zero;

        float camWidth = cam.aspect * cam.orthographicSize;

        min.x = trans.position.x - camWidth;
        max.x = trans.position.x + camWidth;

        min.y = trans.position.y - cam.orthographicSize;
        max.y = trans.position.y + cam.orthographicSize;
    }
}
