using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float MoveSpeed = 5f;
    private PlayerController _controller;
    private GunController _gunController;
    private Camera _viewCamera;

    protected override void Start()
    {
        base.Start();
        _controller = GetComponent<PlayerController>();
        _gunController = GetComponent<GunController>();
        _viewCamera = Camera.main;
    }

    void Update()
    {
        // Movement input
        MovementInput();

        // Look input
        LookInput();

        // Weapon input
        if (Input.GetMouseButton(0))
        {
            _gunController.Shoot();
        }
    }

    private void LookInput()
    {
        Ray ray = _viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            _controller.LookAt(point);
        }
    }

    private void MovementInput()
    {
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * MoveSpeed;
        _controller.Move(moveVelocity);
    }
}