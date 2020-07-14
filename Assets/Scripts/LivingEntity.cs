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

    public void TakeHit(float damage, RaycastHit hit)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;

        if (_health <= 0 && !_dead)
        {
            Die();
        }
    }

    protected void Die()
    {
        _dead = true;
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}