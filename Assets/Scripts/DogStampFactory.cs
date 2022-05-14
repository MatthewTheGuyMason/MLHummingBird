using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogStampFactory : MonoBehaviour
{
    // Turn this into serialized field for error mitigation latter

    [System.Serializable]
    public struct DogPictures
    {
        public DogType dogType;
        public Texture2D[] dogPictures;
    }

    public List<DogPictures> dogPicturesSets;

    public GameObject stampPrefab;

    public GameObject CreateDogStamp()
    {
        // Pick random dog picture
        int randomPictureSetIndex = Random.Range(0, dogPicturesSets.Count);
        Texture2D picture = dogPicturesSets[randomPictureSetIndex].dogPictures[Random.Range(0, dogPicturesSets[randomPictureSetIndex].dogPictures.Length)];
        //dogPicturesSets[randomPictureSetIndex].dogPictures()
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
