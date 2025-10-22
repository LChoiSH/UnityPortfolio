using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VInspector;

namespace UnitSystem
{
    [DisallowMultipleComponent]
    public class Unit : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private Dictionary<Type, Attacker> moduleDic = new Dictionary<Type, Attacker>();

        protected UnitState state = UnitState.Idle;
        private int team;
        private Animator animator;

        private Coroutine stunCoroutine;
        private Coroutine nuckbackCoroutine;
        private ParticleSystem stunEffect;
        private bool isRegister = false;

        public string Id => id;
        public int Team => team;
        public Animator Animator => animator;
        public Attacker Attacker => moduleDic.ContainsKey(typeof(Attacker)) ? moduleDic[typeof(Attacker)] : null;
        public UnitState State => state;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (!isRegister) UnitManager.Instance.Register(this);
        }

        void Update()
        {
            switch (state) {
                case UnitState.Idle:
                    break;
                case UnitState.Attack:
                    if (Attacker != null) Attacker.Attack();
                    break;
            }
        }

        private void OnDestroy()
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
            switch (state)
            {
                case UnitState.Attack:
                    if (Attacker == null) SetState(UnitState.Idle);
                    break;
            }

            this.state = state;
        }
    }
}