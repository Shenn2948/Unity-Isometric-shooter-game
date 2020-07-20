using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float StartingHealth;

    public float Health;
    protected bool Dead;

    public event Action OnDeath;

    protected virtual void Start()
    {
        Health = StartingHealth;
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        Health -= damage;

        if (Health <= 0 && !Dead)
        {
            Die();
        }
    }

    [ContextMenu("Self destruct")]
    public virtual void Die()
    {
        Dead = true;
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}