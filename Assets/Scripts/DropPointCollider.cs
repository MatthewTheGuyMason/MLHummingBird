//====================================================================================================================================================================================================================================
//  Name:               DropPointCollider.cs
//  Author:             Matthew Mason
//  Date Created:       30/05/2022
//  Date Last Modified: 30/05/2022
//  Brief:              Script for marking an object a drop point collider and the stamp holder it connects to
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for marking an object a drop point collider and the stamp holder it connects to
/// </summary>
public class DropPointCollider : MonoBehaviour
{
    /// <summary>
    /// The stamp holder that this is the drop point for
    /// </summary>
    public StampHolder connectedStampHolder;
}
