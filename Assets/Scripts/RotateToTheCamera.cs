using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToTheCamera : MonoBehaviour
{
    private Quaternion _rotation;

    void Awake()
    {
        transform.Rotate(Camera.main.transform.rotation.eulerAngles.x, 0, 0);
        _rotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = _rotation;
    }

    private void AlignCamera()
    {
        if (Camera.main != null)
        {
            Vector3 forward = transform.position - Camera.main.transform.position;
            Vector3 up = Vector3.Cross(forward, Camera.main.transform.right);
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
}