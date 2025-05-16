using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Journal;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Aetherite {
	[AutoloadEquip(EquipType.Head)]
	public class Aetherite_Wreath : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override string Texture => typeof(Ashen.Ashen_Helmet).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.projectileSpeedBoost += 0.15f;
			originPlayer.meleeScaleMultiplier *= 1.15f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Aetherite_Robes>() && legs.type == ModContent.ItemType<Aetherite_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Aetherite").SubstituteKeybind(Keybindings.TriggerSetBonus);
			player.OriginPlayer().setActiveAbility = SetActiveAbility.aetherite_armor;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Aetherite_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Aetherite_Robes>();
		public int LegsItemID => ModContent.ItemType<Aetherite_Pants>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Aetherite_Robes : ModItem, INoSeperateWikiPage {
		public override string Texture => typeof(Ashen.Ashen_Breastplate).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.24f;
			player.maxMinions += 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(24)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Aetherite_Pants : ModItem, INoSeperateWikiPage {
		public override string Texture => typeof(Ashen.Ashen_Greaves).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			if (!player.controlDown) player.gravity *= 0.8f;
			player.jumpSpeedBoost += 2f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(18)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
