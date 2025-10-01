using System.Numerics;

namespace CurrencySystem
{
    public struct CurrencyState
    {
        public long amount;
        public long totalAmount;

        public CurrencyState(long amount, long totalAmount)
        {
            this.amount = amount;
            this.totalAmount = totalAmount;
        }

        public CurrencyState(string amount)
        {
            this.amount = long.TryParse(amount, out long value) ? value : 0;
            this.totalAmount = 0;
        }

        public CurrencyState(string amount, string totalAmount)
        {
            this.amount = long.TryParse(amount, out long value) ? value : 0;
            this.totalAmount = long.TryParse(totalAmount, out long totalValue) ? totalValue : 0;
        }

        public static explicit operator CurrencyState(string amount)
        {
            return new CurrencyState(amount);
        }
    }
}