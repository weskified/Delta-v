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
        _st.AddRule(@"(?<=[aeiou])r(?=[^aeiou]|$)", new[] { "rr", "h", "" }, probability: 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])l(?=[^aeiou])", new[] { "ll", "w", "ul" }, probability: 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])s(?=[kpt]\b)", new[] { "s", "sk", "'" }, probability: 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])[nml][tdsk]\b", new[] { "n'", "m'", "nt", "nd" }, probability: 0.45, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=\w{2})e(?=\w{2})", new[] { "uh", "a", "" }, probability: 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"a(?=[^aeiou]{1,2}\b)", new[] { "uh", "ah" }, probability: 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[^aeiou])ow(?=[^aeiou]|$)", new[] { "aw", "oh", "o" }, probability: 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[^aeiou])[ae]y\b", new[] { "eh", "ey", "aay" }, probability: 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"ee(?=[^aeiou]|$)", new[] { "eee", "eeh", "ii" }, probability: 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"oo(?=[^aeiou]|$)", new[] { "ooo", "uuu", "ooh" }, probability: 0.3, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[^aeiou])aa(?=[^aeiou]|$)", new[] { "aaa", "ahh" }, probability: 0.25, position: MatchPosition.Nothing);

        // consonants
        _st.AddRule(@"s(?=[aeiou])", new[] { "sh", "sz", "ss" }, probability: 0.25, position: MatchPosition.Anywhere);
        _st.AddRule(@"z", new[] { "zh", "zzh" }, probability: 0.3, position: MatchPosition.Anywhere);
        _st.AddRule("str", "shtr", probability: 0.7, position: MatchPosition.Anywhere);
        _st.AddRule("thr", "shr", probability: 0.7, position: MatchPosition.Anywhere);
        _st.AddRule("shr", "shh", probability: 0.7, position: MatchPosition.Anywhere);
        _st.AddRule(@"th", new[] { "f", "d", "t" }, probability: 0.45, position: MatchPosition.Anywhere);
        _st.AddRule(@"([bcdfghjklmnpqrstvwxyz])\1", new[] { "$1$1", "$1$1$1$1" }, probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"\b(scr|spr|spl|squ)", new[] { "sk", "sp", "s" }, probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"\bwh", new[] { "w", "wuh" }, probability: 0.5, position: MatchPosition.Nothing);

        // words
        _st.AddRule(@"[ts]ion\b", new[] { "sh'n", "shun", "shnn" }, probability: 0.65, position: MatchPosition.Nothing);
        _st.AddRule(@"ly\b", new[] { "leh", "li", "l'" }, probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"er\b", new[] { "uh", "a", "err" }, probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"le\b", new[] { "l", "ul", "el" }, probability: 0.35, position: MatchPosition.Nothing);
        _st.AddRule(@"ck\b", new[] { "k", "'" }, probability: 0.5, position: MatchPosition.Nothing);
        _st.AddRule("ing", new[] { "in'", "in" }, probability: 0.7, position: MatchPosition.Suffix);
        _st.AddRule("ght", new[] { "gh'", "t" }, probability: 0.45, position: MatchPosition.Suffix);
        _st.AddRule("mb", new[] { "m", "m'" }, probability: 0.5, position: MatchPosition.Suffix);

        _st.AddRule(@"\bhe\b", "'e", probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(@"\bhim\b", "'im", probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(@"\bher\b", "'er", probability: 0.7, position: MatchPosition.Nowhere);

        _st.AddRule("can", new[] { "c'n", "cn", "kn" }, probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("and", new[] { "an'", "n", "'n", "nd" }, probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("the", new[] { "th'", "da", "d'", "thuh" }, probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("to", new[] { "t'", "tuh", "ta" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("for", new[] { "fr", "fer", "f'r" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("of", new[] { "'f", "uh", "uv" }, probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("are", new[] { "r", "'re", "ar", "er" }, probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("is", new[] { "'s", "s", "z" }, probability: 0.45, position: MatchPosition.Nowhere);
        _st.AddRule("what", new[] { "wha'", "wut", "whuh", "wa" }, probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("with", new[] { "wif", "wit", "w'" }, probability: 0.55, position: MatchPosition.Nowhere);
        _st.AddRule("just", new[] { "jus'", "jus", "jst" }, probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("that", new[] { "tha'", "dat", "th't" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("this", new[] { "dis", "th's" }, probability: 0.45, position: MatchPosition.Nowhere);

        _st.AddRule(new[] { "leave", "let" }, new[] { "le'mme", "lea'", "le..." }, probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("alone", new[] { "'lone", "alonnne" }, probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("stop", new[] { "st'p", "stoppit", "stooop" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("minutes", new[] { "mins", "min'sh", "mi'ts" }, probability: 0.8, position: MatchPosition.Nowhere);
        _st.AddRule("light", new[] { "li't", "bright...", "too bright" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule("please", new[] { "pleaj", "p'ease", "plz" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "hello", "hi", "hey" }, new[] { "'lo", "h-huh?", "m'h?" }, probability: 0.7, position: MatchPosition.Nowhere);

        // slurring
        _st.AddRule(new[] { "going to", "gonna" }, new[] { "gunna", "gon'", "g'nna" }, probability: 0.8, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "want to", "wanna" }, new[] { "wann'", "wan'", "wana", "wunna" }, probability: 0.8, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "have to", "gotta" }, new[] { "gott'", "haft'", "hafta" }, probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "I am", "I'm" }, new[] { "m'", "am", "'m" }, probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("don't know", new[] { "dunno", "dun'", "d'no" }, probability: 0.85, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "get", "got", "getting" }, new[] { "ge'", "go'", "gettin'", "git", "geh" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "about", "around" }, new[] { "'bout", "'round", "bout", "round" }, probability: 0.6, position: MatchPosition.Nowhere);
        _st.AddRule("probably", new[] { "prob'ly", "probly", "prolly" }, probability: 0.8, position: MatchPosition.Nowhere);
        _st.AddRule("actually", new[] { "akshully", "actchully", "ackshly" }, probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("really", new[] { "rly", "realleh", "r'lly" }, probability: 0.65, position: MatchPosition.Nowhere);
        _st.AddRule("whatever", new[] { "whatevr", "whatevs", "wh'tever" }, probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule("because", new[] { "cuz", "c'z", "coz", "cus" }, probability: 0.75, position: MatchPosition.Nowhere);
        _st.AddRule("okay", new[] { "'kay", "k", "mmkay", "okeh" }, probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "what's that", "whats that" }, "wazzat", probability: 0.7, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "yes", "yeah" }, new[] { "yeh", "yaa", "yhh", "mmhm" }, probability: 0.5, position: MatchPosition.Nowhere);
        _st.AddRule(new[] { "no", "nah" }, new[] { "nuh", "nah", "mm-mm", "neh" }, probability: 0.5, position: MatchPosition.Nowhere);

        _st.AddRule(@"!", new[] { "...", "..?", ".." }, position: MatchPosition.Suffix);
        _st.AddRule(@"?", new[] { "...?", "..?" }, probability: 0.5, position: MatchPosition.Suffix);
        _st.AddRule(@"([aeiou])", "$1$1$1...", probability: 0.15, position: MatchPosition.Suffix);

        _st.AddRule(" ", new[]
            {
                " ...mmgh... ", " ...mrrph... ", " ...hnnnngh... ",
                " ...rgggh... ", " ...nnn-nn... ", " ...mm-hmm... ", " ...mrrnn... ",
                " ...*huffs*... ", " ...*grumbles*... ", " ...*exhales*... ", " ...*sighs*... ",
                " ...*groans*... ", " ...*yawns*... ", " ...ughhh... ", " ...rgh... ",
                " ...mnhn... ", " ...uhhh-mm... ", " ...nnnhhh... ", " ...m-mm... ",
                " ...wh-.. ", " ...nnh... ", " ...mmf... "
            },
            probability: 0.1, position: MatchPosition.Anywhere, applyOnce: true
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
