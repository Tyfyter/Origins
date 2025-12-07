using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Ashen {
	[AutoloadEquip(EquipType.Head)]
	public class Ashen_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage, IJournalEntrySource {
		public string[] Categories => [
			"ArmorSet",
			"ExplosiveBoostGear",
			"GenericBoostGear",
			"SelfDamageProtek"
		];
		public string EntryName => "Origins/" + typeof(Ashen_Helmet_Entry).Name;
		public class Ashen_Helmet_Entry : JournalEntry {
			public override string TextKey => "Ashen_Helmet";
			public override JournalSortIndex SortIndex => new("Ashen_Armor", 1);
		}
		public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.08f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Ashen_Breastplate>() && legs.type == ModContent.ItemType<Ashen_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Ashen");
			player.GetModPlayer<OriginPlayer>().ashenKBReduction = true;
			player.GetKnockback(DamageClass.Generic) += 0.15f;
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.4f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<NE8>(), 10)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 15)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Ashen_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Ashen_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Ashen_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Ashen_Breastplate : ModItem, INoSeperateWikiPage, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Ashen_Breastplate_Entry).Name;
		public class Ashen_Breastplate_Entry : JournalEntry {
			public override string TextKey => "Ashen_Breastplate";
			public override JournalSortIndex SortIndex => new("Ashen_Armor", 2);
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.08f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<NE8>(), 20)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 25)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Ashen_Greaves : ModItem, INoSeperateWikiPage, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Ashen_Greaves_Entry).Name;
		public class Ashen_Greaves_Entry : JournalEntry {
			public override string TextKey => "Ashen_Greaves";
			public override JournalSortIndex SortIndex => new("Ashen_Armor", 3);
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 8;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<NE8>(), 15)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
