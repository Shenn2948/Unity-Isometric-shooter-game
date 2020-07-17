using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    private Gun _equippedGun;

    public Gun[] Guns;
    public Transform WeaponHold;


    void Start()
    {
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

    public void Reload()
    {
        if (_equippedGun != null)
        {
            _equippedGun.Reload();
        }
    }

    public float GunHeight =>
        WeaponHold.position.y;

    public void EquipGun(int weaponIndex)
    {
        EquipGun(Guns[Mathf.Clamp(weaponIndex, 0, Guns.Length - 1)]);
    }
}