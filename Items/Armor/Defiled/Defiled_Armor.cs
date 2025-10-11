using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Defiled {
	[AutoloadEquip(EquipType.Head)]
	public class Defiled_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] Categories => [
			"ArmorSet",
			"GenericBoostGear"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled2_Helmet>()] = ModContent.ItemType<Defiled_Helmet>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled_Helmet>()] = ModContent.ItemType<Defiled2_Helmet>();
			Origins.AddHelmetGlowmask(this);
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 5;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return (body.type == ModContent.ItemType<Defiled_Breastplate>() || body.type == ModContent.ItemType<Defiled2_Breastplate>()) && (legs.type == ModContent.ItemType<Defiled_Greaves>() || legs.type == ModContent.ItemType<Defiled2_Greaves>());
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Defiled");
			player.statLifeMax2 += (int)(player.statLifeMax2 * 0.25);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Defiled_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Defiled_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Defiled_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Defiled_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled2_Breastplate>()] = ModContent.ItemType<Defiled_Breastplate>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled_Breastplate>()] = ModContent.ItemType<Defiled2_Breastplate>();
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.03f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Defiled_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled2_Greaves>()] = ModContent.ItemType<Defiled_Greaves>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled_Greaves>()] = ModContent.ItemType<Defiled2_Greaves>();
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.03f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 15)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Defiled2_Helmet : Defiled_Helmet, IWikiArmorSet, INoSeperateWikiPage {
		public override void AddRecipes() { }
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }
		public new string ArmorSetName => "Ancient_Defiled_Armor";
		public new int HeadItemID => Type;
		public new int BodyItemID => ModContent.ItemType<Defiled2_Breastplate>();
		public new int LegsItemID => ModContent.ItemType<Defiled2_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Defiled2_Breastplate : Defiled_Breastplate {
		public override void SetStaticDefaults() { }
		public override void AddRecipes() { }
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Defiled2_Greaves : Defiled_Greaves {
		public override void SetStaticDefaults() { }
		public override void AddRecipes() { }
	}
}
