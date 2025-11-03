namespace CasualBonusSlot.Models
{
    /// <summary>
    /// 当選役の種類
    /// </summary>
    public enum WinType
    {
        None,       // ハズレ
        Bell,       // ベル（払出3枚）
        Cherry,     // チェリー（払出1枚）
        Bonus       // ボーナス（払出100枚）
    }
}
