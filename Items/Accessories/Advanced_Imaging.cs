using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Advanced_Imaging : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"GenericBoostAcc"
		];
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Face_Glow_Layer>(Item.faceSlot, Texture + "_Face_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().advancedImaging = true;
			player.buffImmune[BuffID.Confused] = true;

			player.GetCritChance(DamageClass.Generic) += 10;
			player.OriginPlayer().explosiveBlastRadius += 0.2f;
		}
	}
}
