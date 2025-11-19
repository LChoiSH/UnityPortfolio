namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy for currency-based constraints.
    /// Checks if player has enough currency and consumes it after effect execution.
    /// </summary>
    public class CurrencyConstrictStrategy : IConstrictStrategy
    {
        public bool IsUsable(string name, int needAmount)
        {
            if (CurrencyManager.Instance == null)
            {
                return false;
            }

            return CurrencyManager.Instance.GetCurrencyAmount(name) >= needAmount;
        }

        public void AfterAction(string name, int needAmount)
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.UseCurrency(name, needAmount);
            }
        }
    }
}
