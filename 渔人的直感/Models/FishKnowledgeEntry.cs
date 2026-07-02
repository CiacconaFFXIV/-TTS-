using System.Collections.Generic;

namespace 渔人的直感.Models
{
    public sealed class FishKnowledgeEntry
    {
        public string FishKing { get; set; }
        public string Spot { get; set; }
        public string Duration { get; set; }
        public string Prerequisites { get; set; }

        public int DurationSeconds
        {
            get
            {
                if (string.IsNullOrEmpty(Duration))
                    return 0;
                var text = Duration.Trim().TrimEnd('s', 'S');
                return int.TryParse(text, out var seconds) ? seconds : 0;
            }
        }

        public List<FishPrerequisite> ParsePrerequisites()
        {
            var result = new List<FishPrerequisite>();
            if (string.IsNullOrWhiteSpace(Prerequisites))
                return result;

            foreach (var part in Prerequisites.Split('|'))
            {
                var trimmed = part.Trim();
                var colon = trimmed.LastIndexOf(':');
                if (colon <= 0)
                    continue;

                var name = trimmed.Substring(0, colon).Trim();
                if (!int.TryParse(trimmed.Substring(colon + 1), out var count))
                    continue;

                result.Add(new FishPrerequisite { Name = name, Required = count });
            }

            return result;
        }
    }

    public sealed class FishPrerequisite
    {
        public string Name { get; set; }
        public int Required { get; set; }
        public int Current { get; set; }
    }
}
