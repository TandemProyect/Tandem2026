using System;
using Newtonsoft.Json;

namespace AutocadPlugin
{
    public sealed class WallEditAttrs
    {
        [JsonProperty("h")] public double HeightM { get; set; } = 2.70;
        [JsonProperty("bi")] public bool BracketIn { get; set; } = true;
        [JsonProperty("bo")] public bool BracketOut { get; set; } = true;
        [JsonProperty("si")] public bool StiffIn { get; set; } = true;
        [JsonProperty("so")] public bool StiffOut { get; set; } = true;
        [JsonProperty("p75")] public bool Panel750 { get; set; } = true;
        [JsonProperty("vp")] public bool VerticalPanels { get; set; } = true;
        [JsonProperty("pi")] public bool PropIn { get; set; } = true;
        [JsonProperty("po")] public bool PropOut { get; set; } = true;
        [JsonProperty("pii")] public bool PropInInf { get; set; } = true;
        [JsonProperty("poi")] public bool PropOutInf { get; set; } = true;
        [JsonProperty("ti")] public string TapeIni { get; set; } = "";
        [JsonProperty("te")] public string TapeEnd { get; set; } = "";
        [JsonProperty("s1")] public bool SolapeIni { get; set; }
        [JsonProperty("s2")] public bool SolapeEnd { get; set; }

        public static WallEditAttrs Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new WallEditAttrs();
            try
            {
                var parsed = JsonConvert.DeserializeObject<WallEditAttrs>(json);
                return parsed ?? new WallEditAttrs();
            }
            catch
            {
                return new WallEditAttrs();
            }
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public WallEditAttrs Copy()
        {
            return Load(ToJson());
        }
    }
}
