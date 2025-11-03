using UnityEngine;
using R3;
using CasualBonusSlot.Models;
using CasualBonusSlot.Presenters;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// スロットゲームのView（仮実装）
    /// </summary>
    public class SlotGameView : MonoBehaviour
    {
        [SerializeField] private ReelView leftReel;
        [SerializeField] private ReelView centerReel;
        [SerializeField] private ReelView rightReel;
        [SerializeField] private DisplayView displayView;
        [SerializeField] private WinEffectView winEffectView;
        [SerializeField] private ButtonView leftButton;
        [SerializeField] private ButtonView centerButton;
        [SerializeField] private ButtonView rightButton;
        [SerializeField] private LeverView leverView;
        [SerializeField] private HiView hiView;
        [SerializeField] private ChanceView chanceView;
        [SerializeField] private HapticsView hapticsView;

        // Viewから発行するイベント
        private Subject<Unit> _onScreenTapped = new Subject<Unit>();
        private Subject<ReelPosition> _onStopButtonPressed = new Subject<ReelPosition>();

        // MVP構成要素
        private SlotGameModel _model;
        private SlotGamePresenter _presenter;

        private DisposableBag _disposables = new();

        /// <summary>
        /// 画面タップイベント（リール回転開始トリガー）
        /// Unit: イベント発生のみを通知
        /// </summary>
        public Observable<Unit> OnScreenTapped => _onScreenTapped;

        /// <summary>
        /// 停止ボタン押下イベント
        /// ReelPosition: どのリール（Left/Center/Right）のボタンが押されたか
        /// </summary>
        public Observable<ReelPosition> OnStopButtonPressed => _onStopButtonPressed;

        /// <summary>
        /// 各リールのシンボル順序を取得
        /// </summary>
        public (ReelSymbol[] Left, ReelSymbol[] Center, ReelSymbol[] Right) ReelSymbolOrders =>
            (leftReel.SymbolOrder, centerReel.SymbolOrder, rightReel.SymbolOrder);

        void Awake()
        {
            _model = new SlotGameModel();
            _presenter = new SlotGamePresenter(this, _model);

            // イベントの購読
            SetupButtonEvents();
            SetupLeverEvents();

        }

        void Start()
        {
            // 初期状態でレバーを有効化
            leverView.SetEnabled(true);
        }

        void Update()
        {
            // 仮実装: スペースキーで画面タップ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _onScreenTapped.OnNext(Unit.Default);
            }

            // 仮実装: 1, 2, 3キーで各リールの停止ボタン押下
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _onStopButtonPressed.OnNext(ReelPosition.Left);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _onStopButtonPressed.OnNext(ReelPosition.Center);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _onStopButtonPressed.OnNext(ReelPosition.Right);
            }
        }

        public void UpdateCredit(int credit)
        {
            Debug.Log($"[View] クレジット更新: {credit}枚");

            // DisplayViewのコイン数を更新
            if (displayView != null)
            {
                displayView.SetCoins(credit);
            }
        }

        /// <summary>
        /// ボタンイベントのセットアップ
        /// 各ボタンの押下イベントを_onStopButtonPressedに流す
        /// </summary>
        private void SetupButtonEvents()
        {
            leftButton.OnButtonPressed
                .Subscribe(_ => _onStopButtonPressed.OnNext(ReelPosition.Left))
                .AddTo(ref _disposables);

            centerButton.OnButtonPressed
                .Subscribe(_ => _onStopButtonPressed.OnNext(ReelPosition.Center))
                .AddTo(ref _disposables);

            rightButton.OnButtonPressed
                .Subscribe(_ => _onStopButtonPressed.OnNext(ReelPosition.Right))
                .AddTo(ref _disposables);
        }

        /// <summary>
        /// レバーイベントのセットアップ
        /// レバー操作イベントを_onScreenTappedに流す
        /// </summary>
        private void SetupLeverEvents()
        {
            leverView.OnLeverPulled
                .Subscribe(_ => _onScreenTapped.OnNext(Unit.Default))
                .AddTo(ref _disposables);
        }

        /// <summary>
        /// ゲーム開始イベントハンドラ
        /// ベット完了表示→カットイン演出（あれば）→リール回転開始
        /// </summary>
        public void HandleGameStarted(GameStartEvent e)
        {
            Debug.Log("[View] ベット完了");

            // カットイン演出の実行
            switch (e.CutInType)
            {
                case CutInType.Normal:
                    Debug.Log("[View] カットイン演出: Normal");
                    hiView.PlayCutIn();
                    break;
                case CutInType.Hot:
                    Debug.Log("[View] カットイン演出: Hot");
                    chanceView.PlayCutIn();
                    break;
                case CutInType.None:
                    // カットイン演出なし
                    break;
            }

            Debug.Log("[View] リール回転開始");

            // レバーを無効化（リール回転中は操作不可）
            leverView.SetEnabled(false);

            // 全リールの回転を開始
            leftReel.StartSpin();
            centerReel.StartSpin();
            rightReel.StartSpin();

            // ボタンを押下可能にする
            leftButton.SetEnabled(true);
            centerButton.SetEnabled(true);
            rightButton.SetEnabled(true);
        }

        /// <summary>
        /// リール停止イベントハンドラ
        /// </summary>
        public void HandleReelStopped(ReelPosition position, ReelSymbol symbol)
        {
            Debug.Log($"[View] リール停止: {position} = {symbol}");

            // 対応するリールを停止し、ボタンを無効化
            switch (position)
            {
                case ReelPosition.Left:
                    leftReel.Stop(symbol);
                    leftButton.SetEnabled(false);
                    break;
                case ReelPosition.Center:
                    centerReel.Stop(symbol);
                    centerButton.SetEnabled(false);
                    break;
                case ReelPosition.Right:
                    rightReel.Stop(symbol);
                    rightButton.SetEnabled(false);
                    break;
            }
        }

        /// <summary>
        /// 全リール停止イベントハンドラ
        /// </summary>
        public void HandleAllReelsStopped(WinType winType)
        {
            Debug.Log($"[View] 結果表示: {winType}");

            // レバーを有効化（次ゲームの準備）
            leverView.SetEnabled(true);

            // WinTypeに応じた演出を実行
            switch (winType)
            {
                case WinType.Bell:
                    winEffectView.PlayBellEffect();
                    break;
                case WinType.Cherry:
                    winEffectView.PlayCherryEffect();
                    break;
                case WinType.Bonus:
                    winEffectView.PlayBonusEffect();
                    break;
            }
        }

        /// <summary>
        /// ゲームオーバーイベントハンドラ
        /// </summary>
        public void HandleGameOver()
        {
            Debug.Log("[View] ゲームオーバー");
        }

        /// <summary>
        /// ハプティクスフィードバックイベントハンドラ
        /// </summary>
        public void HandleHapticTriggered()
        {
            hapticsView.TriggerHaptic();
        }

        void OnDestroy()
        {
            _presenter?.Dispose();
            _disposables.Dispose();
            _onScreenTapped.Dispose();
            _onStopButtonPressed.Dispose();
        }
    }
}
