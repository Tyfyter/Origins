using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Riven {
	[AutoloadEquip(EquipType.Head)]
	public class Riven_Mask : ModItem {
		public const float lightMagnitude = 0.3f;
		public static short GlowMask = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riven Mask");
			Tooltip.SetDefault("6% increased weapon speed");
			GlowMask = Origins.AddGlowMask("Armor/Riven/Riven_Mask_Head_Glow");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.06f;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			glowMaskColor = Color.White;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Riven_Coat>() && legs.type == ModContent.ItemType<Riven_Pants>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "+15% torn severity";
			player.GetModPlayer<OriginPlayer>().rivenSetBoost = true;
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
	public class Riven_Coat : ModItem {
		public static short GlowMask = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riven Coat");
			Tooltip.SetDefault("6% increased weapon speed");
			GlowMask = Origins.AddGlowMask("Armor/Riven/Riven_Coat_Body_Glow");
			Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Riven/Riven_Coat_Body_Glow");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.06f;
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
	public class Riven_Pants : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riven Pants");
			Tooltip.SetDefault("6% increased weapon speed");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.06f;
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
	public class Riven2_Mask : Riven_Mask {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Riven Mask");
		}
		public override void AddRecipes() {
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }
		[AutoloadEquip(EquipType.Body)]
		public class Riven2_Coat : Riven_Coat {
			public override void SetStaticDefaults() {
				base.SetStaticDefaults();
				DisplayName.SetDefault("Ancient Riven Coat");
			}
			public override void AddRecipes() {
			}
			public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }
		}
		[AutoloadEquip(EquipType.Legs)]
		public class Riven2_Pants : Riven_Pants {
			public override void SetStaticDefaults() {
				base.SetStaticDefaults();
				DisplayName.SetDefault("Ancient Riven Pants");
			}
			public override void AddRecipes() {
			}
			public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }
		}
	}
}
