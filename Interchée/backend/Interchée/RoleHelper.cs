using System.Globalization;

using System.Text;

namespace Interchée.Utils

{

    /// <summary>

    /// Canonicalizes incoming role text (case/diacritics) to your constants in Interchée.Config.Roles.

    /// Returns null if not recognized.

    /// </summary>

    public static class RoleHelper

    {

        // normalized-input -> canonical role

        private static readonly Dictionary<string, string> Map = BuildMap();

        public static string? ToCanonical(string? input)

        {

            if (string.IsNullOrWhiteSpace(input)) return null;

            var key = Normalize(input);

            return Map.TryGetValue(key, out var canonical) ? canonical : null;

        }

        private static Dictionary<string, string> BuildMap()

        {

            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            // Pull canonical names from your static config

            var canon = new[]

            {

                Interchée.Config.Roles.Admin,

                Interchée.Config.Roles.HR,

                Interchée.Config.Roles.Supervisor,

                Interchée.Config.Roles.Attache,

                Interchée.Config.Roles.Intern,

                // If you add this constant to Interchée.Config.Roles, also include:

                // Interchée.Config.Roles.Instructor,

            };

            // Map canonical names

            foreach (var r in canon)

                map[Normalize(r)] = r;

            // Aliases → canonical (normalized keys)

            map[Normalize("Attaché")] = Interchée.Config.Roles.Attache;

            map[Normalize("attache")] = Interchée.Config.Roles.Attache;

            return map;

        }

        private static string Normalize(string s)

        {

            var low = s.Trim().ToLowerInvariant();

            var norm = low.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder(low.Length);

            foreach (var ch in norm)

            {

                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);

                if (cat != UnicodeCategory.NonSpacingMark) sb.Append(ch);

            }

            return sb.ToString().Normalize(NormalizationForm.FormC);

        }

    }

}

