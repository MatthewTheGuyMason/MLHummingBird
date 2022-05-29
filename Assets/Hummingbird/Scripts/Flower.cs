//====================================================================================================================================================================================================================================
//  Name:               Flower.cs
//  Author:             Matthew Mason
//  Date Created:       29/05/2022
//  Date Last Modified: 29/05/2022
//  Brief:              Manages a single flower with nectar
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script based on unity tutorial located at: https://learn.unity.com/course/ml-agents-hummingbirds

/// <summary>
/// Manages a single flower with nectar
/// </summary>
public class Flower : MonoBehaviour
{
    #region Public Variables
    [Tooltip("The trigger collider representing the nectarCollider")]
    public Collider necterCollider;

    [Tooltip("Color when the flower is full")]
    public Color fullFlowerColor = new Color(1f, 0f, 0.3f);
    [Tooltip("Color when the flower is empty")]
    public Color emptyFlowerColor = new Color(0.5f, 0f, 1f);
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    [Tooltip("The solid collider representing the flower petals")]
    private Collider flowerCollider;
    #endregion

    #region Private Variables
    /// <summary>
    /// The flower's material
    /// </summary>
    private Material flowerMaterial;
    #endregion

    #region Public Properties
    /// <summary>
    /// Whether the flower has any nectar left remaining
    /// </summary>
    public bool HasNectar
    {
        get
        {
            return NectarAmount > 0f;
        }
    }

    /// <summary>
    /// The amount of nectar remaining in the flower
    /// </summary>
    public float NectarAmount { get; private set; }

    /// <summary>
    /// The centre position of the nectar collider
    /// </summary>
    public Vector3 FlowerCenterPosition
    {
        get
        {
            return necterCollider.transform.position;
        }
    }
    /// <summary>
    /// A vector pointing straight out of the flower
    /// </summary>
    public Vector3 FlowerUpVector
    {
        get
        {
            if (necterCollider)
            {
                return necterCollider.transform.up;
            }
            else
            {
                return transform.up;
            }    
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Find when the flower's mesh renderer and get the main material
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        flowerMaterial = meshRenderer.material;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Attempts to remove nectar from the flower
    /// </summary>
    /// <param name="amount">The amount of nectar to remove</param>
    /// <returns>The actual amount successfully removed</returns>
    public float Feed(float amount)
    {
        // Track how much nectar was successfully taken
        float necterTaken = Mathf.Clamp(amount, 0f, NectarAmount);

        NectarAmount -= amount;

        if (!HasNectar)
        {
            // No nectar Remaining
            NectarAmount = 0f;

            // Disable the flower and nectar colliders
            flowerCollider.gameObject.SetActive(false);
            necterCollider.gameObject.SetActive(false);

            // Change the flower color to indicate that it is empty
            flowerMaterial.SetColor("_BaseColor", emptyFlowerColor);
        }

        // Return the amount of nectar that was taken
        return necterTaken;
    }

    /// <summary>
    /// Resets the flower
    /// </summary>
    public void ResetFlower()
    {
        // Refill the nectar
        NectarAmount = 1f;

        // Enabled flower and nectar colliders
        flowerCollider.gameObject.SetActive(true);
        necterCollider.gameObject.SetActive(true);

        // Change the flower color to indicate that it is full
        flowerMaterial.SetColor("_BaseColor", fullFlowerColor);
    }
    #endregion
}
