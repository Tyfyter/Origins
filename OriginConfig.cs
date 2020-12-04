using Newtonsoft.Json;
using Origins.Items.Weapons.Explosives;
using Origins.NPCs.Defiled;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Origins {
    [Label("Settings")]
    public class OriginConfig : ModConfig {
        public static OriginConfig Instance;
        public override ConfigScope Mode => ConfigScope.ServerSide;
        [Label("Use alternate world evil biomes")]
        [OptionStrings(new string[] { "never", "50/50", "always" })]
        [DefaultValue("50/50")]
        public string altWorldEvil;
        [JsonIgnore]
        public sbyte worldTypeSkew = 0;

        [Header("Vanilla Buffs")]

        [Label("Infected Wood Items")]
        [DefaultValue(true)]
        public bool WoodBuffs = true;

        [Label("Deb_A-S_field1")]
        [DefaultValue(3)]
        public int Ace_Shrap_MH{
            get => Ace_Shrapnel_Shard.maxHits;
            set {
                Ace_Shrapnel_Shard.maxHits = value;
            }
        }

        [Label("Deb_A-S_field2")]
        [DefaultValue(5)]
        public int Ace_Shrap_CD{
            get => Ace_Shrapnel_Shard.hitCD;
            set {
                Ace_Shrapnel_Shard.hitCD = value;
            }
        }

        /*[Label("Deb_D-B_field1")]
        [DefaultValue(0.75f)]
        public float Brute_Speed{
            get => Defiled_Brute.speedMult;
            set {
                Defiled_Brute.speedMult = value;
            }
        }*/

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            switch(altWorldEvil) {
                case "never":
                worldTypeSkew = -1;
                break;
                case "always":
                worldTypeSkew = 1;
                break;
                default:
                worldTypeSkew = 0;
                break;
            }
        }
    }
}
