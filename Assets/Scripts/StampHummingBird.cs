//====================================================================================================================================================================================================================================
//  Name:               StampHummingBird.cs
//  Author:             Matthew Mason
//  Date Created:       29/05/2022
//  Date Last Modified: 29/05/2022
//  Brief:              A child the humming bird agent that also deposits stamps after collecting them from flowers
//====================================================================================================================================================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// A child the humming bird agent that also deposits stamps after collecting them from flowers
/// </summary>
public class StampHummingBird : HummingBirdAgent
{
    #region Private Serialized Fields
    [SerializeField]
    [Tooltip("The DogClassifier used to identify new stamps collected")]
    private DogClassifier dogClassifier;

    [SerializeField]
    [Tooltip("The dog stamp factory this get stamps from")]
    private DogStampFactory stampFactory;

    [SerializeField]
    [Tooltip("How close the humming bird has to be to an object before it stop gain and losing reward for getting close")]
    private float desiredRewardStopProximity;

    [SerializeField]
    [Tooltip("The holder spawn to get the spawn stamp holders from")]
    private HolderSpawner holderSpawner;
    #endregion

    #region Private Variables
    /// <summary>
    /// The stamp that the agent is currently holding
    /// </summary>
    private DogStamp stampHeld;

    /// <summary>
    /// A dictionary of the dog breed type and the stamp holders store that kind of stamp
    /// </summary>
    private Dictionary<DogType, StampHolder> stampHolders;

    /// <summary>
    /// The value difference between the beak being in front dot product last check and the new check
    /// </summary>
    private float currentChangeBeakingInFrontDotProduct;
    /// <summary>
    /// The difference between the beak point towards reverse destination up dot product and last time it was checked 
    /// </summary>
    private float currentChangeBeakPointingDotProduct;
    /// <summary>
    /// The amount that distance between the objective and last time it was check and the current time
    /// </summary>
    private float currentChangeInRelativeDistance;
    /// <summary>
    /// The dot product value for the angle between the up of the destination and vector of the beak away from the destination
    /// </summary>
    private float lastBeakInFrontDotProduct;
    /// <summary>
    /// The dot product between the reverse direction of the destination up the last time it was checked
    /// </summary>
    private float lastBeakPointingDotProduct;
    /// <summary>
    /// The value of the distance to object whence last checked
    /// </summary>
    private float lastCheckDistanceToObjective;

    /// <summary>
    /// The destination the humming bird is trying to get too
    /// </summary>
    private Vector3 destination;
    /// <summary>
    /// The destination upward direction that it can be entered from 
    /// </summary>
    private Vector3 desinationUpVector;
    #endregion

    #region Unity Methods
    protected override void Update()
    {
        // Draw a line from the beak tip to the nearest flower
        if (nearestFlower != null)
        {
            Debug.DrawLine(beakTip.position, nearestFlower.FlowerCenterPosition, Color.green);
        }

        // If the nearest flower has no nectar then it should change to another flower
        // This would have likely happened if another humming bird had drained it first
        if (stampHeld == null)
        {
            if (nearestFlower == null)
            {
                UpdateNearestFlower();
            }
            else if (!nearestFlower.HasNectar)
            {
                UpdateNearestFlower();
            }
        }

    }

