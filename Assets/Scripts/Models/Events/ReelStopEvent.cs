namespace CasualBonusSlot.Models
{
    /// <summary>
    /// リール停止時のイベント情報
    /// </summary>
    public struct ReelStopEvent
    {
        public ReelPosition Position;
        public ReelSymbol Symbol;
    }
}
