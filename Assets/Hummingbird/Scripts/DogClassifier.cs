//====================================================================================================================================================================================================================================
//  Name:               DogClassifier.cs
//  Author:             Matthew Mason
//  Date Created:       29/05/2022
//  Date Last Modified: 29/05/2022
//  Brief:              Class used to judge a texture or array of textures to identify what dog breed is most likely on in
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

/// <summary>
/// Class used to judge a texture or array of textures to identify what dog breed is most likely on in
/// </summary>
public class DogClassifier : MonoBehaviour
{
    #region Public Structures
    /// <summary>
    /// Structure containing data on the classification prediction for which dog breed a texture is of.
    /// Containing all the guess weights predicted by storing the highest ID for easy access
    /// </summary>
    [System.Serializable]
    public struct Prediction
    {
        public int predictedValue;
        public float[] predicted;

        public void SetPrediction(Tensor t)
        {
            predicted = t.AsFloats();
            predictedValue = System.Array.IndexOf(predicted, predicted.Max());
        }
    }
    #endregion

    #region Public Constants
    /// <summary>
    /// The number of channels used in a colour texture with no alpha
    /// </summary>
    public const int colourChannelCount = 3;
    /// <summary>
    /// The value Given to an invalid prediction ID
    /// </summary>
    public const int InvalidPredictionID = -1;
    #endregion

    #region Public Variables
    [Tooltip("The height in pixel that the ONNX file expect the texture to be when classifying them")]
    public int expectedTextureHeight;
    [Tooltip("The width in pixel that the ONNX file expect the texture to be when classifying them")]
    public int expectedTextureWidth;

    [Tooltip("The ONNX model to use to classify the dog breeds")]
    public NNModel modelAsset;
    #endregion

    #region Private Variables
    /// <summary>
    /// The worker running the runtime model
    /// </summary>
    private IWorker worker;
    /// <summary>
    /// Run time model created from the ONNX file
    /// </summary>
    private Model runtimeModel;
    #endregion

