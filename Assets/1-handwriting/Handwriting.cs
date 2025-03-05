using UnityEngine;
using Unity.Sentis;
using UnityEditor;
using System.Linq;
using System;
using TMPro;

public class ClassifyHandwrittenDigit : MonoBehaviour
{
    public Texture2D inputTexture;
    public RenderTexture inputRenderTexture;
    public ModelAsset modelAsset;

    public TMP_Text outputText;

    Model runtimeModel;
    Worker worker;
    public float[] results;

    [SerializeField] int guess;

    void Start()
    {
        Model sourceModel = ModelLoader.Load(modelAsset);

        // Create a functional graph that runs the input model and then applies softmax to the output.
        FunctionalGraph graph = new FunctionalGraph();
        FunctionalTensor[] inputs = graph.AddInputs(sourceModel);
        FunctionalTensor[] outputs = Functional.Forward(sourceModel, inputs);
        FunctionalTensor softmax = Functional.Softmax(outputs[0]);

        // Create a model with softmax by compiling the functional graph.
        runtimeModel = graph.Compile(softmax);
    }

    //     // Create input data as a tensor
    //     using Tensor inputTensor = TextureConverter.ToTensor(inputTexture, width: 28, height: 28, channels: 1);

    //     // Create an engine
    //     worker = new Worker(runtimeModel, BackendType.GPUCompute);

    //     // Run the model with the input data
    //     worker.Schedule(inputTensor);

    //     // Get the result
    //     Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

    //     // outputTensor is still pending
    //     // Either read back the results asynchronously or do a blocking download call
    //     results = outputTensor.DownloadToArray();
    // }

    [ContextMenu("Run")]
    void Run()
    {
        Texture2D texture = toTexture2D(inputRenderTexture);
        using Tensor inputTensor = TextureConverter.ToTensor(texture, width: 28, height: 28, channels: 1);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        // Run the model with the input data
        worker.Schedule(inputTensor);
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        results = outputTensor.DownloadToArray();

        float m = results.Max();
        guess = Array.IndexOf(results, m);
        outputText.SetText(guess.ToString());
    }

    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(28, 28, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
            Run();
    }

    void OnDisable()
    {
        // Tell the GPU we're finished with the memory the engine used
        worker.Dispose();
    }
}