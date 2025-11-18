using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    /// <summary>
    /// Unit의 상태를 관리하는 State Machine
    /// 상태 전환, 업데이트, 검증을 담당
    /// </summary>
    public class UnitStateMachine
    {
        private Unit owner;
        private IUnitState currentState;
        private Dictionary<UnitState, IUnitState> states;

        public UnitState CurrentStateType => currentState?.StateType ?? UnitState.Idle;
        public IUnitState CurrentState => currentState;

        public event Action<UnitState, UnitState> onStateChanged; // (이전 상태, 새 상태)

        public UnitStateMachine(Unit owner)
        {
            this.owner = owner;
            states = new Dictionary<UnitState, IUnitState>();
        }

        /// <summary>
        /// State Machine 초기화
        /// Factory Pattern을 사용하여 유닛 타입에 맞는 상태들을 생성
        /// </summary>
        /// <param name="unitType">유닛 타입</param>
        public void Initialize(UnitType unitType)
        {
            // Factory를 통해 유닛 타입에 맞는 상태들 생성
            var statesToRegister = UnitStateFactory.CreateStates(unitType);

            // 생성된 상태들 등록
            foreach (var state in statesToRegister)
            {
                RegisterState(state);
            }

            // 초기 상태를 Idle로 설정
            ChangeState(UnitState.Idle, forceTransition: true);
        }

        /// <summary>
        /// 상태 등록
        /// </summary>
        private void RegisterState(IUnitState state)
        {
            if (states.ContainsKey(state.StateType))
            {
                Debug.LogWarning($"[UnitStateMachine] State {state.StateType} is already registered!");
                return;
            }

            states[state.StateType] = state;
        }

        /// <summary>
        /// 매 프레임 현재 상태 업데이트
        /// </summary>
        public void Update()
        {
            currentState?.Update(owner);
        }

        /// <summary>
        /// 상태 전환
        /// </summary>
        /// <param name="newState">전환할 상태</param>
        /// <param name="forceTransition">강제 전환 여부 (검증 무시)</param>
        /// <returns>전환 성공 여부</returns>
        public bool ChangeState(UnitState newState, bool forceTransition = false)
        {
            // 상태가 등록되어 있는지 확인
            if (!states.TryGetValue(newState, out IUnitState nextState))
            {
                Debug.LogError($"[UnitStateMachine] State {newState} is not registered!");
                return false;
            }

            // 현재 상태가 없으면 (초기화) 바로 진입
            if (currentState == null)
            {
                currentState = nextState;
                currentState.Enter(owner);
                onStateChanged?.Invoke(UnitState.Idle, newState);
                return true;
            }

            // 같은 상태로 전환 시도 시 무시
            if (currentState.StateType == newState)
            {
                return false;
            }

            // 전환 가능 여부 검증 (강제 전환이 아닌 경우)
            if (!forceTransition && !currentState.CanTransitionTo(newState))
            {
                Debug.LogWarning($"[UnitStateMachine] Cannot transition from {currentState.StateType} to {newState}");
                return false;
            }

            // 상태 전환 실행
            UnitState previousState = currentState.StateType;
            currentState.Exit(owner);
            currentState = nextState;
            currentState.Enter(owner);

            onStateChanged?.Invoke(previousState, newState);

            return true;
        }

        /// <summary>
        /// 특정 상태로 전환 가능한지 확인
        /// </summary>
        public bool CanTransitionTo(UnitState newState)
        {
            if (currentState == null) return true;
            return currentState.CanTransitionTo(newState);
        }

        /// <summary>
        /// 현재 상태가 특정 타입인지 확인
        /// </summary>
        public bool IsInState(UnitState stateType)
        {
            return CurrentStateType == stateType;
        }

        // --------- Debug ---------
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogCurrentState()
        {
            Debug.Log($"[{owner.name}] Current State: {CurrentStateType}");
        }
    }
}
