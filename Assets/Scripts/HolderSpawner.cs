//====================================================================================================================================================================================================================================
//  Name:               HolderSpawner.cs
//  Author:             Matthew Mason
//  Date Created:       30/05/2022
//  Date Last Modified: 30/05/2022
//  Brief:              Script for spawning all the stamp holders for each type of dog randomly at the beginning of an episode
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for spawning all the stamp holders for each type of dog randomly at the beginning of an episode
/// </summary>
public class HolderSpawner : MonoBehaviour
{
    #region Private Serialized Fields
    [SerializeField] [Tooltip("The prefab for the spawned stampHolder")]
    private StampHolder stampHolderPrefab;

    [SerializeField] [Tooltip("The FlowerArea that the spawner will be contained within")]
    private FlowerArea flowerArea;

    [SerializeField] [Tooltip("The mesh collider containing the holders within")]
    private MeshCollider islandBoundries;
    #endregion

    #region Private Variables
    /// <summary>
    /// All the stamps holders spawned
    /// </summary>
    private List<StampHolder> stampHolders;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        stampHolders = new List<StampHolder>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns all the stamp holders with one flower area (not the whole scene)
    /// </summary>
    /// <returns>All the stamp holders with one flower area (not the whole scene)</returns>
    public StampHolder[] GetAllHoldersInLevel()
    {
        return stampHolders.ToArray();
    }

    /// <summary>
    /// Removes all holders from the level
    /// </summary>
    private void ClearHolders()
    {
        for (int i = 0; i < stampHolders.Count; ++i)
        {
            Destroy(stampHolders[i].gameObject);
        }
        stampHolders.Clear();
    }
    /// <summary>
    /// Spawns a new set of holders into the level
    /// </summary>
    public void SpawnHolders()
    {
        ClearHolders();

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
            Vector3 topPosition = centrePoint + randomDirection * Random.Range(1f, FlowerArea.areaDiamter * 0.375f);

            // Ray-cast down to see if it can hit the floor
            if (Physics.Raycast(topPosition, Vector3.down, out RaycastHit hitPoint, FlowerArea.areaDiamter))
            {
                GameObject newHolder = GameObject.Instantiate(stampHolderPrefab.gameObject, hitPoint.point, stampHolderPrefab.transform.rotation, flowerArea.transform);
                StampHolder newStampHolder = newHolder.GetComponent<StampHolder>();
                newStampHolder.heldType = enumValue;
                newStampHolder.SetText(enumValue.ToString());
                stampHolders.Add(newStampHolder);
            }
            else
            {
                Debug.LogError("Could not spawn holder for type " + enumValue.ToString());
            }
        }
    }
    #endregion
}
