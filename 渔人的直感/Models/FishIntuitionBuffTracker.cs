using System;

namespace 渔人的直感.Models
{
    /// <summary>
    /// 根据鱼识 Buff（568）的出现、消失与剩余时间同步鱼识倒计时。
    /// </summary>
    public sealed class FishIntuitionBuffTracker
    {
        private const float RefreshThresholdSeconds = 1.5f;

        private readonly FishIntuition _display;
        private bool _hadBuff;
        private float _lastRemaining;

        public FishIntuitionBuffTracker(FishIntuition display)
        {
            _display = display;
        }

        public void Reset()
        {
            _hadBuff = false;
            _lastRemaining = 0f;
        }

        public void Poll(SigScanner scanner, IntPtr statusArrayPtr, Action<Action> dispatch)
        {
            if (scanner == null || statusArrayPtr == IntPtr.Zero || dispatch == null)
                return;

            var hasBuff = BuffWatcher.TryGetBuffRemaining(
                scanner,
                statusArrayPtr,
                (short)Data.FishIntuitionBuffId,
                out var remaining);

            if (hasBuff)
            {
                var isNew = !_hadBuff;
                var isRefresh = _hadBuff && remaining > _lastRemaining + RefreshThresholdSeconds;

                dispatch(() =>
                {
                    if (!_display.HasSpotData)
                        return;

                    if (isNew || isRefresh)
                        _display.BeginCountdownFromBuff(remaining);
                    else
                        _display.UpdateCountdownFromBuff(remaining);
                });

                _hadBuff = true;
                _lastRemaining = remaining;
                return;
            }

            if (_hadBuff)
            {
                dispatch(() => _display.EndCountdownFromBuff());
            }

            _hadBuff = false;
            _lastRemaining = 0f;
        }
    }
}
