using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    /// <summary>
    /// 대기 상태
    /// 유닛이 아무것도 하지 않고 대기 중인 상태
    /// </summary>
    public class IdleState : UnitStateBase
    {
        public override UnitState StateType => UnitState.Idle;

        // Idle에서 전환 가능한 상태들
        protected override HashSet<UnitState> AllowedTransitions => new HashSet<UnitState>
        {
            UnitState.Attack,
            UnitState.Move,
            UnitState.Death
        };

        protected override void OnEnter(Unit unit)
        {
            // Idle 애니메이션은 베이스 클래스에서 자동 재생
            // 추가 로직이 필요하면 여기에 작성
        }

        protected override void OnUpdate(Unit unit)
        {
            // Idle 상태에서의 로직
            // 예: 적 탐지, 명령 대기 등
            // 실제 게임 로직은 AI나 다른 시스템에서 처리하고
            // 여기서는 상태만 관리
        }

        protected override void OnExit(Unit unit)
        {
            // Idle 종료 시 정리 작업
        }
    }
}
