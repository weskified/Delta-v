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

        // voewl
        _st.AddRule(@"(?<=[aeiou])r(?=[^aeiou]|$)", new[] { "rr", "h", "" }, CapitalizationMode.PerLetter, 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])l(?=[^aeiou])", new[] { "ll", "w", "ul" }, CapitalizationMode.PerLetter, 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])s(?=[kpt]\b)", new[] { "s", "sk", "'" }, CapitalizationMode.PerLetter, 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])[nml][tdsk]\b", new[] { "n'", "m'", "nt", "nd" }, CapitalizationMode.PerLetter, 0.45, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=\w{2})e(?=\w{2})", new[] { "uh", "a", "" }, CapitalizationMode.PerLetter, 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"a(?=[^aeiou]{1,2}\b)", new[] { "uh", "ah" }, CapitalizationMode.PerLetter, 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[^aeiou])ow(?=[^aeiou]|$)", new[] { "aw", "oh", "o" }, CapitalizationMode.PerLetter, 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[^aeiou])[ae]y\b", new[] { "eh", "ey", "aay" }, CapitalizationMode.PerLetter, 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"ee(?=[^aeiou]|$)", new[] { "eee", "eeh", "ii" }, CapitalizationMode.PerLetter, 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"oo(?=[^aeiou]|$)", new[] { "ooo", "uuu", "ooh" }, CapitalizationMode.PerLetter, 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[^aeiou])aa(?=[^aeiou]|$)", new[] { "aaa", "ahh" }, CapitalizationMode.PerLetter, 0.25, position: MatchPosition.Nothing);

        // consonants
        _st.AddRule(@"s(?=[aeiou])", new[] { "sh", "sz", "ss" }, CapitalizationMode.PerLetter, 0.25, position: MatchPosition.Anywhere);
        _st.AddRule(@"z", new[] { "zh", "zzh" }, CapitalizationMode.PerLetter, 0.3, position: MatchPosition.Anywhere);
        _st.AddRule("str", "shtr", CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Anywhere);
        _st.AddRule("thr", "shr", CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Anywhere);
        _st.AddRule("shr", "shh", CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Anywhere);
        _st.AddRule(@"th", new[] { "f", "d", "t" }, CapitalizationMode.PerLetter, 0.45, position: MatchPosition.Anywhere);
        _st.AddRule(@"([bcdfghjklmnpqrstvwxyz])\1", new[] { "$1$1", "$1$1$1$1" }, CapitalizationMode.PerLetter, 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"\b(scr|spr|spl|squ)", new[] { "sk", "sp", "s" }, CapitalizationMode.PerLetter, 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"\bwh", new[] { "w", "wuh" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nothing);

        // words
        _st.AddRule(@"[ts]ion\b", new[] { "sh'n", "shun", "shnn" }, CapitalizationMode.PerLetter, 0.65, position: MatchPosition.Nothing);
        _st.AddRule(@"ly\b", new[] { "leh", "li", "l'" }, CapitalizationMode.PerLetter, 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"er\b", new[] { "uh", "a", "err" }, CapitalizationMode.PerLetter, 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"le\b", new[] { "l", "ul", "el" }, CapitalizationMode.PerLetter, 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"ck\b", new[] { "k", "'" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nothing);
        _st.AddRule("ing", new[] { "in'", "in" }, CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Suffix);
        _st.AddRule("ght", new[] { "gh'", "t" }, CapitalizationMode.PerLetter, 0.45, position: MatchPosition.Suffix);
        _st.AddRule("mb", new[] { "m", "m'" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Suffix);

        _st.AddRule(@"\bhe\b", "'e", CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(@"\bhim\b", "'im", CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(@"\bher\b", "'er", CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);

        _st.AddRule("can", new[] { "c'n", "cn", "kn" }, CapitalizationMode.PerLetter, 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("and", new[] { "an'", "n", "'n", "nd" }, CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("the", new[] { "th'", "da", "d'", "thuh" }, CapitalizationMode.PerLetter, 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("to", new[] { "t'", "tuh", "ta" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("for", new[] { "fr", "fer", "f'r" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("of", new[] { "'f", "uh", "uv" }, CapitalizationMode.PerLetter, 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("are", new[] { "r", "'re", "ar", "er" }, CapitalizationMode.PerLetter, 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("is", new[] { "'s", "s", "z" }, CapitalizationMode.PerLetter, 0.45, position: MatchPosition.Nowhere);
        _st.AddRule("what", new[] { "wha'", "wut", "whuh", "wa" }, CapitalizationMode.PerLetter, 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("with", new[] { "wif", "wit", "w'" }, CapitalizationMode.PerLetter, 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("just", new[] { "jus'", "jus", "jst" }, CapitalizationMode.PerLetter, 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("that", new[] { "tha'", "dat", "th't" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("this", new[] { "dis", "th's" }, CapitalizationMode.PerLetter, 0.45, position: MatchPosition.Nowhere);

        _st.AddRule(new[] { "leave", "let" }, new[] { "le'mme", "lea'", "le..." }, CapitalizationMode.PerLetter, 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("alone", new[] { "'lone", "alonnne" }, CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("stop", new[] { "st'p", "stoppit", "stooop" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("minutes", new[] { "mins", "min'sh", "mi'ts" }, CapitalizationMode.PerLetter, 0.8, position: MatchPosition.Nowhere);
        _st.AddRule("light", new[] { "li't", "bright...", "too bright" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("please", new[] { "pleaj", "p'ease", "plz" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "hello", "hi", "hey" }, new[] { "'lo", "h-huh?", "m'h?" }, CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Nowhere);

        // slurring
        _st.AddRule(new[] { "going to", "gonna" }, new[] { "gunna", "gon'", "g'nna" }, CapitalizationMode.PerLetter, 0.8, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "want to", "wanna" }, new[] { "wann'", "wan'", "wana", "wunna" }, CapitalizationMode.PerLetter, 0.8, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "have to", "gotta" }, new[] { "gott'", "haft'", "hafta" }, CapitalizationMode.PerLetter, 0.75, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "I am", "I'm" }, new[] { "m'", "am", "'m" }, CapitalizationMode.PerLetter, 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("don't know", new[] { "dunno", "dun'", "d'no" }, CapitalizationMode.PerLetter, 0.85, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "get", "got", "getting" }, new[] { "ge'", "go'", "gettin'", "git", "geh" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "about", "around" }, new[] { "'bout", "'round", "bout", "round" }, CapitalizationMode.PerLetter, 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("probably", new[] { "prob'ly", "probly", "prolly" }, CapitalizationMode.PerLetter, 0.8, position: MatchPosition.Nowhere);
        _st.AddRule("actually", new[] { "akshully", "actchully", "ackshly" }, CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("really", new[] { "rly", "realleh", "r'lly" }, CapitalizationMode.PerLetter, 0.65, position: MatchPosition.Nowhere);
        _st.AddRule("whatever", new[] { "whatevr", "whatevs", "wh'tever" }, CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("because", new[] { "cuz", "c'z", "coz", "cus" }, CapitalizationMode.PerLetter, 0.75, position: MatchPosition.Nowhere);
        _st.AddRule("okay", new[] { "'kay", "k", "mmkay", "okeh" }, CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "what's that", "whats that" }, "wazzat", CapitalizationMode.PerLetter, 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "yes", "yeah" }, new[] { "yeh", "yaa", "yhh", "mmhm" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "no", "nah" }, new[] { "nuh", "nah", "mm-mm", "neh" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Nowhere);

        _st.AddRule(@"!", new[] { "...", "..?", ".." }, CapitalizationMode.PerLetter, position: MatchPosition.Suffix);
        _st.AddRule(@"?", new[] { "...?", "..?" }, CapitalizationMode.PerLetter, 0.5, position: MatchPosition.Suffix);
        _st.AddRule(@"([aeiou])", "$1$1$1...", CapitalizationMode.PerLetter, 0.15, position: MatchPosition.Suffix);

        _st.AddRule(" ", new[]
            {
                " ...mmgh... ", " ...mrrph... ", " ...hnnnngh... ",
                " ...rgggh... ", " ...nnn-nn... ", " ...mm-hmm... ", " ...mrrnn... ",
                " ...*huffs*... ", " ...*grumbles*... ", " ...*exhales*... ", " ...*sighs*... ",
                " ...*groans*... ", " ...*yawns*... ", " ...ughhh... ", " ...rgh... ",
                " ...mnhn... ", " ...uhhh-mm... ", " ...nnnhhh... ", " ...m-mm... ",
                " ...wh-.. ", " ...nnh... ", " ...mmf... "
            },
            CapitalizationMode.AllLower, 0.1, position: MatchPosition.Anywhere, applyOnce: true
        );

        SubscribeLocalEvent<SleepyMumbleComponent, AccentGetEvent>(OnAccent);
    }

    private static readonly Regex ExclamationRegex = new(@"!{2,}", RegexOptions.Compiled);
    private static readonly Regex QuestionRegex = new(@"\?{2,}", RegexOptions.Compiled);
    private void OnAccent(EntityUid uid, SleepyMumbleComponent component, AccentGetEvent args)
    {
        // because sleepy people dont shout we just make it so they CAN'T..
        var original = args.Message;

        var message = string.IsNullOrEmpty(original)
            ? original
            : char.ToUpperInvariant(original[0]) + original.Substring(1).ToLowerInvariant();
        message = ExclamationRegex.Replace(message, "!");
        message = QuestionRegex.Replace(message, "?");
        args.Message = _st.Process(message);
    }
}
