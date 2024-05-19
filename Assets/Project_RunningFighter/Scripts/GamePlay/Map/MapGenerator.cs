using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Generation")]
    [Header("Prefabs")]
    [SerializeField] private FloorModel startPrefab;
    [SerializeField] private FloorModel middlePrefab;
    [SerializeField] private FloorModel endPrefab;

    [Header("Map Generation Settings")]
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        float currentZ = startPosition.position.z;

        // Generate Start
        CreateFloorSegment(startPrefab, startPosition.position, ref currentZ);

        // Generate Middle
        while (currentZ < endPosition.position.z)
        {
            Vector3 nextPosition = new Vector3(startPosition.position.x, startPosition.position.y, currentZ);
            CreateFloorSegment(middlePrefab, nextPosition, ref currentZ);
        }

        // Generate End
        CreateFloorSegment(endPrefab, endPosition.position, ref currentZ);
    }

    private void CreateFloorSegment(FloorModel prefab, Vector3 position, ref float currentZ)
    {
        FloorModel floorSegment = Instantiate(prefab, position, Quaternion.Euler(0, 90, 0));
        floorSegment.transform.SetParent(transform);
        currentZ += floorSegment.length_of_floor();
    }
}
