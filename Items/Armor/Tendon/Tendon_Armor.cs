using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Tendon {
	[AutoloadEquip(EquipType.Head)]
	public class Tendon_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tendon Helmet");
			Tooltip.SetDefault("+10% ranged damage");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.buyPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Ranged) += 0.1f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Tendon_Shirt>() && legs.type == ModContent.ItemType<Tendon_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Increased mobility based on available lifeforce";
			player.GetModPlayer<OriginPlayer>().tendonSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.CrimtaneOre, 8);
			recipe.AddIngredient(ItemID.Vertebrae, 16);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Tendon_Shirt : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tendon Shirt");
			Tooltip.SetDefault("+6% critical strike chance");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.buyPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 0.06f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.CrimtaneOre, 20);
			recipe.AddIngredient(ItemID.Vertebrae, 28);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Tendon_Pants : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tendon Pants");
			Tooltip.SetDefault("+20% ammo preservation");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.buyPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.ammoBox = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.CrimtaneOre, 14);
			recipe.AddIngredient(ItemID.Vertebrae, 22);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
