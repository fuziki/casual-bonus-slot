using R3;

namespace CasualBonusSlot.Models
{
    /// <summary>
    /// スロットゲーム全体のModelを集約
    /// </summary>
    public class SlotGameModel
    {
        // 各種Model（一方向の参照）
        private readonly CreditModel _creditModel;
        private readonly LotteryModel _lotteryModel;
        private readonly ReelModel _reelModel;

        // ゲーム状態
        private ReactiveProperty<WinType> _currentWinType = new ReactiveProperty<WinType>(WinType.None);
        private ReactiveProperty<ReelSymbol[]> _currentReelSymbols = new ReactiveProperty<ReelSymbol[]>(new ReelSymbol[3]);
        private bool _isPlaying = false; // ゲーム中フラグ

        // 各リールの停止状態（連打防止用）
        private bool _isLeftReelStopped = false;
        private bool _isCenterReelStopped = false;
        private bool _isRightReelStopped = false;

        // イベント通知用Subject
        private readonly Subject<GameStartEvent> _onGameStarted = new Subject<GameStartEvent>();
        private readonly Subject<ReelStopEvent> _onReelStopped = new Subject<ReelStopEvent>();
        private readonly Subject<AllReelsStoppedEvent> _onAllReelsStopped = new Subject<AllReelsStoppedEvent>();
        private readonly Subject<Unit> _onGameOver = new Subject<Unit>();
        private readonly Subject<Unit> _onHapticTriggered = new Subject<Unit>();

        /// <summary>
        /// クレジット（外部公開用）
        /// </summary>
        public ReadOnlyReactiveProperty<int> Credit => _creditModel.Credit;

        /// <summary>
        /// ゲーム開始イベント
        /// </summary>
        public Observable<GameStartEvent> OnGameStarted => _onGameStarted;

        /// <summary>
        /// リール停止イベント（個別）
        /// </summary>
        public Observable<ReelStopEvent> OnReelStopped => _onReelStopped;

        /// <summary>
        /// 全リール停止イベント
        /// </summary>
        public Observable<AllReelsStoppedEvent> OnAllReelsStopped => _onAllReelsStopped;

        /// <summary>
        /// ゲームオーバーイベント
        /// </summary>
        public Observable<Unit> OnGameOver => _onGameOver;

        /// <summary>
        /// ハプティクスフィードバックイベント
        /// </summary>
        public Observable<Unit> OnHapticTriggered => _onHapticTriggered;

        public SlotGameModel()
        {
            _creditModel = new CreditModel();
            _lotteryModel = new LotteryModel();
            _reelModel = new ReelModel();
        }

        /// <summary>
        /// 各リールのシンボル順序をReelModelに設定
        /// </summary>
        public void SetReelSymbolOrders(ReelSymbol[] leftSymbolOrder, ReelSymbol[] centerSymbolOrder, ReelSymbol[] rightSymbolOrder)
        {
            _reelModel.SetSymbolOrders(leftSymbolOrder, centerSymbolOrder, rightSymbolOrder);
        }

        /// <summary>
        /// 画面タップ入力を受け取る（ゲーム開始）
        /// </summary>
        public void OnScreenTap()
        {
            // ガード: すでにゲーム中の場合は無視
            if (_isPlaying)
            {
                return;
            }

            // ガード: クレジット不足の場合は無視
            if (_creditModel.Credit.CurrentValue < 1)
            {
                return;
            }

            // ハプティクスフィードバック（有効なレバー押下）
            _onHapticTriggered.OnNext(Unit.Default);

            // 1. ベット
            _creditModel.Bet();

            // 2. 抽選
            WinType winType = _lotteryModel.ExecuteLottery();
            _currentWinType.Value = winType;

            // 3. カットイン抽選
            CutInType cutInType = _lotteryModel.ExecuteCutInLottery(winType);

            // 4. リール図柄決定
            ReelSymbol[] symbols = _reelModel.DecideStopSymbols(winType);
            _currentReelSymbols.Value = symbols;

            // 5. 状態リセット
            _isPlaying = true;
            _isLeftReelStopped = false;
            _isCenterReelStopped = false;
            _isRightReelStopped = false;

            // 6. イベント発行
            _onGameStarted.OnNext(new GameStartEvent { CutInType = cutInType });
        }

        /// <summary>
        /// 停止ボタン押下入力を受け取る
        /// </summary>
        public void OnStopButtonPressed(ReelPosition position)
        {
            // ガード: ゲーム中でない場合は無視
            if (!_isPlaying)
            {
                return;
            }

            // ガード: 該当リールがすでに停止済みの場合は無視（連打防止）
            switch (position)
            {
                case ReelPosition.Left:
                    if (_isLeftReelStopped) return;
                    _isLeftReelStopped = true;
                    break;
                case ReelPosition.Center:
                    if (_isCenterReelStopped) return;
                    _isCenterReelStopped = true;
                    break;
                case ReelPosition.Right:
                    if (_isRightReelStopped) return;
                    _isRightReelStopped = true;
                    break;
            }

            // 個別リール停止イベント発行
            int index = (int)position;
            ReelSymbol symbol = _currentReelSymbols.CurrentValue[index];
            _onReelStopped.OnNext(new ReelStopEvent
            {
                Position = position,
                Symbol = symbol
            });

            // 全リール停止判定
            if (_isLeftReelStopped && _isCenterReelStopped && _isRightReelStopped)
            {
                // 払い出し
                _creditModel.Payout(_currentWinType.CurrentValue);

                // 全リール停止イベント発行
                _onAllReelsStopped.OnNext(new AllReelsStoppedEvent
                {
                    WinType = _currentWinType.CurrentValue
                });

                // ゲーム状態をリセット
                _isPlaying = false;

                // ゲームオーバー判定
                if (_creditModel.IsGameOver())
                {
                    _onGameOver.OnNext(Unit.Default);
                }
            }
        }
    }
}
