using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Tiles.Other;

namespace Origins.Items.Weapons.Summoner {
	/// <summary>
	/// TODO: implement
	/// </summary>
	public class SMART_Wrench : ModItem, ICustomWikiStat {
        public override void SetDefaults() {
			Item.damage = 11;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 18;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 15)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rotor>(), 5)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
}
