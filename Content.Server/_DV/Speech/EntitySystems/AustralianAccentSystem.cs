using Content.Shared.Speech;
using Content.Server._DV.Speech.Components;
using Content.Server._DV.Speech.SpeechConverter;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class AustralianAccentSystem : EntitySystem
{
    private readonly SpeechConverterSystem _st = new();

    public override void Initialize()
    {
        base.Initialize();

        // roles and stuff
        _st.AddRule(
            new[]
            {
                "friend", "pal", "buddy", "dude", "man", "guy",
                "bro", "brother", "sir", "mister", "boss"
            },
            "mate"
        );

        _st.AddRule(
            new[] { "girl", "lady", "woman", "ma'am", "maam", "miss", "chick" },
            new[] { "sheila", "missus" }
        );

        _st.AddRule(new[] { "engineer", "engi" }, "sparkie");
        _st.AddRule(new[] { "security", "sec" }, new[] { "coppa", "copper", "walloper" });
        _st.AddRule("courier", "postie");
        _st.AddRule("janitor", "garbo");
        _st.AddRule("captain", "skip");

        // gen vocab / expressions
        _st.AddRule(new[] { "hello", "hi" }, "g'day", applyOnce: true);
        _st.AddRule(new[] { "thanks", "thank you" }, "cheers");
        _st.AddRule(new[] { "very", "really" }, "bloody", probability: 0.65);
        _st.AddRule("totally", "dead set", probability: 0.50);
        _st.AddRule("think", "reckon", probability: 0.70);
        _st.AddRule("yes", "nah yea", probability: 0.35, applyOnce: true);
        _st.AddRule("no", "yea nah", probability: 0.35, applyOnce: true);
        _st.AddRule(new[] { "yes", "yeah" }, "yea");
        _st.AddRule("no", "nah");
        _st.AddRule("you", "ya");
        _st.AddRule("your", "yer");
        _st.AddRule("my", "me", probability: 0.60);
        _st.AddRule(new[] { "going to", "going" }, "gonna");
        _st.AddRule(new[] { "don't know", "dont know" }, "dunno");
        _st.AddRule("fuck", "fark");
        _st.AddRule("fucking", "farking");
        _st.AddRule("hell", "'ell");
        _st.AddRule("shit", "shite");

        // slangs
        _st.AddRule("afternoon", "arvo");
        _st.AddRule("biscuits", "bikkies");
        _st.AddRule("chocolate", "chocky");
        _st.AddRule("mosquito", "mozzie");
        _st.AddRule("present", "prezzy");

        _st.AddRule(
            "drinking",
            new[] { "sinking a few", "sinking" },
            probability: 0.50
        );

        _st.AddRule(new[] { "cig", "cigg", "cigarette" }, "ciggy");

        // phonetic stuff
        _st.AddRule(@"ight\b", "oit", position: MatchPosition.Nothing);
        _st.AddRule(@"ou", "aow", probability: 0.25, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])tt(?=[aeiou])", "dd", position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])t(?=[aeiou])", "d", probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"er\b", "ah", position: MatchPosition.Nothing);
        _st.AddRule(@"or\b", "ah", position: MatchPosition.Nothing);
        _st.AddRule(@"ar\b", "ah", position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])r\b", "", position: MatchPosition.Nothing);
        _st.AddRule(@"ing\b", "in'", position: MatchPosition.Nothing);

        SubscribeLocalEvent<AustralianAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, AustralianAccentComponent component, AccentGetEvent args)
    {
        args.Message = _st.Process(args.Message);
    }
}
