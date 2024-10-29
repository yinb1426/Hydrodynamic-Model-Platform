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
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private List<CinemachineVirtualCamera> cameraList = new();
    private List<Vector3> cameraPositionList = new();
    private List<Quaternion> cameraRotationList = new();

    // Start is called before the first frame update
    void Start()
    {
        camCount = transform.childCount - 1;
        initialCamera = transform.GetChild(0).gameObject.GetComponent<CinemachineVirtualCamera>();
        initialCameraPosition = initialCamera.transform.position;
        initialCameraRotation = initialCamera.transform.rotation;
        for(int i = 0; i < camCount; i++)
        {   
            cameraList.Add(transform.GetChild(i + 1).gameObject.GetComponent<CinemachineVirtualCamera>());
            cameraPositionList.Add(cameraList[i].transform.position);
            cameraRotationList.Add(cameraList[i].transform.rotation);
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
        ResetInitialCameraConfiguration();
        ResetCameraListConfiguration(0);
        cameraList[0].Priority = 10;
    }

    private void ActivateNextCamera()
    {
        camIndex = (camIndex + 1) % camCount;
        ResetCameraListConfiguration(camIndex);
        CinemachineVirtualCamera curCamera = cameraList[camIndex];
        foreach(var cam in cameraList)
        {
            cam.Priority = cam == curCamera ? 10 : 0;
        }
    }

    public void ActivateInitialCamera()
    {
        ResetInitialCameraConfiguration();
        initialCamera.Priority = 999;
    }

    private void ResetInitialCameraConfiguration()
    {
        initialCamera.transform.position = initialCameraPosition;
        initialCamera.transform.rotation = initialCameraRotation;
    }

    private void ResetCameraListConfiguration(int index)
    {
        cameraList[index].transform.position = cameraPositionList[index];
        cameraList[index].transform.rotation = cameraRotationList[index];
    }
}
