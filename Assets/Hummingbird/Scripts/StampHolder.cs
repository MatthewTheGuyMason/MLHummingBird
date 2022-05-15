using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StampHolder : MonoBehaviour
{
    [SerializeField]
    private Transform stampStackTransformBottom;

    public Collider placementTrigger;

    private Stack<DogStamp> stampStack;

    public DogType heldType; 

    [SerializeField]
    private Transform dropPointBottom;

    [SerializeField]
    private TextMeshPro textMesh;

    /// <summary>
    /// Position the new stamp at the top of the stack in the game world and in code
    /// </summary>
    /// <param name="despositedStamp"></param>
    public void DespositStamp(DogStamp despositedStamp)
    {
        if (despositedStamp.dogType != heldType)
        {
            Debug.Log("Incorrect Stamp type added");
            Destroy(despositedStamp.gameObject);
            return;
        }

        if (stampStack == null)
        {
            stampStack = new Stack<DogStamp>();
            despositedStamp.transform.position = stampStackTransformBottom.position + Vector3.up * despositedStamp.stampCollider.size.z * 0.5f;
            despositedStamp.transform.SetParent(stampStackTransformBottom);
            despositedStamp.transform.forward = Vector3.up;
            dropPointBottom.transform.position = despositedStamp.transform.position;
            stampStack.Push(despositedStamp);
        }
        else
        {
            DogStamp topStamp = stampStack.Peek();
            despositedStamp.transform.position = topStamp.transform.position + Vector3.up * topStamp.stampCollider.size.z * 0.5f + Vector3.up * despositedStamp.stampCollider.size.z * 0.5f;
            despositedStamp.transform.SetParent(stampStackTransformBottom);
            despositedStamp.transform.forward = Vector3.up;
            dropPointBottom.transform.position = despositedStamp.transform.position;
            stampStack.Push(despositedStamp);
        }   
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }
}
