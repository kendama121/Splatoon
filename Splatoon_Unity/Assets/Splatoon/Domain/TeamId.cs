namespace Splatoon.Domain
{
    /// <summary>
    /// チーム識別子。最大4チームまで対応(本家は2-4チーム可、トリカラで3チーム)。
    /// スプラットマップRGBAチャネルと1対1対応する。
    /// </summary>
    public enum TeamId
    {
        /// <summary>未所属(中立)</summary>
        Neutral = -1,
        /// <summary>チーム0(スプラットマップRチャネル)</summary>
        Alpha = 0,
        /// <summary>チーム1(スプラットマップGチャネル)</summary>
        Bravo = 1,
        /// <summary>チーム2(スプラットマップBチャネル、トリカラ用)</summary>
        Charlie = 2,
        /// <summary>チーム3(スプラットマップAチャネル、予備)</summary>
        Delta = 3,
    }
}
