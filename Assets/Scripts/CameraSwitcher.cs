using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    private int camIndex = 0;
    private int camCount = 0;
    private CinemachineVirtualCamera initialCamera;
    private List<CinemachineVirtualCamera> cameraList = new List<CinemachineVirtualCamera>();

    // Start is called before the first frame update
    void Start()
    {
        camCount = this.transform.childCount - 1;
        initialCamera = this.transform.GetChild(0).gameObject.GetComponent<CinemachineVirtualCamera>();
        for(int i = 0; i < camCount; i++)
        {   
            cameraList.Add(this.transform.GetChild(i + 1).gameObject.GetComponent<CinemachineVirtualCamera>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) 
        {
            Debug.Log(camIndex);
            ActivateNextCamera();
        }
    }

    public void ActivateCameraList()
    {
        initialCamera.Priority = 0;
        cameraList[0].Priority = 10;
    }

    private void ActivateNextCamera()
    {
        camIndex = (camIndex + 1) % camCount;
        CinemachineVirtualCamera curCamera = cameraList[camIndex];
        foreach(var cam in cameraList)
        {
            cam.Priority = cam == curCamera ? 10 : 0;
        }
    }

    public void ActivateInitialCamera()
    {
        initialCamera.Priority = 999;
    }
}
