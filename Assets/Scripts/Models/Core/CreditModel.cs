using R3;

namespace CasualBonusSlot.Models
{
    /// <summary>
    /// クレジット管理を行うModel
    /// </summary>
    public class CreditModel
    {
        private const int INITIAL_CREDIT = 100;
        private const int BET_AMOUNT = 1;

        private ReactiveProperty<int> _credit = new ReactiveProperty<int>(INITIAL_CREDIT);

        /// <summary>
        /// 現在のクレジット（読み取り専用）
        /// </summary>
        public ReadOnlyReactiveProperty<int> Credit => _credit;

        /// <summary>
        /// ベット処理（1枚減算）
        /// </summary>
        public void Bet()
        {
            _credit.Value -= BET_AMOUNT;
        }

        /// <summary>
        /// 払い出し処理
        /// </summary>
        public void Payout(WinType winType)
        {
            int payoutAmount = GetPayoutAmount(winType);
            _credit.Value += payoutAmount;
        }

        /// <summary>
        /// 払い出し枚数を取得
        /// </summary>
        private int GetPayoutAmount(WinType winType)
        {
            return winType switch
            {
                WinType.Bonus => 100,
                WinType.Bell => 3,
                WinType.Cherry => 1,
                WinType.None => 0,
                _ => 0
            };
        }

        /// <summary>
        /// クレジットが0かどうか
        /// </summary>
        public bool IsGameOver()
        {
            return _credit.Value <= 0;
        }
    }
}
