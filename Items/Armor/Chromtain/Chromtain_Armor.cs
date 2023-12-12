using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Chromtain {
    [AutoloadEquip(EquipType.Head)]
	public class Chromtain_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Chromtain/Chromtain_Helmet_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 16;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 15);
			recipe.AddTile(TileID.DefendersForge); //Interstellar Sampler
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
		public string ArmorSetName => "Chromtain_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Chromtain_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Chromtain_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Chromtain_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Chromtain/Chromtain_Breastplate_Body_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 40;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.statLifeMax2 += 20;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 25);
			recipe.AddTile(TileID.DefendersForge); //Interstellar Sampler
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Chromtain_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Chromtain/Chromtain_Greaves_Legs_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 25;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Chromtain_Bar>(), 20);
			recipe.AddTile(TileID.DefendersForge); //Interstellar Sampler
			recipe.Register();
		}
	}
}
