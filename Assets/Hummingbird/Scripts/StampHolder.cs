using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StampHolder : MonoBehaviour
{
    [SerializeField]
    private Transform stampStackTransformBottom;

    public Collider placementTrigger;

    private Stack<Stamp> stampStack;

    /// <summary>
    /// Position the new stamp at the top of the stack in the game world and in code
    /// </summary>
    /// <param name="despositedStamp"></param>
    public void DespositStamp(Stamp despositedStamp)
    {
        if (stampStack == null)
        {
            stampStack = new Stack<Stamp>();
            despositedStamp.transform.position = stampStackTransformBottom.position + Vector3.up * despositedStamp.stampCollider.size.y * 0.5f;
            despositedStamp.transform.SetParent(stampStackTransformBottom);
            stampStack.Push(despositedStamp);
        }
        else
        {
            Stamp topStamp = stampStack.Peek();
            despositedStamp.transform.position = topStamp.transform.position + Vector3.up * topStamp.stampCollider.size.y * 0.5f + Vector3.up * despositedStamp.stampCollider.size.y * 0.5f;
            despositedStamp.transform.SetParent(stampStackTransformBottom);
            stampStack.Push(despositedStamp);
        }   
    }
}
