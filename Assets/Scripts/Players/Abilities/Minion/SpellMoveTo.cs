using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpellMoveTo : Skill
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float _enemyCheckRadius = 6;
    [SerializeField] private LayerMask _enemyLayerMask;
    [SerializeField] private float _damageDeley = 0.5f;

    private Vector3 _targetPoint = Vector3.positiveInfinity;
    private Character _target = null;
    private Character _enemyTarget = null;
    private float _currentDamageDeley;
    private Coroutine _onClickCoroutine;

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    protected override bool IsCanCast => true;


    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget(targetInfo.GetTargets()[0] as Character);
        _targetPoint = targetInfo.Points[0];
    }

    protected virtual void DealDamage()
    {
        Damage damage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(_damageValue),
            Type = DamageType,
            PhysicAttackType = AttackRangeType,
        };
        CmdApplyDamage(damage, _enemyTarget.gameObject);
    }

    protected override IEnumerator CastJob()
    {
        if (_onClickCoroutine != null)
            StopCoroutine(_onClickCoroutine);

        _onClickCoroutine = StartCoroutine(OnClickJob());

        while (_targetPoint != Vector3.positiveInfinity)
        {
            if (_target != null)
                _targetPoint = _target.transform.position;

            _enemyTarget = CheckEnemy(_enemyCheckRadius);

            if (_enemyTarget != null)
            {
                _agent.SetDestination(_enemyTarget.transform.position);

                if (Vector3.Distance(transform.position, _enemyTarget.transform.position) <= Radius)
                {
                    _currentDamageDeley += Time.deltaTime;

                    if (_currentDamageDeley >= _damageDeley)
                    {
                        DealDamage();
                        _currentDamageDeley = 0;
                    }
                }
            }
            else
            {
                _currentDamageDeley = 0;
                _agent.SetDestination(_targetPoint);
            }
            yield return null;
        }
        yield return null;

        if (_onClickCoroutine != null)
            StopCoroutine(_onClickCoroutine);
    }

    protected override void ClearData()
    {
        _agent.SetDestination(transform.position);
        _targetPoint = Vector3.positiveInfinity;
        _target = null;

        if (_onClickCoroutine != null)
            StopCoroutine(_onClickCoroutine);
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
        TargetInfo targetInfo = new TargetInfo();

        while (float.IsPositiveInfinity(_targetPoint.x) && GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();

                if (GetTargetCharacter() is Character character) targetInfo.AddTarget(character);

                if (GetTargetCharacter() == null)
                {
                    _targetPoint = GetMousePoint();

                    targetInfo.Points.Add(_targetPoint);
                }
                else
                {
                    _targetPoint = _target.transform.position;

                    targetInfo.Points.Add(_targetPoint);
                }
            }
            yield return null;
        }
        targetDataSavedCallback(targetInfo);
    }

    private Character CheckEnemy(float radius)
    {
        Collider[] coliders = Physics.OverlapSphere(_targetPoint, radius, _enemyLayerMask);
        Character enemy = null;

        if (coliders.Length > 0)
        {
            Debug.Log(coliders[0].name);
            coliders[0].TryGetComponent<Character>(out enemy);
        }

        return enemy;
    }

    private IEnumerator OnClickJob()
    {
        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                GetTargetCharacter();

                if (GetTargetCharacter() == null)
                {
                    _targetPoint = GetMousePoint();
                }
                else
                {
                    _targetPoint = _target.transform.position;
                }
            }
            yield return null;
        }
    }
}