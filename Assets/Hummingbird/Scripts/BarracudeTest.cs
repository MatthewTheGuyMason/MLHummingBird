//====================================================================================================================================================================================================================================
//  Name:               BarracudeTest.cs
//  Author:             Matthew Mason
//  Date Created:       29/05/2022
//  Date Last Modified: 29/05/2022
//  Brief:              Script for testing out how barracuda works
//====================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

/// <summary>
/// Script for testing out how barracuda works
/// </summary>
[System.Obsolete("This code was intended to test out the functionality of Barracuda and serves no purpose within the game" +
    "As a result it does not perform any meaningful function but is here for learning purposes")]
public class BarracudeTest : MonoBehaviour
{
    #region Public Variables
    [Tooltip("How many images per run")]
    public int batch;
    [Tooltip("How many colour channels are present")]
    public int channels;
    [Tooltip("How tall the image is in pixels")]
    public int height;
    [Tooltip("How wide the image is in pixels")]
    public int width;

    [Tooltip("The ONNX model to load in")]
    public NNModel modelAsset;
    #endregion

    #region Private Variables
    /// <summary>
    /// The worker processing the model
    /// </summary>
    private IWorker m_Worker;

    /// <summary>
    /// The model used at runtime
    /// </summary>
    private Model m_RuntimeModel;
    #endregion

    #region Unity Methods
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
    #endregion
}
