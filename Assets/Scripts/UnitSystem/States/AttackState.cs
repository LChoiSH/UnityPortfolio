using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    /// <summary>
    /// 공격 상태
    /// 유닛이 적을 공격하는 상태
    /// </summary>
    public class AttackState : UnitStateBase
    {
        public override UnitState StateType => UnitState.Attack;

        // Attack에서 전환 가능한 상태들
        protected override HashSet<UnitState> AllowedTransitions => new HashSet<UnitState>
        {
            UnitState.Idle,
            UnitState.Move,
            UnitState.Death
        };

        protected override void OnEnter(Unit unit)
        {
            // 공격 준비
            // 애니메이션은 베이스 클래스에서 자동 재생

            // Attacker가 없으면 Idle로 돌아가기
            if (unit.Attacker == null)
            {
                Log(unit, "No Attacker component, returning to Idle");
                // Note: 여기서 직접 상태 변경은 안 됨, Update에서 처리하거나 외부에서 처리
            }
        }

        protected override void OnUpdate(Unit unit)
        {
            // 공격 로직
            // 실제 공격은 Attacker 컴포넌트가 처리
            // 여기서는 공격 가능 여부만 체크

            // 예: 타겟이 없거나 죽었으면 Idle로 전환 (외부 AI 시스템에서 처리)
        }

        protected override void OnExit(Unit unit)
        {
            // 공격 종료 처리
        }
    }
}
