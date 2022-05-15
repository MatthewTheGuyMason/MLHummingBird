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

    public DogStamp stampPrefab;

    public DogStamp CreateDogStamp()
    {
        // Pick random dog picture
        int randomPictureSetIndex = Random.Range(0, dogPicturesSets.Count);
        Texture2D picture = dogPicturesSets[randomPictureSetIndex].dogPictures[Random.Range(0, dogPicturesSets[randomPictureSetIndex].dogPictures.Length)];
        DogStamp newStamp = GameObject.Instantiate(stampPrefab).GetComponent<DogStamp>();

        newStamp.SetPicture(dogPicturesSets[randomPictureSetIndex].dogType, picture);
        return newStamp;
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateDogStamp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
