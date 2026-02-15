using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Content.Server._DV.Speech.SpeechConverter
{
    public enum CapitalizationMode
    {
        PerLetter, // matches capitalization per letter of the original word, perfect for similar word lengths, but also works for shorter replacements.
        FirstLetter, // follows the capitalization of the first letter of the original, the rest follows (used for single letter replacements)
        AllUpper, // makes everything all caps (this including AllLower isnt really used much, but has a use for meme accents.)
        AllLower // makes everything all lowercase
    }

    public enum MatchPosition
    {
        Anywhere, // matches ANY words, ANYTHING. ANYWHERE. (used for regexing, do not use this unless it's for vowel replacement or something)
        Nowhere, // matches whole words only
        Prefix, // matches only at the start of words
        Suffix, // matches only at the end of words
        Nothing // basically do the regex yourself
    }

    public sealed class ReplacementRule
    {
        public List<string> Patterns { get; }
        public List<string> Replacements { get; }
        public CapitalizationMode CapMode { get; }
        public double Probability { get; }
        public bool ApplyOnce { get; }
        public MatchPosition Position { get; }

        public ReplacementRule(
            IEnumerable<string> patterns,
            IEnumerable<string> replacements,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere)
        {
            Patterns = patterns.ToList();
            Replacements = replacements.ToList();
            CapMode = capMode;
            Probability = Math.Clamp(probability, 0, 1);
            ApplyOnce = applyOnce;
            Position = position;
        }
    }

    public sealed class SpeechConverterSystem
    {
        private readonly List<ReplacementRule> _rules = new();
        private readonly Random _random = new();

        public void AddRule(
            string pattern,
            string replacement,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere
        )
        {
            AddRule(new[] { pattern }, new[] { replacement }, capMode, probability, applyOnce, position);
        }

        public void AddRule(
            string pattern,
            IEnumerable<string> replacements,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere
        )
        {
            AddRule(new[] { pattern }, replacements, capMode, probability, applyOnce, position);
        }

        public void AddRule(
            IEnumerable<string> patterns,
            string replacement,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere
        )
        {
            AddRule(patterns, new[] { replacement }, capMode, probability, applyOnce, position);
        }

        public void AddRule(
            IEnumerable<string> patterns,
            IEnumerable<string> replacements,
            CapitalizationMode capMode = CapitalizationMode.PerLetter,
            double probability = 1.0,
            bool applyOnce = false,
            MatchPosition position = MatchPosition.Nowhere
        )
        {
            if (!patterns.Any() || !replacements.Any())
                throw new ArgumentException("Patterns and Replacements for the patterns must not be empty..");

            _rules.Add(new ReplacementRule(
                patterns,
                replacements,
                capMode,
                probability,
                applyOnce,
                position
            ));
        }

        private struct SpeechSegment
        {
            public string Text;
            public bool IsModified;

            public SpeechSegment(string text, bool isModified)
            {
                Text = text;
                IsModified = isModified;
            }
        }

        private static readonly Regex WordOnlyRegex = new Regex(@"^[\p{L}\p{N}_]+$", RegexOptions.Compiled);
        public sealed class ReplacementRule
        {
            public List<string> Patterns { get; }
            public List<string> Replacements { get; }
            public CapitalizationMode CapMode { get; }
            public double Probability { get; }
            public bool ApplyOnce { get; }
            public MatchPosition Position { get; }

            public Regex CompiledRegex { get; }

            public ReplacementRule(
                IEnumerable<string> patterns,
                IEnumerable<string> replacements,
                CapitalizationMode capMode,
                double probability,
                bool applyOnce,
                MatchPosition position)
            {
                Patterns = patterns.ToList();
                Replacements = replacements.ToList();
                CapMode = capMode;
                Probability = Math.Clamp(probability, 0, 1);
                ApplyOnce = applyOnce;
                Position = position;

                var pattern = "(" + string.Join("|", Patterns.Select(pat =>
                {
                    var esc = Regex.Escape(pat);
                    bool needsWordBoundary = WordOnlyRegex.IsMatch(pat);

                    return Position switch
                    {
                        MatchPosition.Prefix => needsWordBoundary ? @"\b" + esc : esc,
                        MatchPosition.Suffix => needsWordBoundary ? esc + @"\b" : esc,
                        MatchPosition.Nowhere => needsWordBoundary ? @"\b" + esc + @"\b" : esc,
                        MatchPosition.Anywhere => esc,
                        MatchPosition.Nothing => pat,
                        _ => esc
                    };
                })) + ")";

                CompiledRegex = new Regex(
                    pattern,
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant |
                    RegexOptions.Compiled
                );
            }
        }
        public string Process(string input)
        {
            if (!_rules.Any())
                return input;

            var segments = new List<SpeechSegment> { new SpeechSegment(input, false) };

            foreach (var rule in _rules)
            {
                string pattern = "(" + string.Join("|", rule.Patterns.Select(pat =>
                {
                    var esc = Regex.Escape(pat);
                    bool needsWordBoundary = WordOnlyRegex.IsMatch(pat);

                    return rule.Position switch
                    {
                        MatchPosition.Prefix => needsWordBoundary ? @"\b" + esc : esc,
                        MatchPosition.Suffix => needsWordBoundary ? esc + @"\b" : esc,
                        MatchPosition.Nowhere => needsWordBoundary ? @"\b" + esc + @"\b" : esc,
                        MatchPosition.Anywhere => esc,
                        MatchPosition.Nothing => pat,
                        _ => esc
                    };
                })) + ")";

                var potentialMatches = new List<(int SegmentIndex, Match Match)>();

                for (int i = 0; i < segments.Count; i++)
                {
                    if (segments[i].IsModified)
                        continue;

                    var matches = rule.CompiledRegex.Matches(segments[i].Text);

                    foreach (Match m in matches.Cast<Match>())
                    {
                        potentialMatches.Add((i, m));
                    }
                }

                if (potentialMatches.Count == 0)
                    continue;

                var nextSegments = new List<SpeechSegment>();

                if (rule.ApplyOnce)
                {
                    if (_random.NextDouble() > rule.Probability)
                        continue;

                    var chosen = potentialMatches[_random.Next(potentialMatches.Count)];

                    for (int i = 0; i < segments.Count; i++)
                    {
                        if (i != chosen.SegmentIndex)
                        {
                            nextSegments.Add(segments[i]);
                            continue;
                        }

                        var seg = segments[i];
                        var match = chosen.Match;

                        if (match.Index > 0)
                            nextSegments.Add(new SpeechSegment(seg.Text.Substring(0, match.Index), false));

                        string replacement = rule.Replacements[_random.Next(rule.Replacements.Count)];
                        string processedReplacement = ProcessBackreferences(match, replacement);
                        nextSegments.Add(new SpeechSegment(ApplyCapitalization(match.Value, processedReplacement, rule.CapMode), true));

                        if (match.Index + match.Length < seg.Text.Length)
                            nextSegments.Add(new SpeechSegment(seg.Text.Substring(match.Index + match.Length), false));
                    }
                }
                else
                {
                    var matchesBySegment = potentialMatches
                        .GroupBy(x => x.SegmentIndex)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.Match).OrderBy(m => m.Index).ToList());

                    for (int i = 0; i < segments.Count; i++)
                    {
                        if (segments[i].IsModified || !matchesBySegment.ContainsKey(i))
                        {
                            nextSegments.Add(segments[i]);
                            continue;
                        }

                        var seg = segments[i];
                        var matches = matchesBySegment[i];
                        int lastIndex = 0;

                        foreach (var match in matches)
                        {
                            if (match.Index > lastIndex)
                                nextSegments.Add(new SpeechSegment(seg.Text.Substring(lastIndex, match.Index - lastIndex), false));

                            if (_random.NextDouble() <= rule.Probability)
                            {
                                string replacement = rule.Replacements[_random.Next(rule.Replacements.Count)];
                                string processedReplacement = ProcessBackreferences(match, replacement);
                                nextSegments.Add(new SpeechSegment(ApplyCapitalization(match.Value, processedReplacement, rule.CapMode), true));
                            }
                            else
                            {
                                nextSegments.Add(new SpeechSegment(match.Value, false));
                            }

                            lastIndex = match.Index + match.Length;
                        }

                        if (lastIndex < seg.Text.Length)
                            nextSegments.Add(new SpeechSegment(seg.Text.Substring(lastIndex), false));
                    }
                }
                segments = nextSegments;
            }

            var builder = new StringBuilder();
            foreach (var seg in segments) builder.Append(seg.Text);
            return builder.ToString();
        }

        private string ProcessBackreferences(Match match, string replacement)
        {
            var result = replacement;
            for (int i = 0; i < match.Groups.Count; i++)
            {
                result = result.Replace($"${i}", match.Groups[i].Value);
            }
            return result;
        }

        private string ApplyCapitalization(string original, string replacement, CapitalizationMode capMode)
        {
            return capMode switch
            {
                CapitalizationMode.PerLetter => MapPerLetter(original, replacement),
                CapitalizationMode.FirstLetter => char.IsUpper(original[0]) ? replacement.ToUpperInvariant() : replacement.ToLowerInvariant(),
                CapitalizationMode.AllUpper => replacement.ToUpperInvariant(),
                CapitalizationMode.AllLower => replacement.ToLowerInvariant(),
                _ => replacement
            };
        }

        private string MapPerLetter(string original, string replacement)
        {
            var result = new char[replacement.Length];
            int originalIndex = 0;

            for (int i = 0; i < replacement.Length; i++)
            {
                while (originalIndex < original.Length && !char.IsLetter(original[originalIndex]))
                    originalIndex++;

                if (originalIndex < original.Length && char.IsUpper(original[originalIndex]))
                    result[i] = char.ToUpperInvariant(replacement[i]);
                else
                    result[i] = replacement[i];

                originalIndex++;
            }

            return new string(result);
        }
    }
}
