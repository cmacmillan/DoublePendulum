using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumComputeMono : MonoBehaviour
{
    private RenderTexture rt;
    private ComputeBuffer rtCompBuffer;
    private int kernelIndex = -1;
    private int fadeKernelIndex = -1;
    private int renderKernelIndex = -1;
    private ComputeBuffer readBuffer;
    private ComputeBuffer writeBuffer;
    public ComputeShader s;
    public Material mat;

    public Vector2 initialConditions;
    private Vector2 oldInitialConditions;

    public int resolution;
    private int oldResolution;

    public int objectCount;
    private int oldObjectCount;

    public float timeScale;

    public float L1;
    public float L2;
    public float m1;
    public float m2;
    public float g;

    public float fadeRate;
    public float brightness;
    
    private void OnDisable()
    {
        readBuffer.Release();
        writeBuffer.Release();
        rtCompBuffer.Release();
        rt.Release();
    }

    [System.NonSerialized]
    public float deltaTime;
    void FadeTexture(float amount)
    {
        s.SetFloat("fadeAmount",amount);
        s.Dispatch(fadeKernelIndex, resolution / 8, resolution / 8, 1);
    }

    void Update()
    {
        objectCount = Mathf.Max(objectCount, 1);
        if (kernelIndex == -1)
        {
            kernelIndex = s.FindKernel("CSMain");
        }
        if (fadeKernelIndex == -1)
        {
            fadeKernelIndex = s.FindKernel("Fade");
        }
        if (renderKernelIndex == -1)
        {
            renderKernelIndex = s.FindKernel("Render");
        }

        if (resolution != oldResolution)
        {
            oldResolution = resolution;
            if (rtCompBuffer != null)
                rtCompBuffer.Release();
            if (rt != null)
                rt.Release();
            rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat);
            rt.enableRandomWrite = true;
            rt.useMipMap = false;
            rt.Create();
            rtCompBuffer = new ComputeBuffer(resolution * resolution, 4); 
            s.SetBuffer(kernelIndex, "Result", rtCompBuffer);
            s.SetBuffer(fadeKernelIndex, "Result", rtCompBuffer);
            s.SetBuffer(renderKernelIndex, "Result", rtCompBuffer);
            s.SetTexture(renderKernelIndex, "ResultTexture", rt);
            s.SetTexture(fadeKernelIndex, "ResultTexture", rt);
            //s.SetTexture(fadeKernelIndex, "Result", rt);
            mat.SetTexture("_MainTex", rt);
        }

        if (objectCount != oldObjectCount || oldInitialConditions!= initialConditions)
        {
            oldInitialConditions = initialConditions;
            oldObjectCount = objectCount;
            if (readBuffer != null)
                readBuffer.Release();
            if (writeBuffer != null)
                writeBuffer.Release();
            readBuffer = new ComputeBuffer(objectCount, 16);
            List<Vector4> initialValues = new List<Vector4>();
            //Vector4 rand = new Vector4(Random.value * Mathf.PI * 2, 0, Random.value * Mathf.PI * 2, 0);
            Vector4 rand = new Vector4(initialConditions.x, 0, initialConditions.y, 0);
            for (int i = 0; i < objectCount; i++)
            {
                Vector4 v = rand;
                v.z += i / 10000000.0f;
                initialValues.Add(v);
            }
            readBuffer.SetData(initialValues);
            writeBuffer = new ComputeBuffer(objectCount, 16);
        }

        FadeTexture(Mathf.Clamp01(fadeRate*Time.deltaTime));

        s.SetBuffer(kernelIndex, "ping", readBuffer);
        s.SetBuffer(kernelIndex,"pong", writeBuffer);
        s.SetInt("objectCount", objectCount);
        s.SetInt("resolution", resolution);
        s.SetFloat("brightness", brightness);
        s.SetFloat("L1", L1);
        s.SetFloat("L2", L2);
        s.SetFloat("m1",m1);
        s.SetFloat("m2",m2);
        s.SetFloat("g",g);
        s.SetFloat("deltaTime", Time.deltaTime*timeScale);
        var swap = readBuffer;
        readBuffer = writeBuffer;
        writeBuffer = swap;
        s.Dispatch(kernelIndex, Mathf.CeilToInt(objectCount/32.0f), 1, 1);
        s.Dispatch(renderKernelIndex, resolution / 8, resolution / 8, 1);
    }
}
