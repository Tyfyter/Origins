using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Lost {
	[AutoloadEquip(EquipType.Head)]
	public class Lost_Helm : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lost Helm");
			Tooltip.SetDefault("5% increased critical strike chance");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.buyPrice(silver: 75);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 0.05f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Lost_Breastplate>() && legs.type == ModContent.ItemType<Lost_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "The devs are trying their best";
			//player.GetModPlayer<OriginPlayer>().lostSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 16);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Lost_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lost Breastplate");
			Tooltip.SetDefault("3% increased damage");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.buyPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.03f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 20);
			recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 28);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Lost_Pants : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lost Pants");
			Tooltip.SetDefault("6% increased weapon speed");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.buyPrice(silver: 45);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.06f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 14);
			recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 22);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