    #region Unity Methods
    void Start()
    {
        // Set up the runtime model and worker
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.GPU);
    }

    private void OnDestroy()
    {
        // Properly dispose of the worker
        if (worker != null)
        {
            worker.Dispose();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Helper function that tests and texture for an a Dog Breed Type from a texture
    /// </summary>
    /// <param name="texture2DToTest">The texture fed into ONNX model to get the predicted Dog Breed of</param>
    /// <returns>The dog type the texture is mostly of</returns>
    public DogType TestTextureForDogType(Texture2D texture2DToTest)
    {
        return (DogType)TestTextureForIntValue(texture2DToTest);
    }

    /// <summary>
    /// Finds the most common DogType within a set of dog image textures
    /// </summary>
    /// <param name="texture2DsToTest">The textures fed into ONNX model to get the most common dog breed ID from</param>
    /// <returns>The most common dog breed ID found within the array of textures, 
    /// will return multiple value if the prediction weight value was equal between multiple results</returns>
    public DogType[] FindMostCommanDogTypeInTextureArray(Texture2D[] texture2DsToTest)
    {
        Dictionary<int, int> predictionCount = new Dictionary<int, int>();
        int biggestCount = 0;
        List<DogType> currentBiggestPredictions = new List<DogType>();

        // Iterate over all the texture given
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            // Get the prediction for the current texture
            int currentPrediction = TestTextureForIntValue(texture2DsToTest[i]);
            // If the ID has been found before
            if (predictionCount.TryGetValue(currentPrediction, out int count))
            {
                // Add to the count of how many times that prediction has appeared
                int newCount = count + 1;
                predictionCount[currentPrediction] = newCount;
                // If the prediction count was bigger then the make it the most common entry
                if (newCount > biggestCount)
                {
                    biggestCount = newCount;
                    currentBiggestPredictions.Clear();
                    currentBiggestPredictions.Add((DogType)currentPrediction);
                }
                // Otherwise if it was joint biggest, then add it to the most common entries
                else if (newCount == biggestCount)
                {
                    currentBiggestPredictions.Add((DogType)currentPrediction);
                }
            }
            // Otherwise add it to the dictionary
            else
            {
                predictionCount.Add(currentPrediction, 1);
            }
        }
        // Return the most common entries
        return currentBiggestPredictions.ToArray();
    }
    /// <summary>
    /// Test a set of textures for their Dog Breed types
    /// </summary>
    /// <param name="texture2DsToTest">The textures fed into ONNX model to get the predicted Dog Breeds of</param>
    /// <returns>An array of Dog Breeds Types from all the different texture</returns>
    public DogType[] TestTextureForDogTypeArray(Texture2D[] texture2DsToTest)
    {
        // Iterate over each item and get it predicted value
        DogType[] predictedOutComes = new DogType[texture2DsToTest.Length];
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            predictedOutComes[i] = (DogType)TestTextureForIntValue(texture2DsToTest[i]);
        }
        return predictedOutComes;
    }

    /// <summary>
    /// Test a texture for an for its Dog Breed ID
    /// </summary>
    /// <param name="texture2DToTest">The texture fed into ONNX model to get the predicted ID of</param>
    /// <returns>An integer for the ID of dog breed that has the highest prediction weight</returns>
    public int TestTextureForIntValue(Texture2D texture2DToTest)
    {
        // Check if the image is valid to be put through the model
        if (texture2DToTest.width != expectedTextureWidth || texture2DToTest.height != expectedTextureHeight)
        {
            Debug.LogError("Texture was not of expected size when tested", this);
            return InvalidPredictionID;
        }

        // Making a tensor out of a Colour texture
        int channelCount = colourChannelCount; // 1 = Greyscale, 3 = colour, 4 colour + alpha 
        Tensor inputX = new Tensor(texture2DToTest, channelCount);

        // Run the worker with the tensor then dispose of it
        Tensor Output = worker.Execute(inputX).PeekOutput();
        inputX.Dispose();
        
        // Create the prediction
        Prediction typePrediction = new Prediction();
        typePrediction.SetPrediction(Output);
        return typePrediction.predictedValue;
    }

    /// <summary>
    /// Finds the most common ID within a set of dog image textures
    /// </summary>
    /// <param name="texture2DsToTest">The textures fed into ONNX model to get the most common dog breed ID from</param>
    /// <returns>The most common dog breed ID found within the array of textures, 
    /// will return multiple value if the prediction weight value was equal between multiple results</returns>
    public int[] FindMostCommanIntInTextureArray(Texture2D[] texture2DsToTest)
    {
        Dictionary<int, int> predictionCount = new Dictionary<int, int>();
        int biggestCount = 0;
        List<int> currentBiggestPredictions = new List<int>();

        // Iterate over all the texture given
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            // Get the prediction for the current texture
            int currentPrediction = TestTextureForIntValue(texture2DsToTest[i]);
            // If the ID has been found before
            if (predictionCount.TryGetValue(currentPrediction, out int count))
            {
                // Add to the count of how many times that prediction has appeared
                int newCount = count + 1;
                predictionCount[currentPrediction] = newCount;
                // If the prediction count was bigger then the make it the most common entry
                if (newCount > biggestCount)
                {
                    biggestCount = newCount;
                    currentBiggestPredictions.Clear();
                    currentBiggestPredictions.Add(currentPrediction);
                }
                // Otherwise if it was joint biggest, then add it to the most common entries
                else if (newCount == biggestCount)
                {
                    currentBiggestPredictions.Add(currentPrediction);
                }
            }
            // Otherwise add it to the dictionary
            else
            {
                predictionCount.Add(currentPrediction, 1);
            }
        }
        // Return the most common entries
        return currentBiggestPredictions.ToArray();
    }
    /// <summary>
    /// Test a set of textures for their Dog Breed IDs
    /// </summary>
    /// <param name="texture2DsToTest">The textures fed into ONNX model to get the predicted IDs of</param>
    /// <returns>An array of ID from all the different texture</returns>
    public int[] TestTextureForIntValueArray(Texture2D[] texture2DsToTest)
    {
        // Check if the image is valid to be put through the model
        int[] predictedOutComes = new int[texture2DsToTest.Length];
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            predictedOutComes[i] = TestTextureForIntValue(texture2DsToTest[i]);
        }
        return predictedOutComes;
    }
    #endregion
}
