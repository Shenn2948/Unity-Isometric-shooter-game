using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }

    private float _nextShotTime;
    private MuzzleFlash _muzzleFlash;
    private bool _triggerReleasedSinceLastShot;

    public Transform Muzzle;

    public Projectile Projectile;

    public float MsBetweenShots = 100f;

    public float MuzzleVelocity = 35f;

    public Transform Shell;
    public Transform ShellInjection;

    public FireMode FireModeType;


    void Start()
    {
        _muzzleFlash = GetComponent<MuzzleFlash>();
    }

    public void Shoot()
    {
        if (Time.time > _nextShotTime)
        {
            _nextShotTime = Time.time + MsBetweenShots / 1000;

            Projectile newProjectile = Instantiate(Projectile, Muzzle.position, Muzzle.rotation);
            newProjectile.SetSpeed(MuzzleVelocity);

            Instantiate(Shell, ShellInjection.position, ShellInjection.rotation);
            _muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        _triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        _triggerReleasedSinceLastShot = true;
    }
}