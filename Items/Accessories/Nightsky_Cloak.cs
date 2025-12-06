using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back, EquipType.Front)]
	public class Nightsky_Cloak : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.MagicBoostAcc
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMasks(Item, EquipType.Back, EquipType.Front);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 3, silver: 30);
		}
		public override void AddRecipes() => CreateRecipe(Type)
			.AddIngredient(ItemID.ManaCloak)
			.AddIngredient<Celestial_Starlight>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.dimStarlight = true;
			player.manaMagnet = true;
			Lighting.AddLight(player.Center, 0.3f, 0.3f, 1f);

			player.manaFlower = true;
			player.manaCost -= 0.08f;
			player.starCloakItem = Item;
			player.starCloakItem_manaCloakOverrideItem = Item;
		}
	}
}
