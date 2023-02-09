using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Nova {
    [AutoloadEquip(EquipType.Head)]
	public class Nova_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nova Faceshield");
			Tooltip.SetDefault("Greatly increased explosive velocity");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Nova/Nova_Helmet_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.5f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Nova_Breastplate>() && legs.type == ModContent.ItemType<Nova_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Greatly incrreased explosive blast radius\nExplosive projectiles have a chance to be duplicated\nExplosives home to nearby enemies";
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveBlastRadius.Base += 32;
			originPlayer.explosiveBlastRadius *= 1.5f;
			originPlayer.novaSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.LunarBar, 8);
			recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 10);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Nova_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nova Vest");
			Tooltip.SetDefault("-60% explosive self-damage");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Nova/Nova_Breastplate_Body_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 22;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.60f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.LunarBar, 16);
			recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 20);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Nova_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nova Leggings");
			Tooltip.SetDefault("15% increased movement speed");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Nova/Nova_Greaves_Legs_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.15f;
			player.accRunSpeed += 0.15f;
			player.runAcceleration += 0.02f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.LunarBar, 12);
			recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 15);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
}
