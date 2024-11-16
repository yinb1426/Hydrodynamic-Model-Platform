using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera _MainCamera;

    public float _MovementSpeed = 1f;
    public float _RotationSpeed = 0.05f;
    public float _ScrollSpeed = 3f;

    private Vector3 previousMovementPosition = Vector3.zero;
    private Vector3 previousRotationPosition = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        // 相机平移
        if (Input.GetMouseButtonDown(0))
            previousMovementPosition = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector3 movementVector = Input.mousePosition - previousMovementPosition;
            _MainCamera.transform.position += -movementVector.x * _MovementSpeed * _MainCamera.transform.right;
            _MainCamera.transform.position += -movementVector.y * _MovementSpeed * _MainCamera.transform.up;
            previousMovementPosition = Input.mousePosition;
        }
        else
            previousMovementPosition = Vector3.zero;

        // 相机旋转
        if (Input.GetMouseButtonDown(1))
            previousRotationPosition = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {
            Vector3 movementVector = (Input.mousePosition - previousRotationPosition) * _RotationSpeed;
            _MainCamera.transform.Rotate(Vector3.up, movementVector.x, Space.World);
            _MainCamera.transform.Rotate(Vector3.right, movementVector.y);
            previousRotationPosition = Input.mousePosition;
        }
        else
            previousRotationPosition = Vector3.zero;

        // 相机前后移动
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        _MainCamera.transform.position += scrollData * _MainCamera.transform.forward * _ScrollSpeed;
    }
}
