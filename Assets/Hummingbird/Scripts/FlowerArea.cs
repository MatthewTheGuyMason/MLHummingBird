//====================================================================================================================================================================================================================================
//  Name:               FlowerArea.cs
//  Author:             Matthew Mason
//  Date Created:       29/05/2022
//  Date Last Modified: 29/05/2022
//  Brief:              Manages a collection of flower plants and attached flowers
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script based on unity tutorial located at: https://learn.unity.com/course/ml-agents-hummingbirds

/// <summary>
/// Manages a collection of flower plants and attached flowers
/// </summary>
public class FlowerArea : MonoBehaviour
{
    #region Public Constants
    // The diameter of the area where the agent and the flower can be
    // used for observing the relative distance from agent to the flower
    public const float areaDiamter = 20f;
    #endregion

    #region Private Methods
    /// <summary>
    /// A lookup dictionary for looking up a flower from a nectar collider
    /// </summary>
    private Dictionary<Collider, Flower> nectarFlowerDictionary;

    /// <summary>
    /// The list of all flower plants in this flower area (flower plants have multiple flowers)
    /// </summary>
    private List<GameObject> flowerPlants;
    #endregion

    #region Public Properties
    /// <summary>
    /// The flowers inside the flower area
    /// </summary>
    public List<Flower> Flowers { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Initialize variables
        flowerPlants = new List<GameObject>();
        nectarFlowerDictionary = new Dictionary<Collider, Flower>();
        Flowers = new List<Flower>();
    }

    private void Start()
    {
        // Find all flowers that are children of this GameObject/Transform
        FindChildFlowers(transform);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Gets the <see cref="Flower"/> that a nectar collider belongs to
    /// </summary>
    /// <param name="nectarCollider">The nectar collider</param>
    /// <returns>The matching flower</returns>
    public Flower GetFlowerFromNectarCollider(Collider nectarCollider)
    {
        return nectarFlowerDictionary[nectarCollider];
    }

    /// <summary>
    /// Resets all the flowers in area ready for the next episode or round
    /// </summary>
    public void ResetFlowers()
    {
        // Rotate each flower plant around the Y Axis and subtly around the X and Z
        foreach (GameObject flowerPlant in flowerPlants)
        {
            float xRotation = UnityEngine.Random.Range(-5f, 5f);
            float yRotation = UnityEngine.Random.Range(-180f, 180f);
            float zRotation = UnityEngine.Random.Range(-5f, 5f);
            flowerPlant.transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        }

        // Reset each flower
        foreach (Flower flower in Flowers)
        {
            flower.ResetFlower();
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Recursively finds all the flowers and flower plants that are children of a parent transform 
    /// </summary>
    /// <param name="parent">The parent of the children to check</param>
    private void FindChildFlowers(Transform parent)
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild(i);
            if (child.CompareTag("flower_plant"))
            {
                // Found a flower plane, add it to the flowerPlants list
                flowerPlants.Add(child.gameObject);

                // Look for flowers within the flower plant
                FindChildFlowers(child);
            }
            else
            {
                // Not a flower plant, look for a Flower component
                if (child.TryGetComponent<Flower>(out Flower flower))
                {
                    // Found a flower, add it to the Flower list
                    Flowers.Add(flower);

                    // Add the nectar collider to the lookup dictionary
                    nectarFlowerDictionary.Add(flower.necterCollider, flower);

                    // Note: there are no flowers that are children of other flowers
                }
                else
                {
                    // Flower component not found so check children
                    FindChildFlowers(child);
                }
            }
        }
    }
    #endregion
}
