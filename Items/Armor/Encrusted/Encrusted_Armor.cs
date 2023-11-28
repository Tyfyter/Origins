using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Encrusted {
	[AutoloadEquip(EquipType.Head)]
	public class Encrusted_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public const float lightMagnitude = 0.3f;
		public static short GlowMask = -1;
		public override void SetStaticDefaults() {
			GlowMask = Origins.AddGlowMask(Texture+"_Head_Glow");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Summon) += 0.1f;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			glowMaskColor = Color.White;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Encrusted_Coat>() && legs.type == ModContent.ItemType<Encrusted_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Increases minion damage by up to 30% when over half health";
			player.GetDamage(DamageClass.Summon) *= player.GetModPlayer<OriginPlayer>().rivenMult;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Bud_Barnacle>(), 16);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Ore_Item>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public string ArmorSetName => "Encrusted_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Encrusted_Coat>();
		public int LegsItemID => ModContent.ItemType<Encrusted_Pants>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Encrusted_Coat : ModItem, INoSeperateWikiPage {
		public static short GlowMask = -1;
		public override void SetStaticDefaults() {
			GlowMask = Origins.AddGlowMask(Texture + "_Body_Glow");
			Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Encrusted/Encrusted_Coat_Body_Glow");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions++;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			glowMaskColor = Color.White;
		}
		public override void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
			glowMask = GlowMask;
			color = Color.White;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Bud_Barnacle>(), 28);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Ore_Item>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Encrusted_Pants : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 3;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.jumpSpeedBoost += 1f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Bud_Barnacle>(), 22);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Ore_Item>(), 14);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}