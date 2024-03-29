using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Riven {
	[AutoloadEquip(EquipType.Head)]
	public class Riven_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => new string[] {
            "ArmorSet",
            "GenericBoostGear"
        };
        public const float lightMagnitude = 0.3f;
		public short GlowMask = -1;
		public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven2_Mask>()] = ModContent.ItemType<Riven_Mask>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven_Mask>()] = ModContent.ItemType<Riven2_Mask>();
            GlowMask = Origins.AddGlowMask(Texture + "_Head_Glow");
			Item.ResearchUnlockCount = 1;
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
			player.setBonus = Language.GetTextValue("Mods.Origins.Items.GenericTooltip.TornSeverity", 10);
			player.GetModPlayer<OriginPlayer>().tornStrengthBoost.Flat += 0.1f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 15);
			recipe.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public string ArmorSetName => "Riven_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Riven_Coat>();
		public int LegsItemID => ModContent.ItemType<Riven_Pants>();
		public string[] SetCategories => new string[] {
			"Torn",
			"TornSource"
		};
	}
	[AutoloadEquip(EquipType.Body)]
	public class Riven_Coat : ModItem, INoSeperateWikiPage {
		public short GlowMask = -1;
		public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven2_Coat>()] = ModContent.ItemType<Riven_Coat>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven_Coat>()] = ModContent.ItemType<Riven2_Coat>();
            GlowMask = Origins.AddGlowMask(Texture + "_Body_Glow");
			Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Riven/Riven_Coat_Body_Glow");
			Item.ResearchUnlockCount = 1;
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
		public override void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
			glowMask = GlowMask;
			color = Color.White;
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
	public class Riven_Pants : ModItem, INoSeperateWikiPage {
        public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven2_Pants>()] = ModContent.ItemType<Riven_Pants>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven_Pants>()] = ModContent.ItemType<Riven2_Pants>();
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
	public class Riven2_Mask : Riven_Mask, IWikiArmorSet, INoSeperateWikiPage {
		public override void AddRecipes() {}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }
		public new string ArmorSetName => "Ancient_Riven_Armor";
		public new int HeadItemID => Type;
		public new int BodyItemID => ModContent.ItemType<Riven2_Coat>();
		public new int LegsItemID => ModContent.ItemType<Riven2_Pants>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Riven2_Coat : Riven_Coat {
		public override void AddRecipes() {}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Riven2_Pants : Riven_Pants {
		public override void AddRecipes() {}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }
	}
}
