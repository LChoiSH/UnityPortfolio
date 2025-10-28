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

        protected UnitState state = UnitState.Idle;
        private int team;
        private Animator animator;
        private Attacker attacker;
        private Defender defender;

        private bool isRegister = false;

        public string Id => id;
        public int Team => team;
        public Animator Animator => animator;
        public Attacker Attacker => attacker;
        public Defender Defender => defender;
        public UnitState State => state;

        public event Action<Unit> onDeath;
        public event Action<Unit> onDestroy;

        private void Awake()
        {
            animator ??= GetComponentInChildren<Animator>();

            attacker = GetComponent<Attacker>();
            defender = GetComponent<Defender>();

            if(Defender) Defender.onDeath += () => StartCoroutine(DeathAfterSeconds());
        }

        void Update()
        {
            switch (state)
            {
                case UnitState.Idle:
                    break;
                case UnitState.Attack:
                    if (Attacker != null) Attacker.Attack();
                    break;
                case UnitState.Death:
                    break;
            }
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

        public void SetState(UnitState state)
        {
            if (this.state == state) return;

            switch (state)
            {
                case UnitState.Attack:
                    if (Attacker == null) SetState(UnitState.Idle);
                    break;
                case UnitState.Death:

                    break;
            }

            this.state = state;
        }

        public IEnumerator DeathAfterSeconds(float duration = 2f)
        {
            SetState(UnitState.Death);
            onDeath?.Invoke(this);

            yield return new WaitForSeconds(2);

            onDestroy?.Invoke(this);

            yield return new WaitForSeconds(1);

            // if dont have factory
            Destroy(gameObject);
        }
    }
}