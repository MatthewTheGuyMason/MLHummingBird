//====================================================================================================================================================================================================================================
//  Name:               HummingBirdAgent.cs
//  Author:             Matthew Mason
//  Date Created:       29/05/2022
//  Date Last Modified: 29/05/2022
//  Brief:              A hummingbird Machine Learning Agent
//====================================================================================================================================================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

// Script based on unity tutorial located at: https://learn.unity.com/course/ml-agents-hummingbirds

/// <summary>
/// A hummingbird Machine Learning Agent
/// </summary>
public class HummingBirdAgent : Agent
{
    #region Protected Constants
    /// <summary>
    /// Maximum distance form the beak tip to accept nectar collision
    /// </summary>
    protected const float BeakTipRadius = 0.008f;
    /// <summary>
    /// Maximum angle that the bird can pitch up or down
    /// </summary>
    protected const float MaxPitchAngle = 80f;
    #endregion

    #region Public Variables
    [Tooltip("Whether this is training mode or game play mode")]
    public bool trainingMode;

    [Tooltip("Force to apply when moving")]
    public float moveForce = 2f;
    [Tooltip("Speed to pitch up or down")]
    public float pitchSpeed = 100f;
    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;
    #endregion

    #region Protected Serialized Fields
    [Tooltip("The Agents's camera")]
    [SerializeField]
    protected Camera agentCamrea;

    [Tooltip("the Flower area that the agent is in")]
    [SerializeField]
    protected FlowerArea flowerArea;

    [Tooltip("The rigidbody of the agent")]
    [SerializeField]
    protected Rigidbody birdRigidbody;

    [Tooltip("Transform at the tip of the beak")]
    [SerializeField]
    protected Transform beakTip;
    #endregion

    #region Protected Variables
    // Allows for smoother pitch changes
    protected float smoothPitchChange = 0f;

    // Allows for smoother yaw changes
    protected float smoothYawChange = 0f;

    // The nearest flower to the agent
    protected Flower nearestFlower;

    // Weather the agent is frozen (intentionality not flying)
    protected bool frozen = false;
    #endregion

    #region Public Properties
    /// <summary>
    /// The amount of nectar the agent as obtained this episode
    /// </summary>
    public float NectarObtained { get; protected set; }
    #endregion

