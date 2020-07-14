using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private float nextShotTime;

    public Transform Muzzle;

    public Projectile Projectile;

    public float MsBetweenShots = 100f;

    public float MuzzleVelocity = 35f;


    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + MsBetweenShots / 1000;

            Projectile newProjectile = Instantiate(Projectile, Muzzle.position, Muzzle.rotation);
            newProjectile.SetSpeed(MuzzleVelocity);
        }
    }
}