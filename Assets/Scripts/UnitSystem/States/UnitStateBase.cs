using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    /// <summary>
    /// 모든 Unit State의 베이스 클래스
    /// 공통 기능과 기본 구현을 제공
    /// </summary>
    public abstract class UnitStateBase : IUnitState
    {
        public abstract UnitState StateType { get; }

        // 기본 전환 규칙 (각 상태에서 오버라이드 가능)
        protected virtual HashSet<UnitState> AllowedTransitions => new HashSet<UnitState>
        {
            UnitState.Death // 모든 상태에서 Death로 전환 가능
        };

        public virtual void Enter(Unit unit)
        {
            // 애니메이션 재생 (상태 이름과 동일한 트리거 사용)
            if (unit.Animator != null)
            {
                unit.Animator.SetTrigger(StateType.ToString());
            }

            OnEnter(unit);
        }

        public virtual void Update(Unit unit)
        {
            OnUpdate(unit);
        }

        public virtual void Exit(Unit unit)
        {
            OnExit(unit);
        }

        public virtual bool CanTransitionTo(UnitState nextState)
        {
            // 같은 상태로는 전환 불가
            if (StateType == nextState)
                return false;

            // 허용된 전환인지 확인
            return AllowedTransitions.Contains(nextState);
        }

        // 서브클래스에서 구현할 메서드들
        protected virtual void OnEnter(Unit unit) { }
        protected virtual void OnUpdate(Unit unit) { }
        protected virtual void OnExit(Unit unit) { }

        // 유틸리티 메서드
        protected void Log(Unit unit, string message)
        {
            Debug.Log($"[{unit.name}] {StateType}: {message}");
        }
    }
}
