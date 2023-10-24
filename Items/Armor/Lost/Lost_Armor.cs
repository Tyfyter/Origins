using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Lost {
    [AutoloadEquip(EquipType.Head)]
	public class Lost_Helm : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Lost Helm");
			// Tooltip.SetDefault("Increased mana regeneration rate");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.manaRegen += 2;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Lost_Breastplate>() && legs.type == ModContent.ItemType<Lost_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "15% of damage taken is redirected to mana";
			player.GetModPlayer<OriginPlayer>().lostSet = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 16);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public string ArmorSetName => "Lost_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Lost_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Lost_Pants>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Lost_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Lost Breastplate");
			// Tooltip.SetDefault("15% increased magic damage");
			if (Main.netMode != NetmodeID.Server) {
				if (Mod.RequestAssetIfExists("Items/Armor/Lost/Lost_Breastplate_Cloth_Legs", out Asset<Texture2D> asset)) {
					Origins.TorsoLegLayers.Add(Item.bodySlot, asset);
				}
			}
			Item.ResearchUnlockCount = 1;
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
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 20);
			recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 28);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Lost_Pants : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Lost Pants");
			// Tooltip.SetDefault("Increased movement speed");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.06f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 14);
			recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 22);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
namespace Origins.Buffs {
	public class Defiled_Exhaustion_Debuff : ModBuff {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Exhaustion");
		}
		public override void Update(Player player, ref int buffIndex) {
			player.manaRegenBuff = false;
			player.manaRegen = 0;
			player.manaRegenCount = 0;
			player.manaRegenBonus = 0;
		}
	}
}
