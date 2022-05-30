//====================================================================================================================================================================================================================================
//  Name:               DogStamp.cs
//  Author:             Matthew Mason
//  Date Created:       30/05/2022
//  Date Last Modified: 30/05/2022
//  Brief:              The stamps that they stamp hamming bird agent will be delivering
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The stamps that they stamp hamming bird agent will be delivering
/// </summary>
public class DogStamp : MonoBehaviour
{
    #region Public Variables
    [Tooltip("The collider for the stamp area")]
    public BoxCollider stampCollider;
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    [Tooltip("The renders for each side of the stamp")]
    private Renderer[] faceRenderers;

    [SerializeField]
    [Tooltip("The material used in each of the stamps renders")]
    private Material stampMat;
    #endregion

    #region Serialized Publicly Retrievable Properties
    [field: SerializeField]
    [field: Tooltip("The type of dog the stamp represents")]
    public DogType dogType
    {
        private set;
        get;
    }

    [field: SerializeField]
    [field: Tooltip("Texture show on the stamp")]
    public Texture2D picture
    {
        private set;
        get;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the picture shown by the stamp and what type of dog it is
    /// </summary>
    /// <param name="dogType">The type of dog breed the new picture is of</param>
    /// <param name="picture">The new picture to show on the stamp</param>
    public void SetPicture(DogType dogType, Texture2D picture)
    {
        this.picture = picture;
        this.dogType = dogType;
        Material newPictureMat = new Material(stampMat);
        newPictureMat.mainTexture = picture;
        for (int i = 0; i < faceRenderers.Length; ++i)
        {
            faceRenderers[i].material = newPictureMat;
        }
    }
    #endregion
}

