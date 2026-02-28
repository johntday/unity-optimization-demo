using System;
using UnityEngine;
using Unity.Cinemachine;

public class BenchmarkPath : MonoBehaviour
{
    public CinemachineCamera cam;
    CinemachineSplineDolly dolly;

    public int frameLength = 2000;

    private void OnEnable()
    {
        dolly = cam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineSplineDolly;
    }

    // Update is called once per frame
    void Update()
    {
        if (dolly)
        {
            dolly.CameraPosition += 1f / frameLength;
            dolly.CameraPosition = Mathf.Repeat(dolly.CameraPosition, 1f);
        }
    }
}
