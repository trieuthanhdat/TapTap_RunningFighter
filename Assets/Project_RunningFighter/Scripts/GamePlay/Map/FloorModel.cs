using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorModel : MonoBehaviour
{
    private MeshRenderer _meshRenderer; 
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    public float length_of_floor() {
        return _meshRenderer.bounds.size.z;
    }

    public float width_of_floor() {
        // return _meshRenderer.bounds.size.x * transform.localScale.x;
        return _meshRenderer.bounds.size.x;
    }

    public float height_of_floor() {
        // return _meshRenderer.bounds.size.y * transform.localScale.y;
        return _meshRenderer.bounds.size.y;
    }
}
