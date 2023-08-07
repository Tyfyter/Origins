using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Sapphire {
    [AutoloadEquip(EquipType.Head)]
	public class Sapphire_Hood : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Sapphire Mediator Hood");
			// Tooltip.SetDefault("-40 mana");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.statManaMax2 -= 40;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Sapphire_Vest>() && legs.type == ModContent.ItemType<Sapphire_Tights>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Generate a massive aura that pushes away enemies and projectiles\nharder the closer they are to you. Allies receive a minor boost in stats when in\nthe aura. Fades as you run out of mana";
			//player.GetModPlayer<OriginPlayer>().sapphireSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Sapphire, 4);
			recipe.AddIngredient(ItemID.SoulofMight, 3);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Sapphire_Vest : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Sapphire Mediator Vest");
			// Tooltip.SetDefault("+15% magic speed");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Magic) += 0.15f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Sapphire, 12);
			recipe.AddIngredient(ItemID.SoulofMight, 3);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 36);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Sapphire_Tights : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Sapphire Mediator Tights");
			// Tooltip.SetDefault("Increased movement speed");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.1f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Sapphire, 8);
			recipe.AddIngredient(ItemID.SoulofMight, 3);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 24);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
