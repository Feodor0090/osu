// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHoldOff : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Hold Off";

        public override string Acronym => "HO";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => @"Convert all sliders to streams.";

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModTarget), typeof(OsuModStrictTracking) };

        [SettingSource("Beat divisor")]
        public BindableInt BeatDivisor { get; } = new BindableInt(4)
        {
            MinValue = 1,
            MaxValue = 16
        };

        [SettingSource("Maximum stream length in beats", "Streams with more notes than this value will be lightened.")]
        public BindableInt MaxStreamLength { get; } = new BindableInt(16)
        {
            MinValue = 5,
            MaxValue = 100,
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var osuBeatmap = (OsuBeatmap)beatmap;

            var newObjects = new List<OsuHitObject>();

            foreach (var hitObject in osuBeatmap.HitObjects)
            {
                if (hitObject is not Slider s)
                {
                    newObjects.Add(hitObject);
                    continue;
                }

                var point = beatmap.ControlPointInfo.TimingPointAt(s.StartTime);
                s.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

                // SpanDuration <= beat/(divisor/2) && has repeats - likely a kickslider, using another method.
                if (!Precision.DefinitelyBigger(s.SpanDuration, point.BeatLength * 2d / BeatDivisor.Value) && s.RepeatCount > 0)
                {
                    newObjects.AddRange(OsuHitObjectGenerationUtils.ConvertKickSliderToBurst(s, point, BeatDivisor.Value));
                }
                else
                {
                    double divisor = BeatDivisor.Value;
                    // dur/BL*div is always lower than actual note count by 1, so using >=, not >.
                    if (s.Duration / point.BeatLength * divisor >= MaxStreamLength.Value)
                        divisor /= 2d; // making stream slower twice, if it's longer than the limit.
                    newObjects.AddRange(OsuHitObjectGenerationUtils.ConvertSliderToStream(s, point, divisor));
                }
            }

            osuBeatmap.HitObjects = newObjects;
        }
    }
}
