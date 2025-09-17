using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Layers;
using Origins.Tiles.Defiled;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Lost {
	[AutoloadEquip(EquipType.Head)]
	public class Lost_Helm : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] Categories => [
			"ArmorSet",
			"ManaShielding",
			"MagicBoostGear"
		];
		public override void SetStaticDefaults() {
			Accessory_Tangela_Layer.AddTangelaMask<Head_Tangela_Layer>(Item.headSlot, Texture + "_Head_Tangela");
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.manaRegenBonus += 12;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Lost_Breastplate>() && legs.type == ModContent.ItemType<Lost_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Lost");
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.lostSet = true;
			originPlayer.manaShielding += 0.15f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Lost_Ore_Item>(), 8)
			.AddIngredient(ModContent.ItemType<Strange_String>(), 16)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Lost_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Lost_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Lost_Pants>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Lost_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				if (Mod.RequestAssetIfExists("Items/Armor/Lost/Lost_Breastplate_Cloth_Legs", out Asset<Texture2D> asset)) {
					Origins.TorsoLegLayers.Add(Item.bodySlot, asset);
				}
			}
			Accessory_Tangela_Layer.AddTangelaMask<Body_Tangela_Layer>(Item.bodySlot, Texture + "_Body_Tangela");
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Magic) += 0.15f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Lost_Ore_Item>(), 20)
			.AddIngredient(ModContent.ItemType<Strange_String>(), 28)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Lost_Pants : ModItem, INoSeperateWikiPage {

		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.05f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Lost_Ore_Item>(), 14)
			.AddIngredient(ModContent.ItemType<Strange_String>(), 22)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
namespace Origins.Buffs {
	public class Mana_Buffer_Debuff : ModBuff {

		public override void Update(Player player, ref int buffIndex) {
			player.manaRegenBuff = false;
			player.manaRegen = 0;
			player.manaRegenCount = 0;
			player.manaRegenBonus = 0;
		}
	}
}
