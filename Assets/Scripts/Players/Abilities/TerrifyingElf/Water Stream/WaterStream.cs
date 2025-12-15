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
        [SerializeField] private GameObject debugBoxPrefab; // кубик с MeshRenderer
        private GameObject debugBoxInstance;
        private List<ITargetable> _targets = new List<ITargetable>();

        private void Start()
        {
            debugBoxInstance = Instantiate(debugBoxPrefab);
            debugBoxInstance.transform.SetParent(transform); // чтобы двигался с персонажем
            debugBoxInstance.transform.localPosition = Vector3.zero;
            debugBoxInstance.SetActive(false); // пока не нужен
        }

        [SerializeField] private float castLength = 6f;
        [SerializeField] private float castWidth = 4f;
        [SerializeField] private float castHeight = 1f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private Projectile _projectile;
        [SerializeField] private Transform _tileContainer;

        protected override int AnimTriggerCastDelay => 0;

        protected override int AnimTriggerCast => 0;
        // protected override int AnimTriggerCastDelay { get; }
        // protected override int AnimTriggerCast { get; }

        private Vector3 _targetPoint = Vector3.positiveInfinity;

        private Coroutine _coroutine;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                var enemies = GetCharactersInBox(castLength, castWidth, castHeight);
                UpdatePanel(enemies);
            }

            if (Input.GetKey(KeyCode.G))
            {
                debugBoxInstance.SetActive(true);

                Vector3 centers = transform.position + transform.forward * (castLength * 0.5f);
                debugBoxInstance.transform.position = centers;
                debugBoxInstance.transform.rotation = transform.rotation;
                debugBoxInstance.transform.localScale = new Vector3(castWidth, castHeight, castLength);
            }
            else
            {
                debugBoxInstance.SetActive(false);
            }
        }

        private void UpdatePanel(List<Character> enemies)
        {
            Debug.Log($"Найдено врагов: {enemies.Count}");

            foreach (var enemy in enemies)
            {
                Debug.Log(enemy.name);
                // сюда потом добавишь UI-элемент
            }
        }

        public override void LoadTargetData(TargetInfo targetInfo)
        {
            /*SetTarget((Character)targetInfo.GetTargets()[0]);
            _targetPoint = targetInfo.Points[0];*/


            _targets = targetInfo.GetTargets();
            _targetPoint = targetInfo.Points[0];
        }

        protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
        {
            /*Buff.CastSpeed.IncreasePercentage(3);

            while (float.IsPositiveInfinity(_targetPoint.x) && GetTargetCharacter() == null)
            {
                Debug.Log("While");

                if (GetMouseButton)
                {
                    Debug.Log("Mouse Click");
                    FindTargetCharacter();

                    _targetPoint = GetMousePoint();
                }

                yield return null;
            }

            TargetInfo targetInfo = new TargetInfo();
            targetInfo.AddTarget(GetTargetCharacter());
            targetInfo.Points.Add(_targetPoint);
            targetDataSavedCallback(targetInfo);*/

            Buff.CastSpeed.IncreasePercentage(3);
            List<Character> enemies = new List<Character>();

            while (float.IsPositiveInfinity(_targetPoint.x) && GetTargetCharacter() == null)
            {
                Debug.Log("While");

                if (GetMouseButton)
                {
                    Debug.Log("Mouse Click");
                    enemies = GetCharactersInBox(castLength, castWidth, castHeight);


                    _targetPoint = GetMousePoint();
                }

                yield return null;
            }

            List<ITargetable> targetables = new List<ITargetable>();

            foreach (var enemy in enemies)
            {
                if (enemy is ITargetable targetable)
                {
                    targetables.Add(targetable);
                }
            }

            Debug.Log($"Найдено {targetables.Count} целей:");

            // Вывод в консоль для проверки
            if (targetables.Count > 0)
            {
                foreach (var t in targetables)
                    Debug.Log(t);
            }

            // Создаем TargetInfo и добавляем цели
            TargetInfo targetInfo = new TargetInfo();

            foreach (var target in targetables)
            {
                targetInfo.AddTarget(target);
            }

            targetInfo.Points.Add(_targetPoint);
            targetDataSavedCallback(targetInfo);

            yield return null;
        }

        protected override IEnumerator CastJob()
        {
            if (_targets.Count > 0)
            {
                Debug.Log("Targets > 0 ");

                yield return StartCoroutine(StartWaterShot());
            }

            /*if (GetTargetCharacter() != null)
            {
                Debug.Log("Пускае");
                yield return StartCoroutine(StartWaterShot());
            }
            else
            {
                Debug.Log("Пускаем тайл");
                yield return StartCoroutine(StartWaterShot());
            }*/

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

            while (elapsedTime < duration)
            {
                float damageValue = baseDamage * damageMultiplier;

                if (damageValue <= 0)
                    break;

                Damage damage = new Damage();
                damage.Value = damageValue;

                foreach (GameObject target in targetObjects)
                {
                    CmdApplyDamage(damage, target);
                    CmdCreateProjecttile(new Vector3(target.transform.position.x, target.transform.position.y,
                        target.transform.position.z));
                    yield return new WaitForSeconds(tickRate);
                    Debug.Log("Новый выстрел водой");
                    damageMultiplier -= 0.33f;
                    elapsedTime += tickRate;
                }
            }


            /*GameObject target = GetTargetCharacter().gameObject;

            while (elapsedTime < duration)
            {
                float damageValue = baseDamage * damageMultiplier;

                if (damageValue <= 0)
                    break;

                Damage damage = new Damage();
                damage.Value = damageValue;

                CmdApplyDamage(damage, target);

                // Следующая цель получает на 33% меньше

                CmdCreateProjecttile(new Vector3(_targetPoint.x, _targetPoint.y, _targetPoint.z));
                yield return new WaitForSeconds(tickRate);
                Debug.Log("Новый выстрел водой");
                damageMultiplier -= 0.33f;
                elapsedTime += tickRate;
            }*/
        }

        protected override void ClearData()
        {
            ClearTarget();
            _targetPoint = Vector3.positiveInfinity;
        }

        protected override bool IsCanCast { get; } = true;

        [Command]
        protected void CmdCreateProjecttile(Transform target)
        {
            GameObject item = Instantiate(_projectile.gameObject, transform.position, Quaternion.identity);

            Debug.Log("ProjectTile " + item.name);

            SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

            item.GetComponent<Projectile>().StartFly(target, true);

            NetworkServer.Spawn(item);
        }

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
            // центр прямоугольника перед персонажем
            Vector3 center = transform.position + transform.forward * (length * 0.5f);

            // половины размеров
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