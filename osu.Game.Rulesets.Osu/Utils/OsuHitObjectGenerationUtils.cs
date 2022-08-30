// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Utils
{
    public static partial class OsuHitObjectGenerationUtils
    {
        // The relative distance to the edge of the playfield before objects' positions should start to "turn around" and curve towards the middle.
        // The closer the hit objects draw to the border, the sharper the turn
        private const float playfield_edge_ratio = 0.375f;

        private static readonly float border_distance_x = OsuPlayfield.BASE_SIZE.X * playfield_edge_ratio;
        private static readonly float border_distance_y = OsuPlayfield.BASE_SIZE.Y * playfield_edge_ratio;

        private static readonly Vector2 playfield_middle = OsuPlayfield.BASE_SIZE / 2;

        /// <summary>
        /// Rotate a hit object away from the playfield edge, while keeping a constant distance
        /// from the previous object.
        /// </summary>
        /// <remarks>
        /// The extent of rotation depends on the position of the hit object. Hit objects
        /// closer to the playfield edge will be rotated to a larger extent.
        /// </remarks>
        /// <param name="prevObjectPos">Position of the previous hit object.</param>
        /// <param name="posRelativeToPrev">Position of the hit object to be rotated, relative to the previous hit object.</param>
        /// <param name="rotationRatio">
        /// The extent of rotation.
        /// 0 means the hit object is never rotated.
        /// 1 means the hit object will be fully rotated towards playfield center when it is originally at playfield edge.
        /// </param>
        /// <returns>The new position of the hit object, relative to the previous one.</returns>
        public static Vector2 RotateAwayFromEdge(Vector2 prevObjectPos, Vector2 posRelativeToPrev, float rotationRatio = 0.5f)
        {
            float relativeRotationDistance = 0f;

            if (prevObjectPos.X < playfield_middle.X)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_x - prevObjectPos.X) / border_distance_x,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevObjectPos.X - (OsuPlayfield.BASE_SIZE.X - border_distance_x)) / border_distance_x,
                    relativeRotationDistance
                );
            }

            if (prevObjectPos.Y < playfield_middle.Y)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_y - prevObjectPos.Y) / border_distance_y,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevObjectPos.Y - (OsuPlayfield.BASE_SIZE.Y - border_distance_y)) / border_distance_y,
                    relativeRotationDistance
                );
            }

            return RotateVectorTowardsVector(
                posRelativeToPrev,
                playfield_middle - prevObjectPos,
                Math.Min(1, relativeRotationDistance * rotationRatio)
            );
        }

        /// <summary>
        /// Rotates vector "initial" towards vector "destination".
        /// </summary>
        /// <param name="initial">The vector to be rotated.</param>
        /// <param name="destination">The vector that "initial" should be rotated towards.</param>
        /// <param name="rotationRatio">How much "initial" should be rotated. 0 means no rotation. 1 means "initial" is fully rotated to equal "destination".</param>
        /// <returns>The rotated vector.</returns>
        public static Vector2 RotateVectorTowardsVector(Vector2 initial, Vector2 destination, float rotationRatio)
        {
            float initialAngleRad = MathF.Atan2(initial.Y, initial.X);
            float destAngleRad = MathF.Atan2(destination.Y, destination.X);

            float diff = destAngleRad - initialAngleRad;

            while (diff < -MathF.PI) diff += 2 * MathF.PI;

            while (diff > MathF.PI) diff -= 2 * MathF.PI;

            float finalAngleRad = initialAngleRad + rotationRatio * diff;

            return new Vector2(
                initial.Length * MathF.Cos(finalAngleRad),
                initial.Length * MathF.Sin(finalAngleRad)
            );
        }

        /// <summary>
        /// Reflects the position of the <see cref="OsuHitObject"/> in the playfield horizontally.
        /// </summary>
        /// <param name="osuObject">The object to reflect.</param>
        public static void ReflectHorizontally(OsuHitObject osuObject)
        {
            osuObject.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - osuObject.X, osuObject.Position.Y);

            if (!(osuObject is Slider slider))
                return;

            // No need to update the head and tail circles, since slider handles that when the new slider path is set
            slider.NestedHitObjects.OfType<SliderTick>().ForEach(h => h.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - h.Position.X, h.Position.Y));
            slider.NestedHitObjects.OfType<SliderRepeat>().ForEach(h => h.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - h.Position.X, h.Position.Y));

            var controlPoints = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            foreach (var point in controlPoints)
                point.Position = new Vector2(-point.Position.X, point.Position.Y);

            slider.Path = new SliderPath(controlPoints, slider.Path.ExpectedDistance.Value);
        }

        /// <summary>
        /// Reflects the position of the <see cref="OsuHitObject"/> in the playfield vertically.
        /// </summary>
        /// <param name="osuObject">The object to reflect.</param>
        public static void ReflectVertically(OsuHitObject osuObject)
        {
            osuObject.Position = new Vector2(osuObject.Position.X, OsuPlayfield.BASE_SIZE.Y - osuObject.Y);

            if (!(osuObject is Slider slider))
                return;

            // No need to update the head and tail circles, since slider handles that when the new slider path is set
            slider.NestedHitObjects.OfType<SliderTick>().ForEach(h => h.Position = new Vector2(h.Position.X, OsuPlayfield.BASE_SIZE.Y - h.Position.Y));
            slider.NestedHitObjects.OfType<SliderRepeat>().ForEach(h => h.Position = new Vector2(h.Position.X, OsuPlayfield.BASE_SIZE.Y - h.Position.Y));

            var controlPoints = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            foreach (var point in controlPoints)
                point.Position = new Vector2(point.Position.X, -point.Position.Y);

            slider.Path = new SliderPath(controlPoints, slider.Path.ExpectedDistance.Value);
        }

        /// <summary>
        /// Rotate a slider about its start position by the specified angle.
        /// </summary>
        /// <param name="slider">The slider to be rotated.</param>
        /// <param name="rotation">The angle, measured in radians, to rotate the slider by.</param>
        public static void RotateSlider(Slider slider, float rotation)
        {
            void rotateNestedObject(OsuHitObject nested) => nested.Position = rotateVector(nested.Position - slider.Position, rotation) + slider.Position;

            // No need to update the head and tail circles, since slider handles that when the new slider path is set
            slider.NestedHitObjects.OfType<SliderTick>().ForEach(rotateNestedObject);
            slider.NestedHitObjects.OfType<SliderRepeat>().ForEach(rotateNestedObject);

            var controlPoints = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            foreach (var point in controlPoints)
                point.Position = rotateVector(point.Position, rotation);

            slider.Path = new SliderPath(controlPoints, slider.Path.ExpectedDistance.Value);
        }

        /// <summary>
        /// Rotate a vector by the specified angle.
        /// </summary>
        /// <param name="vector">The vector to be rotated.</param>
        /// <param name="rotation">The angle, measured in radians, to rotate the vector by.</param>
        /// <returns>The rotated vector.</returns>
        private static Vector2 rotateVector(Vector2 vector, float rotation)
        {
            float angle = MathF.Atan2(vector.Y, vector.X) + rotation;
            float length = vector.Length;
            return new Vector2(
                length * MathF.Cos(angle),
                length * MathF.Sin(angle)
            );
        }

        /// <summary>
        /// Converts slider to a stream.
        /// </summary>
        /// <param name="slider">Slider to convert.</param>
        /// <param name="timingPoint">Timing point in which the slider is placed.</param>
        /// <param name="spacing">Beat divisor to place objects on.</param>
        /// <returns>List of circles.</returns>
        public static IEnumerable<HitCircle> ConvertSliderToStream(Slider slider, TimingControlPoint timingPoint, int spacing)
        {
            double streamSpacing = timingPoint.BeatLength / spacing;

            int i = 0;
            double time = slider.StartTime;

            while (!Precision.DefinitelyBigger(time, slider.GetEndTime(), 1))
            {
                // positionWithRepeats is a fractional number in the range of [0, HitObject.SpanCount()]
                // and indicates how many fractional spans of a slider have passed up to time.
                double positionWithRepeats = (time - slider.StartTime) / slider.Duration * slider.SpanCount();
                double pathPosition = positionWithRepeats - (int)positionWithRepeats;
                // every second span is in the reverse direction - need to reverse the path position.
                if (Precision.AlmostBigger(positionWithRepeats % 2, 1))
                    pathPosition = 1 - pathPosition;

                Vector2 position = slider.Position + slider.Path.PositionAt(pathPosition);

                var samplePoint = (SampleControlPoint)slider.SampleControlPoint.DeepClone();
                samplePoint.Time = time;

                yield return new HitCircle
                {
                    StartTime = time,
                    Position = position,
                    NewCombo = i == 0 && slider.NewCombo,
                    SampleControlPoint = samplePoint,
                    Samples = slider.HeadCircle.Samples.Select(s => s.With()).ToList()
                };

                i += 1;
                time = slider.StartTime + i * streamSpacing;
            }
        }
    }
}
