using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    [DisallowMultipleComponent]
    public class Unit : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private UnitStat stat = new UnitStat();
        [SerializeField] private UnitType unitType = UnitType.Default;

        private UnitStateMachine stateMachine;
        private int team;
        private Animator animator;
        private Attacker attacker;
        private Mover mover;
        private Defender defender;

        private bool isRegister = false;

        public string Id => id;
        public int Team => team;
        public UnitStat Stat => stat;
        public Animator Animator => animator;
        public Attacker Attacker => attacker;
        public Defender Defender => defender;
        public Mover Mover => mover;
        public UnitState State => stateMachine?.CurrentStateType ?? UnitState.Idle;
        public UnitStateMachine StateMachine => stateMachine;

        public event Action<Unit> onDeath;
        public event Action<Unit> onDestroy;

        private void Awake()
        {
            animator ??= GetComponentInChildren<Animator>();

            attacker = GetComponent<Attacker>();
            defender = GetComponent<Defender>();
            mover = GetComponent<Mover>();

            // State Machine 초기화
            stateMachine = new UnitStateMachine(this);
            stateMachine.Initialize(unitType);

            if(Defender) Defender.onDeath += OnDefenderDeath;
        }

        private void OnDestroy()
        {
            if(Defender) Defender.onDeath -= OnDefenderDeath;
        }

        private void OnDefenderDeath()
        {
            StartCoroutine(DeathAfterSeconds());
        }

        private void Update()
        {
            // State Machine 업데이트
            stateMachine?.Update();
        }

        private void OnEnable()
        {
            if (!isRegister)
            {
                UnitManager.Instance.Register(this);
                isRegister = true;
            }
        }

        private void OnDisable()
        {
            UnitManager.Instance.Unregister(this);
        }

        public void SetTeam(int team)
        {
            if (this.team != team)
            {
                if(isRegister) UnitManager.Instance.Unregister(this);
                this.team = team;
                UnitManager.Instance.Register(this);
                isRegister = true;
            }
        }

        /// <summary>
        /// 상태 변경 (StateMachine으로 위임)
        /// </summary>
        public void SetState(UnitState newState)
        {
            stateMachine?.ChangeState(newState);
        }

        /// <summary>
        /// 상태 강제 변경 (전환 검증 무시)
        /// </summary>
        public void ForceSetState(UnitState newState)
        {
            stateMachine?.ChangeState(newState, forceTransition: true);
        }

        public IEnumerator DeathAfterSeconds(float duration = 2f)
        {
            SetState(UnitState.Death);
            onDeath?.Invoke(this);

            yield return new WaitForSeconds(duration);

            onDestroy?.Invoke(this);

            // if dont have factory
            Destroy(gameObject);
        }
    }
}