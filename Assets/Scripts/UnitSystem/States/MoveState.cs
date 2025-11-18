using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    /// <summary>
    /// 이동 상태
    /// 유닛이 목적지로 이동하는 상태
    /// </summary>
    public class MoveState : UnitStateBase
    {
        public override UnitState StateType => UnitState.Move;

        // Move에서 전환 가능한 상태들
        protected override HashSet<UnitState> AllowedTransitions => new HashSet<UnitState>
        {
            UnitState.Idle,
            UnitState.Attack,
            UnitState.Death
        };

        protected override void OnEnter(Unit unit)
        {
            // 이동 시작
            // 애니메이션은 베이스 클래스에서 자동 재생

            // Mover가 없으면 Idle로 돌아가기
            if (unit.Mover == null)
            {
                Log(unit, "No Mover component, cannot move");
            }
        }

        protected override void OnUpdate(Unit unit)
        {
            // 이동 로직
            // 실제 이동은 Mover 컴포넌트가 처리
            // 여기서는 이동 완료 체크 등만 수행

            // 예: 목적지 도달 시 Idle로 전환 (외부 시스템에서 처리)
            if (unit.Mover != null && !unit.Mover.IsMoving)
            {
                // 이동 완료 - 외부에서 Idle로 전환해야 함
            }
        }

        protected override void OnExit(Unit unit)
        {
            // 이동 중단 처리
            // 필요시 Mover.Stop() 호출 가능
        }
    }
}
