using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogBreedIDer : MonoBehaviour
{
    [System.Serializable]
    public struct textureArray
    {
        public string dogName;
        public Texture2D[] texture2Ds;
    }
    public List<textureArray> allDogPictureOfBreed;

    public DogClassifier dogClassifier;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
}
