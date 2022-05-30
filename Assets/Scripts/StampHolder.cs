//====================================================================================================================================================================================================================================
//  Name:               StampHolder.cs
//  Author:             Matthew Mason
//  Date Created:       30/05/2022
//  Date Last Modified: 30/05/2022
//  Brief:              A Bucket used to hold stamps of a certain type from the humming bird
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A Bucket used to hold stamps of a certain type from the humming bird
/// </summary>
public class StampHolder : MonoBehaviour
{
    #region Public Variables
    [Tooltip("The trigger for when the stamp can be placed")]
    public Collider placementTrigger;

    [Tooltip("The type of dog breed pictures the bucket can contain")]
    public DogType heldType;
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    [Tooltip("The transform marking where the stack starts ")]
    private Transform stampStackTransformBottom;

    [SerializeField]
    [Tooltip("The bottom of the area that stamps can be dropped off at")]
    private Transform dropPointBottom;

    [SerializeField]
    [Tooltip("The text mesh to use to display the dog breed name")]
    private TextMeshPro textMesh;

    [SerializeField]
    [Tooltip("The spacing distance between the stamps when place in a stack")]
    private float distanceBetweenStamp;
    #endregion

    #region Private Variables
    /// <summary>
    /// The stack of all the stamps placed within the holder
    /// </summary>
    private Stack<DogStamp> stampStack;
    #endregion

    #region Public Methods
    /// <summary>
    /// Position the new stamp at the top of the stack in the game world and in code
    /// </summary>
    /// <param name="despositedStamp"></param>
    public void DespositStamp(DogStamp despositedStamp)
    {
        // When the wrong stamp was added
        if (despositedStamp.dogType != heldType)
        {
            Debug.Log("Incorrect Stamp type added");
            Destroy(despositedStamp.gameObject);
            return;
        }

        // Set up the stamp and place it at the bottom of the pile
        if (stampStack == null)
        {
            stampStack = new Stack<DogStamp>();
            despositedStamp.transform.position = stampStackTransformBottom.position;
            despositedStamp.transform.SetParent(stampStackTransformBottom);
            despositedStamp.transform.forward = Vector3.up;
            dropPointBottom.transform.position = despositedStamp.transform.position;
            stampStack.Push(despositedStamp);
        }
        // Otherwise place the stamp on top of the pile
        else
        {
            DogStamp topStamp = stampStack.Peek();
            despositedStamp.transform.position = topStamp.transform.position + Vector3.up * distanceBetweenStamp;
            despositedStamp.transform.SetParent(stampStackTransformBottom);
            despositedStamp.transform.forward = Vector3.up;
            dropPointBottom.transform.position = despositedStamp.transform.position;
            stampStack.Push(despositedStamp);
        }   
    }

    /// <summary>
    /// Update the displayed text
    /// </summary>
    /// <param name="text">The next text to display</param>
    public void SetText(string text)
    {
        textMesh.text = text;
    }
    #endregion
}
