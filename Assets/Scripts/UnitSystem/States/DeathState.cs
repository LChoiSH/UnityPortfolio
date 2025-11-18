using System.Collections.Generic;
using UnityEngine;

namespace UnitSystem
{
    /// <summary>
    /// 사망 상태
    /// 유닛이 죽은 최종 상태 (다른 상태로 전환 불가)
    /// </summary>
    public class DeathState : UnitStateBase
    {
        public override UnitState StateType => UnitState.Death;

        // Death는 최종 상태 - 다른 상태로 전환 불가
        protected override HashSet<UnitState> AllowedTransitions => new HashSet<UnitState>();

        protected override void OnEnter(Unit unit)
        {
            // 사망 처리
            // 애니메이션은 베이스의 Enter()에서 자동 재생

            // 충돌 비활성화
            var collider = unit.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            var collider2D = unit.GetComponent<Collider2D>();
            if (collider2D != null)
            {
                collider2D.enabled = false;
            }

            // 이동 중단
            if (unit.Mover != null)
            {
                unit.Mover.Stop();
            }

            Log(unit, "Unit has died");
        }

        protected override void OnUpdate(Unit unit)
        {
            // 사망 상태에서는 특별한 업데이트 로직이 필요 없음
            // 사망 애니메이션 종료 후 오브젝트 제거는 외부에서 처리
        }

        protected override void OnExit(Unit unit)
        {
            // Death 상태는 빠져나올 수 없음
            // 이 메서드는 호출되지 않아야 함
            Debug.LogWarning($"[{unit.name}] DeathState.OnExit should not be called!");
        }

        // Death 상태는 어떤 상태로도 전환 불가
        public override bool CanTransitionTo(UnitState nextState)
        {
            return false;
        }
    }
}
