using System;
using R3;
using CasualBonusSlot.Models;
using CasualBonusSlot.Views;

namespace CasualBonusSlot.Presenters
{
    /// <summary>
    /// スロットゲーム全体を制御するPresenter（ステートレス）
    /// ModelとViewの橋渡しのみを行う
    /// </summary>
    public class SlotGamePresenter : IDisposable
    {
        // Model
        private readonly SlotGameModel _model;

        // View
        private readonly SlotGameView _view;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public SlotGamePresenter(SlotGameView view, SlotGameModel model)
        {
            _view = view;
            _model = model;

            // Viewからリールのシンボル順序を取得してModelに設定
            var symbolOrders = _view.ReelSymbolOrders;
            _model.SetReelSymbolOrders(symbolOrders.Left, symbolOrders.Center, symbolOrders.Right);

            // ViewからのイベントをModelに渡す
            _view.OnScreenTapped
                .Subscribe(_ => _model.OnScreenTap())
                .AddTo(_disposables);

            _view.OnStopButtonPressed
                .Subscribe(position => _model.OnStopButtonPressed(position))
                .AddTo(_disposables);

            // Modelの状態変化をViewに反映
            _model.Credit
                .Subscribe(credit => _view.UpdateCredit(credit))
                .AddTo(_disposables);

            // Modelのイベントに応じてViewを更新
            _model.OnGameStarted
                .Subscribe(e => _view.HandleGameStarted(e))
                .AddTo(_disposables);

            _model.OnReelStopped
                .Subscribe(e => _view.HandleReelStopped(e.Position, e.Symbol))
                .AddTo(_disposables);

            _model.OnAllReelsStopped
                .Subscribe(e => _view.HandleAllReelsStopped(e.WinType))
                .AddTo(_disposables);

            _model.OnGameOver
                .Subscribe(_ => _view.HandleGameOver())
                .AddTo(_disposables);

            _model.OnHapticTriggered
                .Subscribe(_ => _view.HandleHapticTriggered())
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
