using Content.Shared.Speech;
using Content.Server._DV.Speech.Components;
using Content.Server._DV.Speech.SpeechConverter;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class LateralLispSystem : EntitySystem
{
    private readonly SpeechConverterSystem _st = new();
    public override void Initialize()
    {
        base.Initialize();
        _st.AddRule("s", "shl", CapitalizationMode.PerLetter, position: MatchPosition.Anywhere);
        _st.AddRule("z", "zhl", CapitalizationMode.PerLetter, position: MatchPosition.Anywhere);
        _st.AddRule("x", "kshl", CapitalizationMode.PerLetter, position: MatchPosition.Anywhere);

        SubscribeLocalEvent<LateralLispComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LateralLispComponent component, AccentGetEvent args)
    {
        args.Message = _st.Process(args.Message);
    }
}