    #region Unity Methods
    protected void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterOrStay(other);
    }

    protected void OnTriggerStay(Collider other)
    {
        OnTriggerEnterOrStay(other);
    }

    /// <summary>
    /// Called when the agent collides with something solid 
    /// </summary>
    /// <param name="collision">The collision info</param>
    protected void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && collision.collider.CompareTag("boundary"))
        {
            // Collided with the area boundary, give negative reward
            AddReward(-0.5f);
        }
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    protected virtual void Update()
    {
        // Draw a line from the beak tip to the nearest flower
        if (nearestFlower != null)
        {
            Debug.DrawLine(beakTip.position, nearestFlower.FlowerCenterPosition, Color.green);
        }

        // If the nearest flower has no nectar then it should change to another flower
        // This would have likely happened if another humming bird had drained it first
        if (nearestFlower == null)
        {
            UpdateNearestFlower();
        }
        else if (!nearestFlower.HasNectar)
        {
            UpdateNearestFlower();
        }
    }
    /// <summary>
    /// Called every 0.2 seconds
    /// </summary>
    protected void FixedUpdate()
    {
        if (nearestFlower != null)
        {
            if (nearestFlower != null && !nearestFlower.HasNectar)
            {
                UpdateNearestFlower();
            }
        }
    }

    protected void Start()
    {
        if (nearestFlower != null)
        {
            UpdateNearestFlower();
        }
    }
    #endregion

    #region Public Methods
    #region Overrides
    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        if (!TryCollectHummingbirdObservations(sensor))
        {
            sensor.AddObservation(new float[10]);
            return;
        }
    }
    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        if (birdRigidbody == null)
        {
            birdRigidbody = GetComponent<Rigidbody>();
        }
        if (flowerArea == null)
        {
            flowerArea = GetComponentInParent<FlowerArea>();
        }

        // if not training mode, no max step, play forever
        if (!trainingMode)
        {
            // Only reset flowers in training when there is one per area
            MaxStep = 0;
        }
    }
    /// <summary>
    /// Called when action is received from either player input or the neural network
    /// 
    /// vectorAction[i] represents
    /// Index 0: move vector X (+1 = right,     -1 = left)
    /// Index 1: move vector y (+1 = up,        -1 = down)
    /// Index 2: move vector z (+1 = forward,   -1 = backwards)
    /// Index 3: pitch angle (+1 = pitch up,    -1 = pitch down)
    /// Index 4: yaw angle (+1 = turn right,    -1 = turn left) 
    /// </summary>
    /// <param name="vectorAction">The actions to take</param>
    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        // Don't take actions if frozen
        if (frozen)
        {
            return;
        }

        ProcessForceMovementActions(vectorAction.ContinuousActions[0], vectorAction.ContinuousActions[1], vectorAction.ContinuousActions[2]);

        ProcessRoationActions(vectorAction.ContinuousActions[3], vectorAction.ContinuousActions[4]);
    }
    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            // Only reset flowers in training when there is one agent per area
            flowerArea.ResetFlowers();
        }

        // Reset nectar obtained
        NectarObtained = 0f;

        // Zero out velocities so that movement stops before a new episode begins
        birdRigidbody.velocity = Vector3.zero;
        birdRigidbody.angularVelocity = Vector3.zero;

        // Default to spawning in front of a flower
        bool inFrontOfFlower = true;
        if (trainingMode)
        {
            // Spawn in front of a flower 50% of the time during training
            inFrontOfFlower = UnityEngine.Random.value > 0.5f;
        }

        //move the agent to a new random position
        MoveToSafeRandomPosition(inFrontOfFlower);

        // Recalculate nearest flower now that the agent has moved
        UpdateNearestFlower();


    }
    /// <summary>
    /// When Behaviour type is set to "Heuristic Only" on the agent's behaviour Parameters,
    /// this function will be called. Its return values will be fed into
    /// <see cref="OnActionReceived(float[])"/> instead of a using the neural network
    /// </summary>
    /// <param name="actionsOut">And output action array</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Create placeholder for all movement/turning
        Vector3 forward = Vector3.zero;
        Vector3 left = Vector3.zero;
        Vector3 up = Vector3.zero;
        float pitch = 0f;
        float yaw = 0f;

        // Convert keyboards inputs to movement and turning
        // All values should be between -1 and +1

        // Forward/backwards
        if (Input.GetKey(KeyCode.W))
        {
            forward = transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            forward = -transform.forward;
        }

        // Left/right
        if (Input.GetKey(KeyCode.A))
        {
            left = -transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            left = transform.right;
        }

        // Up/Down
        if (Input.GetKey(KeyCode.E))
        {
            up = transform.up;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            up = -transform.up;
        }

        // Pitch up/down
        if (Input.GetKey(KeyCode.UpArrow))
        {
            pitch = -1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            pitch = 1f;
        }

        // Turn left/right
        if (Input.GetKey(KeyCode.RightArrow))
        {
            yaw = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            yaw = -1f;
        }

        // Combine the movement vectors and normalize
        Vector3 combined = (forward + left + up).normalized;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        // Combine the 3 movement value, pitch, and yaw to the actionsOut array
        continuousActions[0] = combined.x;
        continuousActions[1] = combined.y;
        continuousActions[2] = combined.z;
        continuousActions[3] = pitch;
        continuousActions[4] = yaw;
    }
    #endregion

    /// <summary>
    /// Prevent the agent from moving and taking actions
    /// </summary>
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = true;
        birdRigidbody.Sleep();
    }
    /// <summary>
    /// Resume the agent movement and actions
    /// </summary>
    public void UnFreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = false;
        birdRigidbody.WakeUp();
    }
    #endregion

    #region Protected methods
    /// <summary>
    /// Attempts to collect all the observation needed for any humming bird agent
    /// </summary>
    /// <param name="sensor">The sensor to add the observations too</param>
    /// <returns>True if observations were successfully added to the sensor</returns>
    protected bool TryCollectHummingbirdObservations(VectorSensor sensor)
    {
        // If the nearestFlower is null, observe and emoty array and return early
        if (nearestFlower == null)
        {
            return false;
        }

        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);
        // Get a vector from the beak tip to the nearest flower
        Vector3 toFlower = nearestFlower.FlowerCenterPosition - beakTip.position;

        // Observe a normalized vector pointing to the nearest flower (3 observations)
        sensor.AddObservation(toFlower.normalized);

        // Observe a dot product that indicate whether the beak tip is in front of the flower (1 observations)
        // (+1 means that the beak tip is directly in front of the flower, -1 means directly behind)
        sensor.AddObservation(Vector3.Dot(toFlower.normalized, -nearestFlower.FlowerCenterPosition.normalized));

        // Observe a dot product that indicates whether the beak is point towards the flower (1 observations)
        // (+1 means that the beak is points directly at the flower, -1 means directly away
        sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, -nearestFlower.FlowerUpVector.normalized));

        // Observe the relative distance from the beak tip to the flower (1 observations)
        sensor.AddObservation(toFlower.magnitude / FlowerArea.areaDiamter);

        // 10 Total Observation
        return true;
    }

    /// <summary>
    /// The process force movement action
    /// </summary>
    /// <param name="xMovementAction">The movement along the x axis</param>
    /// <param name="yMovementAction">The movement along the y axis</param>
    /// <param name="zMovementAction">The movement along the z axis</param>
    protected void ProcessForceMovementActions(float xMovementAction, float yMovementAction, float zMovementAction)
    {
        // Calculate movement vector 
        Vector3 move = new Vector3(xMovementAction, yMovementAction, zMovementAction);

        // Add force in the direction of the move vector
        birdRigidbody.AddForce(move * moveForce);
    }
    /// <summary>
    /// Processes the forces for the rotational movement
    /// </summary>
    /// <param name="pitchChange">The change around the X axis</param>
    /// <param name="yawChange">The change around the Y axis</param>
    protected void ProcessRoationActions(float pitchChange, float yawChange)
    {
        // Get the current rotation
        Vector3 rotationVector = transform.rotation.eulerAngles;

        // Calculate smooth rotation changes
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        // Calculate new pitch and yaw based on smoothed values
        // Clamp pitch and avoid flipping upside down
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

        // Apply the new rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
    /// <summary>
    /// Move the agent to a safe random position (i.e. does not collide with anything)
    /// If in front of flower, also point the beak at the flower
    /// </summary>
    /// <param name="inFrontOfFlower">Whether to chose a position in front of the flower</param>
    protected void MoveToSafeRandomPosition(bool inFrontOfFlower)
    {
        bool safePositionFound = false;
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // Until Safe Position found
        for (int attemptsRemaining = 100; attemptsRemaining > 0; --attemptsRemaining)
        {
            if (inFrontOfFlower)
            {
                // Pick a random flower
                Flower randomFlower = flowerArea.Flowers[UnityEngine.Random.Range(0, flowerArea.Flowers.Count)];

                // Position 10 to 20 cm in front of the flower
                float distanceFromFlower = UnityEngine.Random.Range(0.1f, 0.2f);
                potentialPosition = randomFlower.transform.position + randomFlower.FlowerUpVector * distanceFromFlower;

                // Point beak at flower (bird's head is centre of transform)
                Vector3 toFlower = randomFlower.FlowerCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);
            }
            else
            {
                // Pick a random height from the ground
                float height = UnityEngine.Random.Range(1.2f, 2.5f);

                // Pick a random radius from the centre of the area 
                float radius = UnityEngine.Random.Range(2f, 7f);

                // Pick a random direction rotated around the y axis
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);

                // Combine height, radius and direction to pick a potential position
                potentialPosition = flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;

                // Chose and set random starting pitch and yaw
                float pitch = UnityEngine.Random.Range(-60f, 60f);
                float yaw = UnityEngine.Random.Range(-180f, 180f);
                potentialRotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // Check to see if the agent will collide with anything
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);

            // Safe position has been found if no overlaps
            safePositionFound = colliders.Length == 0;
        }

        Debug.Assert(safePositionFound, "Could not find a safe position to spawn", this);

        // Set position and rotation
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }

    /// <summary>
    /// Update the nearest flower to the agent
    /// </summary>
    protected void UpdateNearestFlower()
    {
        float distanceToCurrentNearestFlower = Mathf.Infinity;
        foreach (Flower flower in flowerArea.Flowers)
        {
            if (nearestFlower == null && flower.HasNectar)
            {
                // No current nearest flower and this flower has nectar, so set to this flower
                nearestFlower = flower;
                distanceToCurrentNearestFlower = Vector3.Distance(nearestFlower.transform.position, beakTip.position);
            }
            else if (flower.HasNectar)
            {
                // Calculate distance to this flower and distance to the current nearest flower
                float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);

                // If current nearest flower is empty OR this flower is closer, update the nearest flower
                if (!nearestFlower.HasNectar || distanceToFlower < distanceToCurrentNearestFlower)
                {
                    nearestFlower = flower;
                    distanceToCurrentNearestFlower = Vector3.Distance(nearestFlower.transform.position, beakTip.position);
                }
            }
        }
    }

    /// <summary>
    /// Handles when the agent's collider enters or stays in a trigger collider
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerEnterOrStay(Collider collider)
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

                // If flower is empty, update the nearest flower
                if (!flower.HasNectar)
                {
                    UpdateNearestFlower();
                }
            }
        }
    }
    #endregion
}
