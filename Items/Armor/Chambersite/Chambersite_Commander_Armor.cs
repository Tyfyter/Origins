using Origins.Buffs;
using Origins.Dev;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Chambersite {
	[AutoloadEquip(EquipType.Head)]
	public class Chambersite_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override string Texture => "Origins/Items/Armor/Ashen/Ashen_Helmet";
		public string[] Categories => [
			"ArmorSet",
			"GenericBoostGear"
		];
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.10f;
			player.OriginPlayer().projectileSpeedBoost += 0.1f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Chambersite_Breastplate>() && legs.type == ModContent.ItemType<Chambersite_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.ChambersiteCommander");
			player.OriginPlayer().chambersiteCommandoSet = true;
			player.GetKnockback(DamageClass.Generic) += 0.15f;
			player.AddBuff(ModContent.BuffType<Voidsight_Buff>(), 60);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.LunarTabletFragment, 3)
			.AddIngredient<Chambersite_Item>(4)
			.AddIngredient<Carburite_Item>(12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public string ArmorSetName => "Chambersite_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Chambersite_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Chambersite_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Chambersite_Breastplate : ModItem, INoSeperateWikiPage {
		public override string Texture => "Origins/Items/Armor/Ashen/Ashen_Breastplate";
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 0.10f;
			player.endurance += (1 - player.endurance) * 0.08f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.LunarTabletFragment, 3)
			.AddIngredient<Chambersite_Item>(12)
			.AddIngredient<Carburite_Item>(36)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Chambersite_Greaves : ModItem, INoSeperateWikiPage {
		public override string Texture => "Origins/Items/Armor/Ashen/Ashen_Greaves";
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.1f;
			player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.LunarTabletFragment, 3)
			.AddIngredient<Chambersite_Item>(8)
			.AddIngredient<Carburite_Item>(24)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
