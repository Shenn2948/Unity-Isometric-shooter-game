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
    private int _shotRemainingInBurst;
    private Vector3 _recoilSmoothDampVelocity;
    private float _recoilAngle;
    private float _recoilRotationSmoothDampVelocity;

    public Transform[] ProjectileSpawn;
    public Projectile Projectile;

    public float MsBetweenShots = 100f;
    public float MuzzleVelocity = 35f;
    public int BurstCount;

    [Header("Recoil")]
    public Vector2 KickbackMinMax = new Vector2(.05f, .2f);
    public Vector2 RecoilAngleMinMax = new Vector2(3, 5);
    public float RecoilMoveSettleTime = .1f;
    public float RecoilRotationSettleTime = .1f;

    public Transform Shell;
    public Transform ShellInjection;

    public FireMode FireModeType;


    void Start()
    {
        _muzzleFlash = GetComponent<MuzzleFlash>();
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref _recoilSmoothDampVelocity, RecoilMoveSettleTime);
        _recoilAngle = Mathf.SmoothDamp(_recoilAngle, 0, ref _recoilRotationSmoothDampVelocity, RecoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.right * -_recoilAngle;
    }

    private void Shoot()
    {
        if (Time.time > _nextShotTime)
        {
            if (FireModeType == FireMode.Burst)
            {
                if (_shotRemainingInBurst == 0)
                {
                    return;
                }

                _shotRemainingInBurst--;
            }
            else if (FireModeType == FireMode.Single)
            {
                if (!_triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            foreach (Transform t in ProjectileSpawn)
            {
                _nextShotTime = Time.time + MsBetweenShots / 1000;
                Projectile newProjectile = Instantiate(Projectile, t.position, t.rotation);
                newProjectile.SetSpeed(MuzzleVelocity);
            }

            Instantiate(Shell, ShellInjection.position, ShellInjection.rotation);
            _muzzleFlash.Activate();

            transform.localPosition -= Vector3.forward * Random.Range(KickbackMinMax.x, KickbackMinMax.y);
            _recoilAngle += Random.Range(RecoilAngleMinMax.x, RecoilAngleMinMax.y);
            _recoilAngle = Mathf.Clamp(_recoilAngle, 0, 60);
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
        _shotRemainingInBurst = BurstCount;
    }

    public void Aim(Vector3 aimPoint)
    {
        transform.LookAt(aimPoint);
    }
}