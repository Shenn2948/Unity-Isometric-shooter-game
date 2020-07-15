using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float StartingHealth;

    protected float _health;
    protected bool _dead;

    public event Action OnDeath;

    protected virtual void Start()
    {
        _health = StartingHealth;
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        _health -= damage;

        if (_health <= 0 && !_dead)
        {
            Die();
        }
    }

    [ContextMenu("Self destruct")]
    protected void Die()
    {
        _dead = true;
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}