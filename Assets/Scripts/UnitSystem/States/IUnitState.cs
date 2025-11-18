namespace UnitSystem
{
    /// <summary>
    /// Unit의 상태를 나타내는 인터페이스
    /// State Pattern의 핵심 인터페이스
    /// </summary>
    public interface IUnitState
    {
        /// <summary>
        /// 이 상태의 타입
        /// </summary>
        UnitState StateType { get; }

        /// <summary>
        /// 상태에 진입할 때 호출됨
        /// </summary>
        /// <param name="unit">상태를 소유한 유닛</param>
        void Enter(Unit unit);

        /// <summary>
        /// 매 프레임 호출됨
        /// </summary>
        /// <param name="unit">상태를 소유한 유닛</param>
        void Update(Unit unit);

        /// <summary>
        /// 상태를 빠져나갈 때 호출됨
        /// </summary>
        /// <param name="unit">상태를 소유한 유닛</param>
        void Exit(Unit unit);

        /// <summary>
        /// 다른 상태로 전환 가능한지 확인
        /// </summary>
        /// <param name="nextState">전환하려는 상태</param>
        /// <returns>전환 가능 여부</returns>
        bool CanTransitionTo(UnitState nextState);
    }
}
