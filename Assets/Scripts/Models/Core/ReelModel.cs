using System;

namespace CasualBonusSlot.Models
{
    /// <summary>
    /// リール制御を管理するModel
    /// </summary>
    public class ReelModel
    {
        private readonly Random _random = new Random();

        // 各リールのシンボル順序
        private ReelSymbol[] _leftSymbolOrder;
        private ReelSymbol[] _centerSymbolOrder;
        private ReelSymbol[] _rightSymbolOrder;

        /// <summary>
        /// 各リールのシンボル順序を設定
        /// </summary>
        public void SetSymbolOrders(ReelSymbol[] leftSymbolOrder, ReelSymbol[] centerSymbolOrder, ReelSymbol[] rightSymbolOrder)
        {
            _leftSymbolOrder = leftSymbolOrder;
            _centerSymbolOrder = centerSymbolOrder;
            _rightSymbolOrder = rightSymbolOrder;
        }

        /// <summary>
        /// 当選役に応じた停止図柄を決定
        /// </summary>
        public ReelSymbol[] DecideStopSymbols(WinType winType)
        {
            switch (winType)
            {
                case WinType.Bonus:
                    // 777揃い
                    return new ReelSymbol[] { ReelSymbol.Seven, ReelSymbol.Seven, ReelSymbol.Seven };

                case WinType.Bell:
                    // ベル揃い
                    return new ReelSymbol[] { ReelSymbol.Bell, ReelSymbol.Bell, ReelSymbol.Bell };

                case WinType.Cherry:
                    // チェリー揃い
                    return new ReelSymbol[] { ReelSymbol.Cherry, ReelSymbol.Cherry, ReelSymbol.Cherry };

                case WinType.None:
                default:
                    // 役が成立しないランダムな図柄配列を生成
                    return GenerateNonWinningSymbols();
            }
        }

        /// <summary>
        /// 役が成立しないランダムな図柄配列を生成
        /// 各リールから図柄をランダムに抽選し、上・真ん中・下の行、斜め2辺で役が成立しないことを確認
        /// </summary>
        private ReelSymbol[] GenerateNonWinningSymbols()
        {
            var symbols = new ReelSymbol[3];

            // 最大10回試行して、条件に合う配列を生成
            for (int attempt = 0; attempt < 10; attempt++)
            {
                // 各リールからランダムに図柄を抽選
                symbols[0] = GetRandomSymbolFromReel(_leftSymbolOrder);
                symbols[1] = GetRandomSymbolFromReel(_centerSymbolOrder);
                symbols[2] = GetRandomSymbolFromReel(_rightSymbolOrder);

                // リールのシンボル順序から上・真ん中・下の図柄を取得
                var leftIndex = GetSymbolIndex(_leftSymbolOrder, symbols[0]);
                var centerIndex = GetSymbolIndex(_centerSymbolOrder, symbols[1]);
                var rightIndex = GetSymbolIndex(_rightSymbolOrder, symbols[2]);

                var leftTop = GetSymbolAtOffset(_leftSymbolOrder, leftIndex, -1);
                var leftMiddle = symbols[0];
                var leftBottom = GetSymbolAtOffset(_leftSymbolOrder, leftIndex, 1);

                var centerTop = GetSymbolAtOffset(_centerSymbolOrder, centerIndex, -1);
                var centerMiddle = symbols[1];
                var centerBottom = GetSymbolAtOffset(_centerSymbolOrder, centerIndex, 1);

                var rightTop = GetSymbolAtOffset(_rightSymbolOrder, rightIndex, -1);
                var rightMiddle = symbols[2];
                var rightBottom = GetSymbolAtOffset(_rightSymbolOrder, rightIndex, 1);

                // 全ラインで役が成立していないかチェック
                if (!IsWinningLine(leftTop, centerTop, rightTop) &&          // 上段
                    !IsWinningLine(leftMiddle, centerMiddle, rightMiddle) && // 中段
                    !IsWinningLine(leftBottom, centerBottom, rightBottom) && // 下段
                    !IsWinningLine(leftTop, centerMiddle, rightBottom) &&    // 斜め（左上→右下）
                    !IsWinningLine(leftBottom, centerMiddle, rightTop))      // 斜め（左下→右上）
                {
                    return symbols;
                }
            }

            // 万が一10回試行しても揃ってしまう場合は強制的にバラバラにする
            return new ReelSymbol[]
            {
                ReelSymbol.Watermelon,
                ReelSymbol.Grape,
                ReelSymbol.Bell
            };
        }

        /// <summary>
        /// リールからランダムに図柄を取得
        /// </summary>
        private ReelSymbol GetRandomSymbolFromReel(ReelSymbol[] reelSymbolOrder)
        {
            if (reelSymbolOrder == null || reelSymbolOrder.Length == 0)
            {
                // フォールバック
                return ReelSymbol.Watermelon;
            }

            int randomIndex = _random.Next(reelSymbolOrder.Length);
            return reelSymbolOrder[randomIndex];
        }

        /// <summary>
        /// リール内の図柄のインデックスを取得
        /// </summary>
        private int GetSymbolIndex(ReelSymbol[] reelSymbolOrder, ReelSymbol symbol)
        {
            if (reelSymbolOrder == null || reelSymbolOrder.Length == 0)
            {
                return 0;
            }

            for (int i = 0; i < reelSymbolOrder.Length; i++)
            {
                if (reelSymbolOrder[i] == symbol)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 指定されたオフセット位置の図柄を取得（ループ対応）
        /// </summary>
        private ReelSymbol GetSymbolAtOffset(ReelSymbol[] reelSymbolOrder, int baseIndex, int offset)
        {
            if (reelSymbolOrder == null || reelSymbolOrder.Length == 0)
            {
                return ReelSymbol.Watermelon;
            }

            int targetIndex = (baseIndex + offset) % reelSymbolOrder.Length;
            if (targetIndex < 0)
            {
                targetIndex += reelSymbolOrder.Length;
            }

            return reelSymbolOrder[targetIndex];
        }

        /// <summary>
        /// 3つの図柄が揃っているかチェック
        /// </summary>
        private bool IsWinningLine(ReelSymbol left, ReelSymbol center, ReelSymbol right)
        {
            return left == center && center == right;
        }
    }
}
