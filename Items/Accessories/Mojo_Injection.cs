using Origins.Dev;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Mojo_Injection : ModItem, ICustomWikiStat {
		public const float healing = 0.0000444f;
		public string[] Categories => [
			"Misc"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.NaturesGift] = Type;
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 26);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(silver: 40);
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.accessory = false;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.glowMask = glowmask;
			Item.consumable = true;
		}
		public override bool? UseItem(Player player) {
			ref bool mojoInjection = ref player.GetModPlayer<OriginPlayer>().mojoInjection;
			if (mojoInjection) return false;
			mojoInjection = true;
			return true;
		}
		public static void UpdateEffect(OriginPlayer originPlayer) {
			originPlayer.CorruptionAssimilation -= Math.Min(healing, originPlayer.CorruptionAssimilation);
			originPlayer.CrimsonAssimilation -= Math.Min(healing, originPlayer.CrimsonAssimilation);
			originPlayer.DefiledAssimilation -= Math.Min(healing, originPlayer.DefiledAssimilation);
			originPlayer.RivenAssimilation -= Math.Min(healing, originPlayer.RivenAssimilation);
		}
	}
}
