using Content.Shared.Speech;
using Content.Server._DV.Speech.Components;
using Content.Server._DV.Speech.SpeechConverter;

using System.Text.RegularExpressions;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class SleepyMumbleSystem : EntitySystem
{
    private readonly SpeechConverterSystem _st = new();

    public override void Initialize()
    {
        base.Initialize();

        _st.AddRule(@"(?<=[aeiou])l(?=[^aeiou])", ["w", "ul", "'l"], probability: 0.25, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])[nml][tdk]\b", ["n'", "m'", "nd"], probability: 0.45, position: MatchPosition.Nothing);
        _st.AddRule(@"a(?=[^aeiou]{1,2}\b)", ["uh", "ah"], probability: 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[^aeiou])ow(?=[^aeiou]|$)", ["aw", "oh"], probability: 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"ee(?=[^aeiou]|$)", ["eee", "eeh"], probability: 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"oo(?=[^aeiou]|$)", ["ooo", "uuh"], probability: 0.3, position: MatchPosition.Nothing);

        _st.AddRule("the", "da", probability: 0.4, position: MatchPosition.Anywhere);
        _st.AddRule("with", "wif", probability: 0.4, position: MatchPosition.Anywhere);
        _st.AddRule(@"\bwh", ["w", "wuh"], probability: 0.5, position: MatchPosition.Nothing);

        _st.AddRule(@"[ts]ion\b", ["sh'n", "shun"], probability: 0.65, position: MatchPosition.Nothing);
        _st.AddRule(@"ly\b", ["leh", "l'"], probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"er\b", ["uh", "ur"], probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"ck\b", ["g", "'"], probability: 0.3, position: MatchPosition.Nothing);
        _st.AddRule("ing", ["in'", "in"], probability: 0.8, position: MatchPosition.Suffix);
        _st.AddRule("ght", ["gh'", "t"], probability: 0.45, position: MatchPosition.Suffix);

        // word slurring
        _st.AddRule(@"\bhe\b", "'e", probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule(@"\bhim\b", "'im", probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule(@"\bher\b", "'er", probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("can", ["c'n", "cn"], probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("and", ["an'", "n", "'n"], probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("the", ["th'", "d'", "thuh"], probability: 0.55, position: MatchPosition.Nowhere);w
        _st.AddRule("to", ["t'", "tuh", "ta"], probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("for", ["fr", "fer"], probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("of", ["'f", "uh", "uv"], probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("are", ["r", "er"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("what", ["wha'", "wut", "whuh"], probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("with", ["wif", "wit", "w'"], probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("just", ["jus'", "jus"], probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("that", ["tha'", "dat"], probability: 0.5, position: MatchPosition.Nowhere);

        _st.AddRule(["hello", "hi", "hey"], ["'lo...", "m'h?", "wha...?"], probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(["going to", "gonna"], ["gunna", "gon'"], probability: 0.8, position: MatchPosition.Nowhere);
        _st.AddRule(["want to", "wanna"], ["wann'", "wunna"], probability: 0.8, position: MatchPosition.Nowhere);
        _st.AddRule(["have to", "gotta"], ["hafta", "haft'"], probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule("don't know", ["dunno", "d'no..."], probability: 0.85, position: MatchPosition.Nowhere);
        _st.AddRule(["get", "got", "getting"], ["ge'", "go'", "gettin'"], probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("probably", ["prob'ly", "pro'ly"], probability: 0.8, position: MatchPosition.Nowhere);
        _st.AddRule("actually", ["actchully", "akshly"], probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("because", ["cuz", "coz"], probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule("okay", ["'kay", "mkay", "mhm..."], probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(["what's that", "whats that"], "wazzat...", probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(["yes", "yeah"], ["yeah...", "mhm", "yuh"], probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(["no", "nah"], ["nuh...", "nah...", "nnn"], probability: 0.5, position: MatchPosition.Nowhere);

        _st.AddRule(@"\.", ["...", ".."], probability: 0.4, position: MatchPosition.Suffix);
        _st.AddRule(@"\?", ["...?", "..?"], probability: 0.5, position: MatchPosition.Suffix);
        _st.AddRule(@"([aeiou])", "$1$1$1...", probability: 0.1, position: MatchPosition.Suffix);

        _st.AddRule(" ", new[]
            {
                " ...mmgh... ", " ...mnh... ", " ...hnnn... ",
                " ...rgh... ", " ...nuh... ", " ...mhm... ", " ...mrrnn... ",
                " ...hwaaah... ", " ...ugh... ", " ...uhhh... ", " ...zzz... ",
                " ...m-mm... ", " ...wha... ", " ...nnh... "
            },
            probability: 0.08, position: MatchPosition.Anywhere, applyOnce: true
        );

        SubscribeLocalEvent<SleepyMumbleComponent, AccentGetEvent>(OnAccent);
    }

    private static readonly Regex ExclamationRegex = new(@"!+", RegexOptions.Compiled);
    private static readonly Regex QuestionRegex = new(@"\?{2,}", RegexOptions.Compiled);
    private void OnAccent(EntityUid uid, SleepyMumbleComponent component, AccentGetEvent args)
    {
        var original = args.Message;

        var message = string.IsNullOrEmpty(original)
            ? original
            : char.ToUpperInvariant(original[0]) + original.Substring(1).ToLowerInvariant();
        message = ExclamationRegex.Replace(message, "...");
        message = QuestionRegex.Replace(message, "...?");

        args.Message = _st.Process(message);
    }
}
