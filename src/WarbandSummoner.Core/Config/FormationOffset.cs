using System;
using System.Globalization;

namespace WarbandSummoner.Core.Config
{
    /// <summary>
    /// A slot's place in the formation (DESIGN §5.2) as a local-space
    /// offset from the player: <see cref="Right"/> metres to the player's
    /// right and <see cref="Forward"/> metres ahead (negative = behind).
    /// Config spells it "right, forward". Height is not configurable; the
    /// plugin drops the point onto the ground.
    /// </summary>
    public readonly struct FormationOffset : IEquatable<FormationOffset>
    {
        public float Right { get; }
        public float Forward { get; }

        public FormationOffset(float right, float forward)
        {
            Right = right;
            Forward = forward;
        }

        /// <summary>
        /// The shipped melee arc: pairs fan out left/right and each pair
        /// sits a row further back, so any slot count gets a sane place.
        /// Slots 0–3 land at (-1.5,-2) (1.5,-2) (-3,-3.5) (3,-3.5).
        /// </summary>
        public static FormationOffset DefaultMelee(int slotIndex)
        {
            if (slotIndex < 0) throw new ArgumentOutOfRangeException(nameof(slotIndex));
            int row = slotIndex / 2;
            float side = slotIndex % 2 == 0 ? -1f : 1f;
            return new FormationOffset(side * (1.5f + 1.5f * row), -(2f + 1.5f * row));
        }

        /// <summary>
        /// The shipped ranged line, furthest back: slot 0 dead centre, any
        /// extra ranged slots alternating out to the sides on the same row.
        /// </summary>
        public static FormationOffset DefaultRanged(int slotIndex)
        {
            if (slotIndex < 0) throw new ArgumentOutOfRangeException(nameof(slotIndex));
            int pair = (slotIndex + 1) / 2;
            float side = slotIndex % 2 == 1 ? -1f : 1f;
            return new FormationOffset(slotIndex == 0 ? 0f : side * 2f * pair, -5.5f);
        }

        /// <summary>"right, forward" with invariant-culture numbers, e.g. "-1.5, -2".</summary>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Right, Forward);

        /// <summary>Parses the <see cref="ToString"/> form. Whitespace around either number is fine; anything else is not.</summary>
        public static bool TryParse(string? text, out FormationOffset offset)
        {
            offset = default;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var parts = text!.Split(',');
            if (parts.Length != 2) return false;

            const NumberStyles style = NumberStyles.Float;
            if (!float.TryParse(parts[0].Trim(), style, CultureInfo.InvariantCulture, out float right)) return false;
            if (!float.TryParse(parts[1].Trim(), style, CultureInfo.InvariantCulture, out float forward)) return false;
            if (float.IsNaN(right) || float.IsInfinity(right) || float.IsNaN(forward) || float.IsInfinity(forward)) return false;

            offset = new FormationOffset(right, forward);
            return true;
        }

        public bool Equals(FormationOffset other) => Right.Equals(other.Right) && Forward.Equals(other.Forward);

        public override bool Equals(object? obj) => obj is FormationOffset other && Equals(other);

        public override int GetHashCode() => unchecked(Right.GetHashCode() * 397 ^ Forward.GetHashCode());

        public static bool operator ==(FormationOffset a, FormationOffset b) => a.Equals(b);

        public static bool operator !=(FormationOffset a, FormationOffset b) => !a.Equals(b);
    }
}
