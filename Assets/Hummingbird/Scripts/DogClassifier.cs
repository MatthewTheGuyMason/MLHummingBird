using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class DogClassifier : MonoBehaviour
{

    public Texture2D texture2D;

    public NNModel modelAsset;

    private Model runtimeModel;
    private IWorker worker;

    public int textureWidth;
    public int textureHeight;

    [System.Serializable]
    public struct Prediction
    {
        public int predictedValue;
        public float[] predicted;

        public void SetPrediction(Tensor t)
        {
            predicted = t.AsFloats();
            predictedValue = System.Array.IndexOf(predicted, predicted.Max());
            Debug.Log($"Predicted {predictedValue}");
        }
    }

    public Prediction prediction;

    // Start is called before the first frame update
    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.GPU);
        prediction = new Prediction();
        texture2D = ScaleTexture(texture2D);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (texture2D.width != textureWidth || texture2D.height != textureHeight)
            {
                texture2D = ScaleTexture(texture2D);
            }

            // Making a tensor out of a greyscale texture
            var channelCount = 3; // 1 = Greyscale, 3 = colour, 4 colour + alpha 
            var inputX = new Tensor(texture2D, channelCount);

            Tensor Output = worker.Execute(inputX).PeekOutput();
            inputX.Dispose();
            prediction.SetPrediction(Output);
        }
    }

    private Texture2D ScaleTexture(Texture2D oldTexture)
    {
        return TextureScaler.scaled(oldTexture, textureWidth, textureHeight);
    }

    private void OnDestroy()
    {
        if (worker != null)
        {
            worker.Dispose();
        }

    }
}
