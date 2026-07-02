using System.Diagnostics;
using System.Text.RegularExpressions;

namespace 渔人的直感.Models
{
    public sealed class FishIntuitionTracker
    {
        private static readonly Regex CastSpotRegex =
            new Regex(@"在(?<spot>.+?)甩出了鱼线开始钓鱼", RegexOptions.Compiled);

        private static readonly Regex DiscoverSpotRegex =
            new Regex(@"发现了新钓场[“""](?<spot>.+?)[”""]！", RegexOptions.Compiled);

        private static readonly Regex RecordSpotRegex =
            new Regex(@"将新钓场[“""](?<spot>.+?)[”""]记录", RegexOptions.Compiled);

        private static readonly Regex CatchFishRegex =
            new Regex(@"成功钓上了(?<fish>.+?)（[\d.]+星寸）(?:×(?<count>\d+))?", RegexOptions.Compiled);

        private static readonly Regex StopFishingRegex =
            new Regex(@"收回了鱼线|收竿停止了钓鱼", RegexOptions.Compiled);

        private readonly FishKnowledgeRepository _repository;
        private readonly FishIntuition _display;

        public FishIntuitionTracker(FishKnowledgeRepository repository, FishIntuition display)
        {
            _repository = repository;
            _display = display;
        }

        public void HandleChatMessage(ChatLogEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Message))
                return;

            var message = entry.Message;

            if (StopFishingRegex.IsMatch(message))
            {
                _display.Hide();
                return;
            }

            if (TryHandleCatch(message))
                return;

            var spot = ExtractSpotName(message);
            if (string.IsNullOrEmpty(spot))
                return;

            ApplySpot(spot);
        }

        public void ClearOnMapChange()
        {
            if (!_display.IsActive)
                return;

            Debug.WriteLine("[FishIntuition] 检测到切换地图/海域，清空鱼识状态。");
            _display.Clear();
        }

        private bool TryHandleCatch(string message)
        {
            var match = CatchFishRegex.Match(message);
            if (!match.Success)
                return false;

            var fishName = match.Groups["fish"].Value.Trim();
            var count = 1;
            if (match.Groups["count"].Success &&
                int.TryParse(match.Groups["count"].Value, out var parsed) &&
                parsed > 0)
                count = parsed;

            for (var i = 0; i < count; i++)
                _display.TryHandleCaughtFish(fishName);

            return true;
        }

        private void ApplySpot(string spotName)
        {
            var entry = _repository.FindBySpot(spotName);
            if (entry == null)
            {
                Debug.WriteLine($"[FishIntuition] 钓场「{spotName}」未在鱼识数据中找到，清空显示。");
                _display.Clear();
                return;
            }

            if (string.Equals(_display.SpotName, spotName, System.StringComparison.Ordinal))
            {
                _display.Show();
                return;
            }

            _display.SetSpot(entry);
        }

        private static string ExtractSpotName(string message)
        {
            foreach (var regex in new[] { CastSpotRegex, DiscoverSpotRegex, RecordSpotRegex })
            {
                var match = regex.Match(message);
                if (match.Success)
                    return match.Groups["spot"].Value.Trim();
            }

            return null;
        }
    }
}
