using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class StampHummingBird : HummingBirdAgent
{
    /// <summary>
    /// The stamp the agent is currently holding
    /// </summary>
    private DogStamp stampHeld;

    private Dictionary<DogType, StampHolder> stampHolders; 

    [SerializeField]
    private DogStampFactory stampFactory;

    [SerializeField]
    private HolderSpawner holderSpawner;

    [SerializeField]
    private DogClassifier dogClassifier;

    public override void Initialize()
    {
        base.Initialize();
        stampHolders = new Dictionary<DogType, StampHolder>();
        StampHolder[] stampHolderComponents = flowerArea.GetComponentsInChildren<StampHolder>();
        for (int i = 0; i < stampHolderComponents.Length; ++i)
        {
            stampHolders.Add(stampHolderComponents[i].heldType, stampHolderComponents[i]);
        }
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        Vector3 desination;// = Vector3.zero;
        Vector3 desinationUpVector;// = Vector3.up;
        // If the stamp is not held its trying to get to flowers
        if (stampHeld == null)
        {
            desination = nearestFlower.FlowerCenterPosition;
            desinationUpVector = Vector3.up;
        }
        else
        {
            if (stampHolders.TryGetValue(stampHeld.dogType, out StampHolder stampHolder))
            {
                desination = stampHolder.transform.position;
                desinationUpVector = stampHolder.transform.up;
            }
            else
            {
                Destroy(stampHeld.gameObject);
                desination = nearestFlower.FlowerCenterPosition;
                desinationUpVector = Vector3.up;
            }
        }
        // Get a vector from the beak tip to the nearest flower
        Vector3 toDestination = desination - beakTip.position;

        // Observe a normalized vector pointing to the nearest flower (3 observations)
        sensor.AddObservation(toDestination.normalized);

        // Observe a dot product that indicate whether the beak tip is in front of the flower (1 observations)
        // (+1 means that the beak tip is directly in front of the flower, -1 means directly behind)
        sensor.AddObservation(Vector3.Dot(toDestination.normalized, -desination.normalized));

        // Observe a dot product that indicates whether the beak is point towards the flower (1 observations)
        // (+1 means that the beak is points directly at the flower, -1 means directly away
        sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, -desinationUpVector.normalized));

        // Observe the relative distance from the beak tip to the flower (1 observations)
        sensor.AddObservation(toDestination.magnitude / FlowerArea.areaDiamter);

        // Do the extra observations here
        // If a stamp is held
        sensor.AddObservation(stampHeld != null);

        // 11 Total Observation
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

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        //float distanceToHolder = 0f;
        //if (stampHeld != null)
        //{
        //    distanceToHolder = Vector3.Distance(transform.position, stampHolder.transform.position);
        //}

        base.OnActionReceived(vectorAction);

        //// Reward the agent for moving closer to the stamp holder if it is holding a stamp
        //if (stampHeld != null)
        //{
        //    AddReward((distanceToHolder - Vector3.Distance(transform.position, stampHolder.transform.position)) * 0.0001f);
        //}
    }

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
}
