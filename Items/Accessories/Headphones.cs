using Origins.Dev;
using Origins.Items.Materials;
using Origins.Layers;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Headphones : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"SummonBoostGear"
		];
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Face_Glow_Layer>(Item.faceSlot, Texture + "_Face_Glow");
		}
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().summonTagForceCrit = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 8)
			.AddIngredient(ModContent.ItemType<Fiberglass_Item>(), 6)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rubber>(), 5)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
}
