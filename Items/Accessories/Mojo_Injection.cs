using Origins.Dev;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Mojo_Injection : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Misc"
		};
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(16, 26);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 2);
            Item.glowMask = glowmask;
        }
		public override void UpdateAccessory(Player player, bool hideVisual) {
			const float healing = 0.0000444f;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.CorruptionAssimilation -= Math.Min(healing, originPlayer.CorruptionAssimilation);
			originPlayer.CrimsonAssimilation -= Math.Min(healing, originPlayer.CrimsonAssimilation);
			originPlayer.DefiledAssimilation -= Math.Min(healing, originPlayer.DefiledAssimilation);
			originPlayer.RivenAssimilation -= Math.Min(healing, originPlayer.RivenAssimilation);
		}
	}
}
