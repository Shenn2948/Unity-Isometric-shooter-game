using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToTheCamera : MonoBehaviour
{
    private Quaternion _rotation;

    void Awake()
    {
        transform.Rotate(70, 0, 0);
        _rotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = _rotation;
    }
}