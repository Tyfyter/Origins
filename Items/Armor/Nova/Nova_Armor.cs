using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Nova {
    [AutoloadEquip(EquipType.Head)]
	public class Nova_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => new string[] {
            "PostMLArmorSet",
            "ExplosiveBoostGear"
        };
        public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Nova/Nova_Helmet_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Red;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.04f;
			player.GetCritChance(DamageClasses.Explosive) += 0.04f;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.5f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Nova_Breastplate>() && legs.type == ModContent.ItemType<Nova_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Nova");
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveBlastRadius.Base += 32;
			originPlayer.explosiveBlastRadius *= 2f;
			originPlayer.novaSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.LunarBar, 8);
			recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 10);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
		public string ArmorSetName => "Nova_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Nova_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Nova_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Nova_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Nova/Nova_Breastplate_Body_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 22;
			Item.value = Item.sellPrice(gold: 14);
			Item.rare = ItemRarityID.Red;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.04f;
			player.GetCritChance(DamageClasses.Explosive) += 0.04f;
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.45f;
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
	public class Nova_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Nova/Nova_Greaves_Legs_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.sellPrice(gold: 10, silver: 50);
			Item.rare = ItemRarityID.Red;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.04f;
			player.GetCritChance(DamageClasses.Explosive) += 0.04f;
			player.moveSpeed += 0.15f;
			player.accRunSpeed += 0.15f;
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
