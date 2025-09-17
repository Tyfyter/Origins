using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Nova {
    [AutoloadEquip(EquipType.Head)]
	public class Nova_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "PostMLArmorSet",
            "ExplosiveBoostGear",
			"SelfDamageProtek"
		];
		public bool? Hardmode => true;
        public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
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
			originPlayer.explosiveBlastRadius += 0.6f;
			originPlayer.novaSet = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.LunarBar, 8)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 10)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
		public string ArmorSetName => "Nova_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Nova_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Nova_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Nova_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Origins.AddBreastplateGlowmask(this);
		}
		public override void SetDefaults() {
			Item.defense = 22;
			Item.value = Item.sellPrice(gold: 14);
			Item.rare = ItemRarityID.Red;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.04f;
			player.GetCritChance(DamageClasses.Explosive) += 0.04f;
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.6f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.LunarBar, 16)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 20)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Nova_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			Origins.AddLeggingGlowMask(this);
		}
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.sellPrice(gold: 10, silver: 50);
			Item.rare = ItemRarityID.Red;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClasses.Explosive) += 0.04f;
			player.GetCritChance(DamageClasses.Explosive) += 0.04f;
			player.moveSpeed += 0.25f;
			player.accRunSpeed += 0.25f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.LunarBar, 12)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 15)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
