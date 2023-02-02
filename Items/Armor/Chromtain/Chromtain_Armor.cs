using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Origins.Items.Materials;

namespace Origins.Items.Armor.Chromtain {
	[AutoloadEquip(EquipType.Head)]
	public class Chromtain_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Chromtain Helmet");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Chromtain/Chromtain_Helmet_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 21;
			Item.value = Item.buyPrice(platinum: 1);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 15);
			recipe.AddTile(TileID.DefendersForge); //Omni-Printer
			recipe.Register();
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Chromtain_Breastplate>() && legs.type == ModContent.ItemType<Chromtain_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Enemies are more likely to target you. Your armor is unbreakable.";
			player.aggro += 400;
			player.buffImmune[BuffID.BrokenArmor] = true;
			player.buffImmune[BuffID.WitheredArmor] = true;
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Chromtain_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Chromtain Breastplate");
			Tooltip.SetDefault("+20 max life");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Chromtain/Chromtain_Breastplate_Body_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 54;
			Item.value = Item.buyPrice(platinum: 1);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.statLifeMax2 += 20;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 25);
			recipe.AddTile(TileID.DefendersForge); //Omni-Printer
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Chromtain_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Chromtain Greaves");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Chromtain/Chromtain_Greaves_Legs_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 33;
			Item.value = Item.buyPrice(platinum: 1);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 20);
			recipe.AddTile(TileID.DefendersForge); //Omni-Printer
			recipe.Register();
		}
	}
}
