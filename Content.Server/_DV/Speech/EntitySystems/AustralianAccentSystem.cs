// sources: australian members of deltav discord

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

        // word replacements
        _st.AddRule(new[] { "engineer", "engi" }, "sparkie", CapitalizationMode.PerLetter);
        _st.AddRule(new[] { "security", "sec" }, new[] { "coppa", "copper", "walloper" }, CapitalizationMode.PerLetter);
        _st.AddRule("courier", "postie", CapitalizationMode.PerLetter);
        _st.AddRule(new[] { "medical", "doctor", "paramedic" }, "ambo", CapitalizationMode.PerLetter);
        _st.AddRule("janitor", "garbo", CapitalizationMode.PerLetter);
        _st.AddRule("captain", "skip", CapitalizationMode.PerLetter);
        _st.AddRule("hi", "ayy mate", CapitalizationMode.PerLetter);
        _st.AddRule("hey", "oi", CapitalizationMode.PerLetter);
        _st.AddRule("girl", new[] { "sheila", "missus" }, CapitalizationMode.PerLetter);
        _st.AddRule("woman", "lady", CapitalizationMode.FirstLetter);
        _st.AddRule("walking", "waltzing", CapitalizationMode.PerLetter);
        _st.AddRule("talking", "havin' a yarn", CapitalizationMode.PerLetter);
        _st.AddRule("beer", new[] { "schooner", "cold one", "tinny" }, CapitalizationMode.PerLetter);
        _st.AddRule("alcohol", "booze", CapitalizationMode.PerLetter);
        _st.AddRule("biscuit", "bickie", CapitalizationMode.PerLetter);
        _st.AddRule("shrimp", "prawn", CapitalizationMode.PerLetter);
        _st.AddRule("kangaroo", new[] { "roo", "joey" }, CapitalizationMode.PerLetter);
        _st.AddRule("thanks", "cheers", CapitalizationMode.PerLetter);
        _st.AddRule("thank you", "cheers mate", CapitalizationMode.PerLetter);
        _st.AddRule(new[] { "cig", "cigg", "cigarette" }, "ciggy", CapitalizationMode.PerLetter);
        _st.AddRule("fries", "chips", CapitalizationMode.PerLetter);
        _st.AddRule("afternoon", "arvo", CapitalizationMode.PerLetter);
        _st.AddRule("breakfast", "brekkie", CapitalizationMode.PerLetter);
        _st.AddRule("service", "servo", CapitalizationMode.PerLetter);
        _st.AddRule("devastated", "gutted", CapitalizationMode.PerLetter);
        _st.AddRule("stolen", "nicked", CapitalizationMode.PerLetter);
        _st.AddRule("drinking", new[] { "having a few cold ones", "sinking a few", "sinking" }, CapitalizationMode.PerLetter, probability: 0.50);
        _st.AddRule("shit", "bugger", CapitalizationMode.PerLetter, probability: 0.60);
        _st.AddRule(@"^oh shit$", "crikey", CapitalizationMode.PerLetter, probability: 0.40);
        _st.AddRule(@"^awesome$", new[] { "ripper", "legit", "cracker" }, CapitalizationMode.PerLetter, probability: 0.65);
        _st.AddRule("yes", "nah yea", CapitalizationMode.PerLetter, probability: 0.40, applyOnce: true);
        _st.AddRule("no", "yea nah", CapitalizationMode.PerLetter, probability: 0.40, applyOnce: true);
        _st.AddRule(new[] { "yes", "yeah" }, "yea", CapitalizationMode.PerLetter);
        _st.AddRule("no", "nah", CapitalizationMode.PerLetter);
        _st.AddRule(new[] { "going to", "going" }, "gonna", CapitalizationMode.PerLetter);
        _st.AddRule(new[] { "candy", "sweets" }, "lollies", CapitalizationMode.PerLetter);
        _st.AddRule(new[] { "friend", "dude", "bro", "sir", "man", "guy" }, "mate", CapitalizationMode.PerLetter);
        _st.AddRule("fuck", "fark", CapitalizationMode.PerLetter, probability: 0.45);
        _st.AddRule("hell", "'ell", CapitalizationMode.PerLetter);
        _st.AddRule("very", "bloody", CapitalizationMode.PerLetter, probability: 0.50);

        // syllable/ending modiciations
        _st.AddRule(@"ing\b", "in'", CapitalizationMode.PerLetter, position: MatchPosition.Nothing);
        _st.AddRule(@"ight\b", "oit", CapitalizationMode.PerLetter, position: MatchPosition.Nothing);

        // phonetic modifications
        _st.AddRule("h", "'", CapitalizationMode.PerLetter, probability: 0.40, position: MatchPosition.Prefix);
        _st.AddRule(@"ater\b", "adah", CapitalizationMode.PerLetter, position: MatchPosition.Nothing);
        _st.AddRule(@"etter\b", "eddah", CapitalizationMode.PerLetter, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])tt(?=[aeiou])", "dd", CapitalizationMode.PerLetter, position: MatchPosition.Nothing);
        _st.AddRule(@"(?<=[aeiou])r\b", "", CapitalizationMode.PerLetter, position: MatchPosition.Nothing);
        _st.AddRule(@"er\b", "ah", CapitalizationMode.PerLetter, position: MatchPosition.Nothing);

        SubscribeLocalEvent<AustralianAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, AustralianAccentComponent component, AccentGetEvent args)
    {
        args.Message = _st.Process(args.Message);
    }
}
