using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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

    public ParticleSystem DeathEffect;

    public Image HealthBarImage;

    void Awake()
    {
        _pathFinder = GetComponent<NavMeshAgent>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _hasTarget = true;

            _target = player.transform;
            _targetEntity = _target.GetComponent<LivingEntity>();

            _myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            _targetCollisionRadius = GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (_hasTarget)
        {
            _currentState = State.Chasing;

            _targetEntity.OnDeath += OnTargetDeath;

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
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);

        if (damage >= Health)
        {
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(DeathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), DeathEffect.main.startLifetime.constant);
        }

        base.TakeHit(damage, hitPoint, hitDirection);

        HealthBarImage.fillAmount = Health / StartingHealth;
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

                if (!Dead)
                {
                    _pathFinder?.SetDestination(targetPos);
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

    public void SetCharacteristics(Spawner.Wave currentWave)
    {
        _pathFinder.speed = currentWave.MoveSpeed;

        if (_hasTarget)
        {
            _damage = Mathf.Ceil(_targetEntity.StartingHealth / currentWave.HitsToKillPlayer);
        }

        StartingHealth = currentWave.EnemyHealth;

        ParticleSystem.MainModule deathEffectMain = DeathEffect.main;
        deathEffectMain.startColor = new ParticleSystem.MinMaxGradient { color = new Color(currentWave.SkinColor.r, currentWave.SkinColor.g, currentWave.SkinColor.b, 1) };

        _skinMaterial = GetComponent<Renderer>().material;
        _skinMaterial.color = currentWave.SkinColor;
        _originalColor = _skinMaterial.color;
    }
}