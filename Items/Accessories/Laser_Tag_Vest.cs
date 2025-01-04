using Origins.Dev;
using Origins.Items.Materials;
using Origins.Layers;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class Laser_Tag_Vest : ModItem, ICustomWikiStat {
		public string[] Categories => [
		];
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Waist_Glow_Layer>(Item.waistSlot, Texture + "_Waist_Glow", player => {
				if (!player.OriginPlayer().laserTagVestActive) return Color.Transparent;
				return Main.teamColor[player.team];
			});
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightPurple;
			Item.glowMask = glowmask;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.laserTagVest = true;
			if (Laser_Tag_Console.LaserTagGameActive) {
				player.noBuilding = !Laser_Tag_Console.LaserTagRules.Building;
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rubber>(), 6)
			//.AddIngredient(ModContent.ItemType<Space_Goo_Item>(), 10)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
}
