using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompShaderMono : MonoBehaviour
{
    public Material mat;
    public ComputeShader s;

    private RenderTexture rt;
    private int kernelIndex = -1;
    private int fadeKernelIndex = -1;
    private ComputeBuffer readBuffer;
    private ComputeBuffer writeBuffer;

    [Header("Params")]
    public float g;
    public float forcemax;
    public float speed=1;
    public float zoom;

    public float fadeAmount = 1;

    public int resolution=512;
    private int oldResolution = -1;

    public int objectCount=10;
    private int oldObjectCount = -1;

    public float velocityMax;
    private float oldVelocityMax = -1;

    public float distanceMax;
    private float oldDistanceMax = -1;

    private void OnDisable()
    {
        readBuffer.Release();
        writeBuffer.Release();
        rt.Release();
    }
    void FadeTexture(float amount)
    {
        s.SetFloat("fadeAmount",amount);
        s.Dispatch(fadeKernelIndex, resolution / 8, resolution / 8, 1);
    }
    void UpdateDraw()
    {
        if (kernelIndex == -1)
        {
            kernelIndex = s.FindKernel("CSMain");
        }

        if (fadeKernelIndex == -1)
        {
            fadeKernelIndex= s.FindKernel("Fade");
        }

        if (resolution != oldResolution)
        {
            oldResolution = resolution;
            if (rt != null)
                rt.Release();
            rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat);
            rt.enableRandomWrite = true;
            rt.useMipMap = false;
            rt.Create();
            s.SetTexture(kernelIndex, "Result", rt);
            s.SetTexture(fadeKernelIndex, "Result",rt);
            mat.SetTexture("_MainTex", rt);
        }

        if (objectCount != oldObjectCount || oldDistanceMax!=distanceMax || oldVelocityMax!=velocityMax)
        {
            oldObjectCount = objectCount;
            oldVelocityMax = velocityMax;
            oldDistanceMax = distanceMax;
            if (readBuffer != null)
                readBuffer.Release();
            if (writeBuffer != null)
                writeBuffer.Release();
            readBuffer = new ComputeBuffer(objectCount, 16);
            List<Vector4> initialValues = new List<Vector4>();
            for (int i = 0; i < objectCount; i++)
            {
                Vector2 pos = Random.Range(0, distanceMax) * Random.insideUnitCircle;
                Vector2 vel = Random.Range(0, velocityMax) * Random.insideUnitCircle;
                initialValues.Add(new Vector4(pos.x,pos.y,vel.x,vel.y));
            }
            readBuffer.SetData(initialValues);
            s.SetBuffer(kernelIndex, "ping", readBuffer);
            writeBuffer = new ComputeBuffer(objectCount, 16);
            s.SetBuffer(kernelIndex,"pong", writeBuffer);
            FadeTexture(1);
        }

        s.SetFloat("timestep",Time.deltaTime*speed);
        s.SetFloat("g", g);
        s.SetFloat("forcemax", forcemax);
        s.SetFloat("zoom", zoom);
        s.SetInt("objCount",objectCount);
        s.SetVector("mid",new Vector4(resolution/2.0f,resolution/2.0f,0,0));
        s.Dispatch(kernelIndex, objectCount, 1, 1);

        FadeTexture(Time.deltaTime * fadeAmount);

        var swap = readBuffer;
        readBuffer = writeBuffer;
        writeBuffer = swap;
        s.SetBuffer(kernelIndex,"ping", readBuffer);
        s.SetBuffer(kernelIndex,"pong", writeBuffer);
    }
    void Update()
    {
        UpdateDraw();
    }
}
