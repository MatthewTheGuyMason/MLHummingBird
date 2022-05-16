using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

/// <summary>
/// Class used to judge a texture or set of textures to identify what dog breed is most likely on in
/// </summary>
public class DogClassifier : MonoBehaviour
{
    public int expectedTextureWidth;
    public int expectedTextureHeight;

    public NNModel modelAsset;

    private IWorker worker;
    private Model runtimeModel;

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

    // Start is called before the first frame update
    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.GPU);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    //if (texture2D.width != textureWidth || texture2D.height != textureHeight)
        //    //{
        //    //    //texture2D = ScaleTexture(texture2D);
        //    //}
        //    //texture2D = ScaleTexture(texture2D);
        //    Material material = new Material(textureViewer.material);
        //    material.mainTexture = texture2D;
        //    textureViewer.material = material;

        //    // Making a tensor out of a greyscale texture
        //    var channelCount = 3; // 1 = Greyscale, 3 = colour, 4 colour + alpha 
        //    var inputX = new Tensor(texture2D, channelCount);

        //    Tensor Output = worker.Execute(inputX).PeekOutput();
        //    inputX.Dispose();
        //    prediction.SetPrediction(Output);
        //}
    }

    //private Texture2D ScaleTexture(Texture2D oldTexture)
    //{
    //    return TextureScaler.scaled(oldTexture, expectedTextureWidth, ex);
    //}

    private void OnDestroy()
    {
        if (worker != null)
        {
            worker.Dispose();
        }

    }

    public int TestTextureForIntValue(Texture2D texture2DToTest)
    {
        if (texture2DToTest.width != expectedTextureWidth || texture2DToTest.height != expectedTextureHeight)
        {
            Debug.LogError("Texture was not of expected size when tested", this);
            return -1;
        }

        // Making a tensor out of a greyscale texture
        int channelCount = 3; // 1 = Greyscale, 3 = colour, 4 colour + alpha 
        Tensor inputX = new Tensor(texture2DToTest, channelCount);

        Tensor Output = worker.Execute(inputX).PeekOutput();
        inputX.Dispose();
        Prediction typePrediction = new Prediction();
        typePrediction.SetPrediction(Output);
        return typePrediction.predictedValue;
    }

    public DogType TestTextureForDogType(Texture2D texture2DToTest)
    {
        return (DogType)TestTextureForIntValue(texture2DToTest);
    }

    public int[] TestTextureForIntValueArray(Texture2D[] texture2DsToTest)
    {
        int[] predictedOutComes = new int[texture2DsToTest.Length];
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            predictedOutComes[i] = TestTextureForIntValue(texture2DsToTest[i]);
        }
        return predictedOutComes;
    }

    public DogType[] TestTextureForDogTypeArray(Texture2D[] texture2DsToTest)
    {
        DogType[] predictedOutComes = new DogType[texture2DsToTest.Length];
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            predictedOutComes[i] = (DogType)TestTextureForIntValue(texture2DsToTest[i]);
        }
        return predictedOutComes;
    }

    public int[] FindMostCommanIntInTextureArray(Texture2D[] texture2DsToTest)
    {
        Dictionary<int, int> predictionCount = new Dictionary<int, int>();
        int biggestCount = 0;
        List<int> currentBiggestPredictions = new List<int>();
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            int currentPrediction = TestTextureForIntValue(texture2DsToTest[i]);
            if (predictionCount.TryGetValue(currentPrediction, out int count))
            {
                int newCount = count + 1;
                predictionCount[currentPrediction] = newCount;
                if (newCount > biggestCount)
                {
                    biggestCount = newCount;
                    currentBiggestPredictions.Clear();
                    currentBiggestPredictions.Add(currentPrediction);
                }
                else if (newCount == biggestCount)
                {
                    currentBiggestPredictions.Add(currentPrediction);
                }
            }
            else
            {
                predictionCount.Add(currentPrediction, 1);
            }
        }
        return currentBiggestPredictions.ToArray();
    }

    public DogType[] FindMostCommanDogTypeInTextureArray(Texture2D[] texture2DsToTest)
    {
        Dictionary<int, int> predictionCount = new Dictionary<int, int>();
        int biggestCount = 0;
        List<DogType> currentBiggestPredictions = new List<DogType>();
        for (int i = 0; i < texture2DsToTest.Length; ++i)
        {
            int currentPrediction = TestTextureForIntValue(texture2DsToTest[i]);
            if (predictionCount.TryGetValue(currentPrediction, out int count))
            {
                int newCount = count + 1;
                predictionCount[currentPrediction] = newCount;
                if (newCount > biggestCount)
                {
                    biggestCount = newCount;
                    currentBiggestPredictions.Clear();
                    currentBiggestPredictions.Add((DogType)currentPrediction);
                }
                else if (newCount == biggestCount)
                {
                    currentBiggestPredictions.Add((DogType)currentPrediction);
                }
            }
            else
            {
                predictionCount.Add(currentPrediction, 1);
            }
        }
        return currentBiggestPredictions.ToArray();
    }
}
