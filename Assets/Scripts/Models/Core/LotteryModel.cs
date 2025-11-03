using UnityEngine;

namespace CasualBonusSlot.Models
{
    /// <summary>
    /// 抽選処理を管理するModel
    /// </summary>
    public class LotteryModel
    {
        // 抽選確率の定義（仕様書より）
        // ボーナス: 0～3276 (5%)
        // ベル: 3277～32767 (45%)
        // チェリー: 32768～52428 (30%)
        // ハズレ: 52429～65535 (20%)
        private const int BONUS_MAX = 3276;
        private const int BELL_MAX = 32767;
        private const int CHERRY_MAX = 52428;

        // カットイン抽選確率
        // ボーナス時: 激熱80%, 通常20%
        // ハズレ時: 通常10%, なし90%
        private const float BONUS_HOT_CUTIN_RATE = 0.8f;
        private const float LOSE_NORMAL_CUTIN_RATE = 0.1f;

        /// <summary>
        /// 内部抽選を実行して当選役を決定
        /// </summary>
        public WinType ExecuteLottery()
        {
            int randomValue = Random.Range(0, 65536); // 0～65535

            if (randomValue <= BONUS_MAX)
            {
                return WinType.Bonus;
            }
            else if (randomValue <= BELL_MAX)
            {
                return WinType.Bell;
            }
            else if (randomValue <= CHERRY_MAX)
            {
                return WinType.Cherry;
            }
            else
            {
                return WinType.None;
            }
        }

        /// <summary>
        /// カットイン演出の抽選
        /// </summary>
        public CutInType ExecuteCutInLottery(WinType winType)
        {
            switch (winType)
            {
                case WinType.Bonus:
                    // ボーナス時: 80%で激熱、20%で通常
                    return Random.value < BONUS_HOT_CUTIN_RATE ? CutInType.Hot : CutInType.Normal;

                case WinType.None:
                    // ハズレ時: 10%で通常、90%でなし
                    return Random.value < LOSE_NORMAL_CUTIN_RATE ? CutInType.Normal : CutInType.None;

                default:
                    // ベル・チェリー時: カットインなし
                    return CutInType.None;
            }
        }
    }
}
