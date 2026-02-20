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
            [
                "friend", "pal", "buddy", "dude", "man", "guy",
                "bro", "brother", "sir", "mister", "boss"
            ],
            ["mate", "cobba"]
        );

        _st.AddRule(
            ["girl", "lady", "woman", "ma'am", "maam", "miss"],
            ["sheila", "missus"]
        );

        _st.AddRule(["doctor", "medical", "physician"], "doc", probability: 0.7);
        _st.AddRule(["paramedic", "medical doctor", "medic"], "ambo", probability: 0.5);
        _st.AddRule(["scientist", "researcher"], "boffin", probability: 0.7);
        _st.AddRule(["cargo", "quartermaster"], "truckie", probability: 0.7);
        _st.AddRule(["botanist", "gardener"], "greenie", probability: 0.7);
        _st.AddRule("clown", "funny bugger", probability: 0.7);
        _st.AddRule(["syndi", "syndie", "syndicate", "nukie", "nuke op", "nuclear operative", "traitor"], "shonky bastard", probability: 0.7);

        // gen vocab / expressions
        _st.AddRule(["hello", "hello there", "hi"], "g'day", applyOnce: true);
        _st.AddRule(["thanks", "thank you"], "cheers");
        _st.AddRule(["very", "really"], "bloody", probability: 0.65);
        _st.AddRule("totally", "dead set", probability: 0.50);
        _st.AddRule("think", "reckon", probability: 0.70);
        _st.AddRule("yes", "nah yea", probability: 0.35, applyOnce: true);
        _st.AddRule("no", "yea nah", probability: 0.35, applyOnce: true);
        _st.AddRule(["yes", "yeah"], "yea");
        _st.AddRule("no", "nah");
        _st.AddRule("you", "ya");
        _st.AddRule("your", "yer");
        _st.AddRule("my", "me", probability: 0.60);
        _st.AddRule(["going to", "going"], "gonna");
        _st.AddRule(["don't know", "dont know"], "dunno");
        _st.AddRule("fuck", "fark");
        _st.AddRule("fucking", "farking");
        _st.AddRule("hell", "'ell");
        _st.AddRule("shit", "shite");
        _st.AddRule("idiot", ["drongo", "galah", "muppet"]);
        _st.AddRule(["no problem", "you're welcome", "its okay"], "no worries");
        _st.AddRule("wine", "goon"); //blame JohnHelldiver for this
        _st.AddRule("drunk", "maggot");

        // slangs
        _st.AddRule("afternoon", "arvo");
        _st.AddRule("biscuits", "bikkies");
        _st.AddRule("chocolate", "chocky");
        _st.AddRule("mosquito", "mozzie");
        _st.AddRule("present", "prezzy");
        _st.AddRule("kangaroo", ["roo", "joey"]);
        _st.AddRule(["candy", "sweets"], "lollies");
        _st.AddRule(["cig", "cigg", "cigarette"], ["ciggy", "dart"]);

        // phonetic stuff
        _st.AddRule(@"ight\b", "oight", position: MatchPosition.Nothing);
        _st.AddRule(@"ou", "aow", probability: 0.25, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])tt(?=[aeiou])", "dd", position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])t(?=[aeiou])", "d", probability: 0.4, position: MatchPosition.Nothing);
        _st.AddRule(@"er\b", "ah", position: MatchPosition.Nothing);
        _st.AddRule(@"or\b", "ah", position: MatchPosition.Nothing);
        _st.AddRule(@"ar\b", "ah", position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])r\b", "", position: MatchPosition.Nothing);
        _st.AddRule(@"ing\b", "in'", position: MatchPosition.Nothing);
        _st.AddRule(@"ay", "ai", probability: 0.30, position: MatchPosition.Nothing);
        _st.AddRule(@"rry\b", "zza", probability: 0.50, position: MatchPosition.Nothing);
        _st.AddRule(@"\bto\b", "ta", position: MatchPosition.Nothing);
        _st.AddRule(@"\band\b", "n'", position: MatchPosition.Nothing);

        SubscribeLocalEvent<AustralianAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, AustralianAccentComponent component, AccentGetEvent args)
    {
        args.Message = _st.Process(args.Message);
    }
}