    protected void OnDrawGizmos()
    {
        if (Application.isPlaying && Application.isEditor)
        {
            // Draw the area of expected hovering
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetDestinationHoverPoint(), desiredRewardStopProximity);
            Gizmos.DrawLine(beakTip.position, beakTip.position + beakTip.forward);
            Gizmos.DrawLine(destination, destination + desinationUpVector);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        // If the stamp is not held its trying to get to flowers
        if (stampHeld == null)
        {
            destination = nearestFlower.FlowerCenterPosition;
            desinationUpVector = nearestFlower.FlowerUpVector;
        }
        else
        {
            if (stampHolders.TryGetValue(stampHeld.dogType, out StampHolder stampHolder))
            {
                destination = stampHolder.transform.position;
                desinationUpVector = stampHolder.transform.up;
            }
            else
            {
                Destroy(stampHeld.gameObject);
                destination = nearestFlower.FlowerCenterPosition;
                desinationUpVector = Vector3.up;
            }
        }
        // Get a vector from the beak tip to the nearest flower
        Vector3 toDestination = destination - beakTip.position;

        // Observe a normalized vector pointing to the nearest flower (3 observations)
        sensor.AddObservation(toDestination.normalized);

        float newBeakingInFrontDotProduct = Vector3.Dot(toDestination.normalized, -destination.normalized);
        currentChangeBeakingInFrontDotProduct = newBeakingInFrontDotProduct - lastBeakInFrontDotProduct;
        lastBeakInFrontDotProduct = newBeakingInFrontDotProduct;
        // Observe a dot product that indicate whether the beak tip is in front of the flower (1 observations)
        // (+1 means that the beak tip is directly in front of the flower, -1 means directly behind)
        sensor.AddObservation(newBeakingInFrontDotProduct);

        float newBeakPointingDotProduct = Vector3.Dot(beakTip.forward.normalized, -desinationUpVector.normalized);
        currentChangeBeakPointingDotProduct = newBeakPointingDotProduct - lastBeakPointingDotProduct;
        lastBeakPointingDotProduct = newBeakPointingDotProduct;
        // Observe a dot product that indicates whether the beak is point towards the flower (1 observations)
        // (+1 means that the beak is points directly at the flower, -1 means directly away
        sensor.AddObservation(newBeakPointingDotProduct);

        // Observe the relative distance from the beak tip to the flower (1 observations)
        float newReltiveDistance = toDestination.magnitude / FlowerArea.areaDiamter;
        currentChangeInRelativeDistance = newReltiveDistance - lastCheckDistanceToObjective;
        sensor.AddObservation(newReltiveDistance);
        lastCheckDistanceToObjective = newReltiveDistance;

        // Do the extra observations here
        // If a stamp is held
        sensor.AddObservation(stampHeld != null);

        // 11 Total Observation
    }
    public override void Initialize()
    {
        base.Initialize();
        // Set up dictionary of the stamp holders
        stampHolders = new Dictionary<DogType, StampHolder>();
        StampHolder[] stampHolderComponents = flowerArea.GetComponentsInChildren<StampHolder>();
        for (int i = 0; i < stampHolderComponents.Length; ++i)
        {
            stampHolders.Add(stampHolderComponents[i].heldType, stampHolderComponents[i]);
        }
    }
    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        // Reward or punish the humming bird for getting closer to objective
        // Or reward it for angling its beak correctly 
        if (Vector3.Distance(GetDestinationHoverPoint(), beakTip.position) > desiredRewardStopProximity)
        {
            AddReward(-currentChangeInRelativeDistance * 0.001f);
        }
        else
        {
            AddReward(currentChangeBeakingInFrontDotProduct * 0.001f);
            AddReward(currentChangeBeakPointingDotProduct * 0.001f);
        }

        base.OnActionReceived(vectorAction);
    }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        // Remove stamp Held
        if (stampHeld != null)
        {
            GameObject.Destroy(stampHeld.gameObject);
            stampHeld = null;
        }

        // Reset the holders
        holderSpawner.SpawnHolders();
        if (stampHolders == null)
        {
            stampHolders = new Dictionary<DogType, StampHolder>();
        }
        else
        {
            stampHolders.Clear();
        }
        StampHolder[] stampHolderComponents = holderSpawner.GetAllHoldersInLevel();
        for (int i = 0; i < stampHolderComponents.Length; ++i)
        {
            stampHolders.Add(stampHolderComponents[i].heldType, stampHolderComponents[i]);
        }
    }
    #endregion

    #region Protected Methods
    protected override void OnTriggerEnterOrStay(Collider collider)
    {
        // Add a section about adding items to the collection box
        if (stampHeld != null)
        {
            if (collider.TryGetComponent<DropPointCollider>(out DropPointCollider dropCollider))
            {
                dropCollider.connectedStampHolder.DespositStamp(stampHeld);
                stampHeld = null;
                AddReward(0.1f);
                Debug.Log("Stamp Collected");
                UpdateNearestFlower();
            }
        }
        // If it is not holding a stamp it can collect another stamp from nectar 
        else
        {
            // Check if agent is colliding with nectar
            if (collider.CompareTag("nectar"))
            {
                Vector3 closestPointToBeakTip = collider.ClosestPoint(beakTip.position);

                // Check if the closest collision point is close to the beak tip
                // Note: a collision with anything but the beak tip should not count
                if (Vector3.Distance(beakTip.position, closestPointToBeakTip) < BeakTipRadius)
                {
                    // Look up the flower for this nectar collider
                    Flower flower = flowerArea.GetFlowerFromNectarCollider(collider);

                    // Attempt to take .01 nectar
                    // Note: this is per fixed timestep, meaning it happens every .02 seconds, or 50x per second
                    float nectarReceived = flower.Feed(0.1f);

                    // Keep track or nectar obtained
                    NectarObtained += nectarReceived;

                    if (trainingMode)
                    {
                        // Calculate reward for getting nectar
                        float bonus = .02f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, -nearestFlower.FlowerUpVector.normalized));
                        AddReward(.01f + bonus);
                    }

                    // If flower has be drained, collect at stamp and update the nearest flower
                    if (!flower.HasNectar)
                    {
                        // Spawn a stamp at the beak
                        stampHeld = stampFactory.CreateDogStamp();
                        stampHeld.transform.rotation = beakTip.rotation;
                        stampHeld.transform.position = beakTip.position;
                        stampHeld.transform.SetParent(beakTip);
                        nearestFlower = null;
                        Debug.Log("Flower Drained");
                    }
                }
            }
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Returns the point where the humming bird should be hovering in front of the destination before getting the stamp
    /// </summary>
    /// <returns>The point where the humming bird should be hovering in front of the destination before getting the stamp</returns>
    private Vector3 GetDestinationHoverPoint()
    {
        return destination + desinationUpVector * desiredRewardStopProximity * 0.9f;
    }
    #endregion
}
