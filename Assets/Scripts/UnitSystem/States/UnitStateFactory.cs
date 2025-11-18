using System.Collections.Generic;

namespace UnitSystem
{
    /// <summary>
    /// 유닛 타입 정의
    /// </summary>
    public enum UnitType
    {
        Default,    // 기본 유닛
        Melee,      // 근접 유닛
        Ranged,     // 원거리 유닛
        Tank,       // 탱커 유닛
        Support,    // 서포터 유닛
        Boss        // 보스 유닛
    }

    /// <summary>
    /// Unit State를 생성하는 Factory 클래스
    /// Factory Pattern을 활용하여 다양한 유닛 타입에 맞는 State 조합을 제공
    /// OCP 준수: 새로운 유닛 타입 추가 시 case만 추가하면 됨
    /// </summary>
    public static class UnitStateFactory
    {
        /// <summary>
        /// 유닛 타입에 따라 적절한 상태 세트 반환
        /// </summary>
        public static IEnumerable<IUnitState> CreateStates(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Boss:
                    return CreateBossStates();

                case UnitType.Ranged:
                    return CreateRangedStates();

                case UnitType.Melee:
                case UnitType.Tank:
                case UnitType.Support:
                case UnitType.Default:
                default:
                    return CreateDefaultStates();
            }
        }

        /// <summary>
        /// 기본 유닛용 상태 세트
        /// </summary>
        private static IEnumerable<IUnitState> CreateDefaultStates()
        {
            return new IUnitState[]
            {
                new IdleState(),
                new AttackState(),
                new MoveState(),
                new DeathState()
            };
        }

        /// <summary>
        /// 원거리 유닛용 상태 세트
        /// </summary>
        private static IEnumerable<IUnitState> CreateRangedStates()
        {
            return new IUnitState[]
            {
                new IdleState(),
                new AttackState(),
                new MoveState(),
                new DeathState()
                // 나중에 RangedAttackState 등 추가 가능
            };
        }

        /// <summary>
        /// 보스 유닛용 상태 세트 (확장 가능성 시연)
        /// </summary>
        private static IEnumerable<IUnitState> CreateBossStates()
        {
            return new IUnitState[]
            {
                new IdleState(),
                new AttackState(),
                new MoveState(),
                new DeathState()
                // 나중에 BossSkillState, EnrageState 등 추가 가능
            };
        }
    }
}
