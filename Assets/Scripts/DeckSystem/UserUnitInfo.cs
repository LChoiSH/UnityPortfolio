namespace DeckSystem
{
    public class UserUnitInfo
    {
        private int level;
        private int count;

        [Newtonsoft.Json.JsonIgnore] public System.Action<int> onLevelChanged;
        [Newtonsoft.Json.JsonIgnore] public System.Action<int> onCountChanged;
        public int Level => level;
        public int Count => count;

        public UserUnitInfo(int level = 0, int count = 0)
        {
            this.level = level;
            this.count = count;
        }

        public void SetLevel(int level)
        {
            this.level = level;

            onLevelChanged?.Invoke(level);
        }

        public void SetCount(int count)
        {
            this.count = count;

            onCountChanged?.Invoke(count);
        }

        public void AddCount(int count)
        {
            this.count += count;

            onCountChanged?.Invoke(count);
        }
    }
}
