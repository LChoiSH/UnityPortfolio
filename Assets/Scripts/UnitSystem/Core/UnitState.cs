namespace UnitSystem
{
    /// <summary>
    /// 유닛 상태 식별자
    /// - Enum: 빠른 비교, 직렬화, 네트워크 동기화에 사용
    /// - State 클래스: 각 상태의 행동과 로직 캡슐화
    /// </summary>
    public enum UnitState
    {
        Idle,
        Attack,
        Move,
        Death
    }
}