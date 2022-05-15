using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogStamp : MonoBehaviour
{
    public BoxCollider stampCollider;

    [SerializeField]
    private Renderer[] faceRenderers;

    [SerializeField]
    private Material stampMat;

    [field: SerializeField]
    public DogType dogType
    {
        private set;
        get;
    }

    [field: SerializeField]
    public Texture2D picture
    {
        private set;
        get;
    }

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
}

