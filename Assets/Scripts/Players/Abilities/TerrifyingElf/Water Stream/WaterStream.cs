using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Players.Abilities.TerrifyingElf.Water_Stream
{
    public class WaterStream : Skill
    {
        [SerializeField] private float castLength = 6f;
        [SerializeField] private float castWidth = 4f;
        [SerializeField] private float castHeight = 1f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private Projectile _projectile;
        [SerializeField] private Transform _tileContainer;
        [SerializeField] private MoveComponent _moveComponent;

        protected override int AnimTriggerCastDelay => 0;
        protected override int AnimTriggerCast => 0;
        
        private List<ITargetable> _targets = new List<ITargetable>();
        private Vector3 _targetPoint = Vector3.positiveInfinity;
        private Coroutine _coroutine;

        public override void LoadTargetData(TargetInfo targetInfo)
        {
            _targets = targetInfo.GetTargets();
            _targetPoint = targetInfo.Points[0];
        }

        protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
        {
            Buff.CastSpeed.IncreasePercentage(3);
            List<Character> enemies = new List<Character>();

            while (float.IsPositiveInfinity(_targetPoint.x) && GetTargetCharacter() == null)
            {
                if (GetMouseButton)
                {
                    enemies = GetCharactersInBox(castLength, castWidth, castHeight);
                    _targetPoint = GetMousePoint();
                    
                    _hero.Move.CanMove = false;
                }

                yield return null;
            }

            List<ITargetable> targetables = new List<ITargetable>();

            foreach (var enemy in enemies)
            {
                if (enemy is ITargetable targetable)
                    targetables.Add(targetable);
            }

            Debug.Log($"Найдено {targetables.Count} целей:");
            
            if (targetables.Count > 0)
            {
                foreach (var t in targetables)
                    Debug.Log(t);
            }
            
            TargetInfo targetInfo = new TargetInfo();

            foreach (var target in targetables)
                targetInfo.AddTarget(target);

            targetInfo.Points.Add(_targetPoint);
            targetDataSavedCallback(targetInfo);
            yield return null;
        }

        protected override IEnumerator CastJob()
        {
            if (_targets.Count > 0)
                yield return StartCoroutine(StartWaterShot());

            _hero.Move.CanMove = true;
            yield return null;
        }

        private IEnumerator StartWaterShot()
        {
            float duration = 2f;
            float tickRate = 0.4f;
            float baseDamage = 20f;
            float elapsedTime = 0f;
            float damageMultiplier = 1f;
            Debug.Log("Water");

            List<GameObject> targetObjects = _targets
                .OfType<MonoBehaviour>() // оставляем только объекты, которые являются MonoBehaviour
                .Select(mb => mb.gameObject) // получаем GameObject
                .ToList();
            
            Damage damage = new Damage();
            float damageValue;
            
            while (elapsedTime < duration)
            {
                foreach (GameObject target in targetObjects)
                {
                    damageValue = baseDamage * damageMultiplier;

                    if (damageValue <= 0)
                        break;
                    
                    damage.Value = damageValue;
                    CmdApplyDamage(damage, target);
                    CmdCreateProjecttile(target.transform.position);
                    damageMultiplier -= 0.33f;
                }

                yield return new WaitForSeconds(tickRate);
                elapsedTime += tickRate;
            }
        }

        protected override void ClearData()
        {
            ClearTarget();
            _targetPoint = Vector3.positiveInfinity;
        }

        protected override bool IsCanCast { get; } = true;

        [Command]
        protected void CmdCreateProjecttile(Vector3 point)
        {
            GameObject item = Instantiate(_projectile.gameObject, transform.position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);
            item.GetComponent<Projectile>().StartFly(point, true);
            NetworkServer.Spawn(item);
        }

        private List<Character> GetCharactersInBox(float length, float width, float height)
        {
            Vector3 center = transform.position + transform.forward * (length * 0.5f);
            Vector3 halfExtents = new Vector3(width * 0.5f, height * 0.5f, length * 0.5f);
            Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
            List<Character> characters = new List<Character>();

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Character>(out var character))
                    if (character != Hero)
                        characters.Add(character);
            }

            return characters;
        }
    }
}