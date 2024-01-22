using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject camera3D;
    [SerializeField]
    private GameObject camera2D;

    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("There are at least 2 Camera Managers");

        camera2D.SetActive(true);
        camera3D.SetActive(true);
    }

    public void TurnOn3DView()
    {
        camera3D.SetActive(true);
        camera2D.SetActive(false);
    }

    public void TurnOn2DView()
    {
        camera3D.SetActive(false);
        camera2D.SetActive(true);
    }
}
