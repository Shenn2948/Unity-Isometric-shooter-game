using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    }

    private State _currentState;

    private NavMeshAgent _pathFinder;
    private Transform _target;
    private LivingEntity _targetEntity;
    private Material _skinMaterial;

    private Color _originalColor;

    private float _attackDistanceThreshold = .5f;
    private float _timeBetweenAttacks = 1;
    private float _damage = 1;

    private float _nextAttackTime;
    private float _myCollisionRadius;
    private float _targetCollisionRadius;

    private bool _hasTarget;

    protected override void Start()
    {
        base.Start();
        _pathFinder = GetComponent<NavMeshAgent>();
        _skinMaterial = GetComponent<Renderer>().material;
        _originalColor = _skinMaterial.color;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _currentState = State.Chasing;
            _hasTarget = true;

            _target = player.transform;
            _targetEntity = _target.GetComponent<LivingEntity>();
            _targetEntity.OnDeath += OnTargetDeath;

            _myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            _targetCollisionRadius = GetComponent<CapsuleCollider>().radius;

            StartCoroutine(UpdatePath());
        }
    }

    private void Update()
    {
        if (_hasTarget)
        {
            if (Time.time > _nextAttackTime)
            {
                float sqrDistanceToTarget = (_target.position - transform.position).sqrMagnitude;

                if (sqrDistanceToTarget < Mathf.Pow(_attackDistanceThreshold + _myCollisionRadius + _targetCollisionRadius, 2))
                {
                    _nextAttackTime = Time.time + _timeBetweenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    private IEnumerator Attack()
    {
        _currentState = State.Attacking;
        _pathFinder.enabled = false;

        Vector3 originalPos = transform.position;
        Vector3 dirToTarget = (_target.position - transform.position).normalized;
        Vector3 attackPos = _target.position - dirToTarget * (_myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0;

        _skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                _targetEntity.TakeDamage(_damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPos, attackPos, interpolation);

            yield return null;
        }

        _skinMaterial.color = _originalColor;
        _currentState = State.Chasing;
        _pathFinder.enabled = true;
    }

    private IEnumerator UpdatePath()
    {
        const float refreshRate = .25f;

        while (_hasTarget)
        {
            if (_currentState == State.Chasing)
            {
                Vector3 dirToTarget = (_target.position - transform.position).normalized;
                Vector3 targetPos = _target.position - dirToTarget * (_myCollisionRadius + _targetCollisionRadius + _attackDistanceThreshold / 2);

                if (!_dead)
                {
                    _pathFinder.SetDestination(targetPos);
                }
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }

    private void OnTargetDeath()
    {
        _hasTarget = false;
        _currentState = State.Idle;
    }
}