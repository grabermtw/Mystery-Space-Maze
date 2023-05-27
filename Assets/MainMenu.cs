using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MainMenu : MonoBehaviour
{
    public CinemachineVirtualCamera gameVCam;
    public Camera mainCamera;
    public Camera uiCamera;
    public GameObject menuUI;

    // Start is called before the first frame update
    void Start()
    {
        gameVCam.m_Priority = 10;
        mainCamera.rect = new Rect(0, 0, 1, 1);
        uiCamera.rect = new Rect(0, 0, 0, 0);
        menuUI.SetActive(true);
    }

    public void StartGame()
    {
        gameVCam.m_Priority = 20;
        mainCamera.rect = new Rect(0, 0.1f, 1, 1);
        uiCamera.rect = new Rect(0, 0, 1, 0.1f);
        menuUI.SetActive(false);
    }
}
