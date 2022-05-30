//====================================================================================================================================================================================================================================
//  Name:               DogBreedIDer.cs
//  Author:             Matthew Mason
//  Date Created:       29/05/2022
//  Date Last Modified: 29/05/2022
//  Brief:              Script for figuring out what ID produce by the an ONNX classifier for each dog breed
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for figuring out what ID produce by the an ONNX classifier for each dog breed
/// </summary>
public class DogBreedIDer : MonoBehaviour
{
    #region Public Structures
    /// <summary>
    /// Structure for storing the a list of dogs texture and the name of the dog breed in the texture
    /// </summary>
    [System.Serializable]
    public struct DogTextureArray
    {
        public string dogName;
        public Texture2D[] texture2Ds;
    }
    #endregion

    #region Public Variables
    /// <summary>
    /// All the different array of dog pictures and their names
    /// </summary>
    public List<DogTextureArray> allDogPictureOfBreed;

    /// <summary>
    /// The Dog Classifier to use to get ID of the dog breed to compare to its name
    /// </summary>
    public DogClassifier dogClassifier;
    #endregion

    #region Unity Methods
    // Update is called once per frame
    void Update()
    {
        // When the user presses space, Check through all the arrays and output the most common ID output by the god classifier for the array
        // along side the name of the dog breed the array contained images of
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int arrayNum = 0; arrayNum < allDogPictureOfBreed.Count; ++arrayNum)
            {
                int[] dogIds = dogClassifier.FindMostCommanIntInTextureArray(allDogPictureOfBreed[arrayNum].texture2Ds);
                for (int i = 0; i < dogIds.Length; ++i)
                {
                    Debug.Log(allDogPictureOfBreed[arrayNum].dogName + " Dog ID is: " + dogIds[i]);
                }
            }
        }    
    }
    #endregion
}
