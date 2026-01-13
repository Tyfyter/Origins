using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Mildew {
	[AutoloadEquip(EquipType.Head)]
	public class Mildew_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] Categories => [
			"ArmorSet"
		];
		int backHeadID;
		public override void Load() {
			backHeadID = EquipLoader.AddEquipTexture(Mod, Texture + "_Head_Back", EquipType.Head, name: Name + "_Head_Back");
		}
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
			ArmorIDs.Head.Sets.FrontToBackID[Item.headSlot] = backHeadID;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.mildewHead = true;
			player.GetDamage(DamageClass.Summon) += 0.1f;
			originPlayer.artifactDamage += 0.1f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Mildew_Breastplate>() && legs.type == ModContent.ItemType<Mildew_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Mildew");
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.OriginPlayer().mildewSet = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Mildew_Item>(13)
			.AddIngredient<Eitrite_Bar>(10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public string ArmorSetName => "Mildew_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Mildew_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Mildew_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Mildew_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 12;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.projectileSpeedBoost += 0.1f;
			player.GetDamage(DamageClass.Summon) += 0.1f;
			originPlayer.artifactDamage += 0.1f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Mildew_Item>(26)
			.AddIngredient<Eitrite_Bar>(20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Mildew_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 8;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions += 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Mildew_Item>(23)
			.AddIngredient<Eitrite_Bar>(16)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
