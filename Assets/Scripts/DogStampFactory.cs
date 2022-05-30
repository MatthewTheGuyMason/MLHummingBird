//====================================================================================================================================================================================================================================
//  Name:               DogStampFactory.cs
//  Author:             Matthew Mason
//  Date Created:       30/05/2022
//  Date Last Modified: 30/05/2022
//  Brief:              Factory pattern script used to produce dog stamps for the stamp humming bird
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory pattern script used to produce dog stamps for the stamp humming bird
/// </summary>
public class DogStampFactory : MonoBehaviour
{
    #region Public Structure
    /// <summary>
    /// Serializable Structure used to store picture textures matching a given dog type
    /// </summary>
    [System.Serializable]
    public struct DogPictures
    {
        public DogType dogType;
        public Texture2D[] dogPictures;
    }
    #endregion

    #region Public Variables
    /// <summary>
    /// The all the different dog pictures that can be produced
    /// </summary>
    public List<DogPictures> dogPicturesSets;
    /// <summary>
    /// The prefab used as a basis for all the dog stamps
    /// </summary>
    public DogStamp stampPrefab;
    #endregion

    #region Public Methods
    /// <summary>
    /// The main function for creating dog stamps
    /// </summary>
    /// <returns>The DogStamp component attached to a new GameObject set up as a DogStamp</returns>
    public DogStamp CreateDogStamp()
    {
        // Pick random dog picture
        int randomPictureSetIndex = Random.Range(0, dogPicturesSets.Count);
        Texture2D picture = dogPicturesSets[randomPictureSetIndex].dogPictures[Random.Range(0, dogPicturesSets[randomPictureSetIndex].dogPictures.Length)];
        DogStamp newStamp = GameObject.Instantiate(stampPrefab).GetComponent<DogStamp>();

        newStamp.SetPicture(dogPicturesSets[randomPictureSetIndex].dogType, picture);
        return newStamp;
    }
    #endregion
}
