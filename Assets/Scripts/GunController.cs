using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    private Gun _equippedGun;

    public Gun StartingGun;
    public Transform WeaponHold;


    void Start()
    {
        if (StartingGun != null)
        {
            EquipGun(StartingGun);
        }
    }

    public void EquipGun(Gun gunToEquip)
    {
        if (_equippedGun != null)
        {
            Destroy(_equippedGun.gameObject);
        }

        _equippedGun = Instantiate(gunToEquip, WeaponHold.position, WeaponHold.rotation);
        _equippedGun.transform.parent = WeaponHold;
    }

    public void OnTriggerHold()
    {
        if (_equippedGun != null)
        {
            _equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease()
    {
        if (_equippedGun != null)
        {
            _equippedGun.OnTriggerRelease();
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if (_equippedGun != null)
        {
            _equippedGun.Aim(aimPoint);
        }
    }

    public float GunHeight =>
        WeaponHold.position.y;
}