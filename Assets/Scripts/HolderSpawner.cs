using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolderSpawner : MonoBehaviour
{
    [SerializeField]
    private StampHolder StampHolderPrefab;

    [SerializeField]
    private FlowerArea flowerArea;

    [SerializeField]
    private MeshCollider islandBoundries;

    private void Awake()
    {
        // iterate through each value of the enum
        foreach (DogType enumValue in System.Enum.GetValues(typeof(DogType)))
        {
            // Pick a point at the top of the boundaries
            float yTop = islandBoundries.bounds.max.y;

            // Create the centre point at the top
            Vector3 centrePoint = new Vector3(islandBoundries.transform.position.x, yTop, islandBoundries.transform.position.z);

            // Pick a random direction
            Vector3 randomDirection = Random.onUnitSphere;
            randomDirection = new Vector3(randomDirection.x, 0f, randomDirection.z).normalized;

            // Move out in that direction
            Vector3 topPosition =  centrePoint + randomDirection * Random.Range(1f, FlowerArea.areaDiamter * 0.375f);

            // Ray-cast down to see if it can hit the floor
            if (Physics.Raycast(topPosition, Vector3.down, out RaycastHit hitPoint, FlowerArea.areaDiamter))
            {
                GameObject newHolder = GameObject.Instantiate(StampHolderPrefab.gameObject, hitPoint.point, StampHolderPrefab.transform.rotation, flowerArea.transform);
                StampHolder newStampHolder = newHolder.GetComponent<StampHolder>();
                newStampHolder.heldType = enumValue;
                newStampHolder.SetText(enumValue.ToString());
            }
        }
    }
}
