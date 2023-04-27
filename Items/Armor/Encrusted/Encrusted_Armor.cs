using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Encrusted {
	[AutoloadEquip(EquipType.Head)]
	public class Encrusted_Mask : ModItem {
		public const float lightMagnitude = 0.3f;
		public static short GlowMask = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Encrusted Mask");
			Tooltip.SetDefault("Increases minion damage by 10%");
			GlowMask = Origins.AddGlowMask("Armor/Encrusted/Encrusted_Mask_Head_Glow");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
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
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 15);
			recipe.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Encrusted_Coat : ModItem {
		public static short GlowMask = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Encrusted Coat");
			Tooltip.SetDefault("Increases your max number of minions by 1");
			GlowMask = Origins.AddGlowMask("Armor/Encrusted/Encrusted_Coat_Body_Glow");
			Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Encrusted/Encrusted_Coat_Body_Glow");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions++;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			glowMaskColor = Color.White;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 25);
			recipe.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Encrusted_Pants : ModItem {
		public static short GlowMask = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Encrusted Pants");
			Tooltip.SetDefault("Increases jump height");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.jumpSpeedBoost += 1f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 20);
			recipe.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Encrusted2_Mask : Encrusted_Mask {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Encrusted Mask");
		}
		[AutoloadEquip(EquipType.Body)]
		public class Encrusted2_Coat : Encrusted_Coat {
			public override void SetStaticDefaults() {
				base.SetStaticDefaults();
				DisplayName.SetDefault("Ancient Encrusted Coat");
			}
		}
		[AutoloadEquip(EquipType.Legs)]
		public class Encrusted2_Pants : Encrusted_Pants {
			public override void SetStaticDefaults() {
				base.SetStaticDefaults();
				DisplayName.SetDefault("Ancient Encrusted Pants");
			}
		}
	}
}
