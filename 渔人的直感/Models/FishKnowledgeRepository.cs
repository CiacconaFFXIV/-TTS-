using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;

namespace 渔人的直感.Models
{
    public sealed class FishKnowledgeRepository
    {
        private readonly List<FishKnowledgeEntry> _entries = new List<FishKnowledgeEntry>();

        public void Load()
        {
            _entries.Clear();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "FishKnowledge.json");
            if (!File.Exists(path))
            {
                Debug.WriteLine($"[FishIntuition] 未找到鱼识数据: {path}");
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                var serializer = new JavaScriptSerializer();
                var loaded = serializer.Deserialize<List<FishKnowledgeEntry>>(json);
                if (loaded != null)
                    _entries.AddRange(loaded);
                Debug.WriteLine($"[FishIntuition] 已加载 {_entries.Count} 条鱼识数据。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FishIntuition] 加载鱼识数据失败: {ex.Message}");
            }
        }

        public FishKnowledgeEntry FindBySpot(string spotName)
        {
            if (string.IsNullOrWhiteSpace(spotName))
                return null;

            foreach (var entry in _entries)
            {
                if (string.Equals(entry.Spot, spotName, StringComparison.Ordinal))
                    return entry;
            }

            foreach (var entry in _entries)
            {
                if (entry.Spot != null && (entry.Spot.Contains(spotName) || spotName.Contains(entry.Spot)))
                    return entry;
            }

            return null;
        }
    }
}
