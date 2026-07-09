using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace 渔人的直感.Models
{
    public enum FishIntuitionMode
    {
        None,
        Prerequisites,
        Countdown
    }

    public sealed class FishIntuition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string SpotName { get; private set; }
        public string FishKing { get; private set; }
        public int DurationSeconds { get; private set; }
        public FishIntuitionMode Mode { get; private set; }
        public IList<FishPrerequisite> Prerequisites { get; } = new List<FishPrerequisite>();

        private bool _isShown;
        private float _buffRemainingSeconds;
        private int _lastNotifiedCountdownSecond = -1;

        public bool HasSpotData => !string.IsNullOrEmpty(SpotName);

        public bool IsActive => Mode != FishIntuitionMode.None && HasSpotData;

        public string DisplayText
        {
            get
            {
                if (!IsActive || !_isShown)
                    return string.Empty;

                if (Mode == FishIntuitionMode.Countdown)
                    return $"鱼识：{RemainingCountdownSeconds}";

                var builder = new StringBuilder();
                for (var i = 0; i < Prerequisites.Count; i++)
                {
                    if (i > 0)
                        builder.Append("  ");
                    var item = Prerequisites[i];
                    builder.Append(item.Name).Append('：').Append(item.Current).Append('/').Append(item.Required);
                }

                return builder.ToString();
            }
        }

        public int RemainingCountdownSeconds =>
            Mode == FishIntuitionMode.Countdown
                ? Math.Max(0, (int)Math.Ceiling(_buffRemainingSeconds))
                : 0;

        public System.Windows.Visibility Visibility =>
            _isShown && IsActive
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

        public bool AllPrerequisitesMet =>
            Mode == FishIntuitionMode.Prerequisites &&
            Prerequisites.Count > 0 &&
            Prerequisites.All(p => p.Current >= p.Required);

        public void SetSpot(FishKnowledgeEntry entry)
        {
            SpotName = entry?.Spot;
            FishKing = entry?.FishKing;
            DurationSeconds = entry?.DurationSeconds ?? 0;
            Prerequisites.Clear();
            Mode = FishIntuitionMode.None;

            if (entry != null)
            {
                foreach (var item in entry.ParsePrerequisites())
                    Prerequisites.Add(new FishPrerequisite { Name = item.Name, Required = item.Required });
                Mode = FishIntuitionMode.Prerequisites;
            }

            _isShown = IsActive;
            NotifyChanged();
            DebugWrite($"匹配钓场「{SpotName}」鱼王「{FishKing}」前置: {DisplayText}");
        }

        public void Hide()
        {
            if (!_isShown)
                return;

            _isShown = false;
            NotifyChanged();
            DebugWrite($"已隐藏（保留数据）: {Mode} / {DisplayText}");
        }

        public void Show()
        {
            if (!IsActive || _isShown)
                return;

            _isShown = true;
            NotifyChanged();
            DebugWrite($"已恢复显示: {DisplayText}");
        }

        public void Clear()
        {
            SpotName = null;
            FishKing = null;
            DurationSeconds = 0;
            Prerequisites.Clear();
            Mode = FishIntuitionMode.None;
            _isShown = false;
            _buffRemainingSeconds = 0f;
            _lastNotifiedCountdownSecond = -1;
            NotifyChanged();
            DebugWrite("已清空鱼识状态。");
        }

        public void BeginCountdownFromBuff(float remainingSeconds)
        {
            if (!HasSpotData || remainingSeconds <= 0f)
                return;

            Mode = FishIntuitionMode.Countdown;
            _buffRemainingSeconds = remainingSeconds;
            _lastNotifiedCountdownSecond = -1;
            _isShown = true;
            NotifyChanged();
            NotifyLayoutRequired();
            DebugWrite($"鱼识 Buff 同步倒计时 {remainingSeconds:F1}s");
        }

        public void UpdateCountdownFromBuff(float remainingSeconds)
        {
            if (Mode != FishIntuitionMode.Countdown)
                return;

            _buffRemainingSeconds = remainingSeconds;
            if (remainingSeconds <= 0f)
            {
                EndCountdownFromBuff();
                return;
            }

            var remaining = RemainingCountdownSeconds;
            if (remaining == _lastNotifiedCountdownSecond)
                return;

            _lastNotifiedCountdownSecond = remaining;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
        }

        public void EndCountdownFromBuff()
        {
            if (Mode != FishIntuitionMode.Countdown)
                return;

            DebugWrite("鱼识 Buff 已消失，结束倒计时。");
            ResetPrerequisites();
        }

        private void NotifyLayoutRequired()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LayoutRequired"));
        }

        public void ResetPrerequisites()
        {
            if (!HasSpotData)
            {
                Clear();
                return;
            }

            foreach (var item in Prerequisites)
                item.Current = 0;

            Mode = FishIntuitionMode.Prerequisites;
            _buffRemainingSeconds = 0f;
            _lastNotifiedCountdownSecond = -1;
            NotifyChanged();
            DebugWrite($"重置为前置状态{( _isShown ? "" : "（保持隐藏）")}: {DisplayText}");
        }

        public bool TryIncrementFish(string fishName, int count = 1)
        {
            if (!_isShown || Mode != FishIntuitionMode.Prerequisites || string.IsNullOrEmpty(fishName))
                return false;

            var matched = false;
            foreach (var item in Prerequisites)
            {
                if (item.Name != fishName)
                    continue;

                item.Current = Math.Min(item.Current + count, item.Required);
                matched = true;
            }

            if (!matched)
                return false;

            NotifyChanged();
            DebugWrite($"前置进度更新: {fishName} +{count} → {DisplayText}");

            return true;
        }

        public bool TryHandleCaughtFish(string fishName)
        {
            if (string.IsNullOrEmpty(fishName) || !HasSpotData)
                return false;

            if (IsFishKing(fishName))
            {
                if (Mode == FishIntuitionMode.Countdown)
                {
                    DebugWrite($"钓上鱼王「{fishName}」，重置前置状态。");
                    ResetPrerequisites();
                    return true;
                }

                return false;
            }

            if (Mode == FishIntuitionMode.Countdown)
                return false;

            return TryIncrementFish(fishName);
        }

        private bool IsFishKing(string fishName)
        {
            if (string.IsNullOrEmpty(FishKing))
                return false;

            return fishName == FishKing ||
                   fishName.Contains(FishKing) ||
                   FishKing.Contains(fishName);
        }

        private void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visibility)));
        }

        private static void DebugWrite(string message)
        {
            System.Diagnostics.Debug.WriteLine("[FishIntuition] " + message);
        }
    }
}
