using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class BarracudeTest : MonoBehaviour
{
    public NNModel modelAsset;
    private Model m_RuntimeModel;

    private IWorker m_Worker;

    public int batch;
    public int height;
    public int width;
    public int channels;

    void Start()
    {
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        m_Worker = WorkerFactory.CreateWorker(m_RuntimeModel);

        Tensor input = new Tensor(batch, height, width, channels);
        float[] inputs = input.AsFloats();
        for (int i = 0; i < 100; ++i)
        {
            Debug.Log("Inputs " + i.ToString() + ": " + inputs[i]);
        }
        m_Worker.Execute(input);
        Tensor O = m_Worker.PeekOutput("dog");
        Debug.Log(O.AsFloats()[0]);
        input.Dispose();
    }

    void Update()
    {

    }

}
