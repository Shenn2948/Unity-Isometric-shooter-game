using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float MoveSpeed = 5f;
    public Crosshairs Crosshairs;
    private PlayerController _controller;
    private GunController _gunController;
    private Camera _viewCamera;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _gunController = GetComponent<GunController>();
        _viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void Update()
    {
        MovementInput();
        LookInput();

        // Weapon input
        if (Input.GetMouseButton(0))
        {
            _gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _gunController.OnTriggerRelease();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _gunController.Reload();
        }

        if (transform.position.y < -10)
        {
            TakeDamage(_health);
        }
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player death", transform.position);
        base.Die();
    }

    void OnNewWave(int waveNumber)
    {
        _health = StartingHealth;
        _gunController.EquipGun(waveNumber - 1);
    }

    private void LookInput()
    {
        Ray ray = _viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * _gunController.GunHeight);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            _controller.LookAt(point);
            Crosshairs.transform.position = point;
            Crosshairs.DetectTargets(ray);

            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                _gunController.Aim(point);
            }
        }
    }

    private void MovementInput()
    {
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * MoveSpeed;
        _controller.Move(moveVelocity);
    }
}