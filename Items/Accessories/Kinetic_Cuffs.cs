using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
	public class Kinetic_Cuffs : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMasks(Item, EquipType.HandsOn, EquipType.HandsOff);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
			Item.defense = 2;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().pacemaker = true;
			player.magicCuffs = true;
			player.manaMagnet = true;
			player.statManaMax2 += 40;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.CelestialCuffs)
			.AddIngredient<Pacemaker>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
