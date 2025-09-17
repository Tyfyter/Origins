using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Felnum {
	[AutoloadEquip(EquipType.Head)]
	public class Felnum_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public const float shock_damage_divisor = 15f;
		public string[] Categories => [
			"ArmorSet",
			"GenericBoostGear"
		];
		public short GlowMask = -1;
		protected virtual int CraftingStation => TileID.Anvils;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			GlowMask = Origins.AddGlowMask(Texture + "_Head_Glow");
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.05f;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock * 255) / drawPlayer.statLifeMax2, 255), 1);
			glowMaskColor = new Color(a, a, a, a);
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return (body.type == ModContent.ItemType<Felnum_Breastplate>() || body.type == ModContent.ItemType<Ancient_Felnum_Breastplate>()) && (legs.type == ModContent.ItemType<Felnum_Greaves>() || legs.type == ModContent.ItemType<Ancient_Felnum_Greaves>());
		}
		public override void UpdateArmorSet(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Felnum");
			originPlayer.felnumSet = true;
			originPlayer.felnumEnemiesFriendly = true;
			if (player.velocity.Length() > 4) {
				originPlayer.felnumShock += player.velocity.Length() / 4;
			} else if (originPlayer.felnumShock > 0) {
				originPlayer.felnumShock--;
			}
			if (player.HasBuff(BuffID.Electrified)) {
				int id = BuffID.BeetleMight1;
				if (player.HasBuff(BuffID.BeetleMight1)) {
					id = BuffID.BeetleMight2;
				} else if (player.HasBuff(BuffID.BeetleMight2)) {
					id = BuffID.BeetleMight3;
				} else if (player.HasBuff(BuffID.BeetleMight3)) {
					id = BuffID.BeetleEndurance3;
				}
				player.buffType[player.FindBuffIndex(BuffID.Electrified)] = id;
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 15)
			.AddTile(CraftingStation)
			.Register();
		}
		public string ArmorSetName => "Felnum_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Felnum_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Felnum_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Felnum_Breastplate : ModItem, INoSeperateWikiPage {
		public short GlowMask = -1;
		protected virtual int CraftingStation => TileID.Anvils;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			GlowMask = Origins.AddGlowMask(Texture + "_Body_Glow");
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.05f;
			player.moveSpeed += 0.05f;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock * 255) / drawPlayer.statLifeMax2, 255), 1);
			glowMaskColor = new Color(a, a, a, a);
		}
		public override void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
			glowMask = GlowMask;
			int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock * 255) / drawPlayer.statLifeMax2, 255), 1);
			color = new Color(a, a, a, a);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 25)
			.AddTile(CraftingStation)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Felnum_Greaves : ModItem, INoSeperateWikiPage {
		public short GlowMask = -1;
		protected virtual int CraftingStation => TileID.Anvils;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			GlowMask = Origins.AddGlowMask(Texture + "_Legs_Glow");
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.1f;
		}
		public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			glowMask = GlowMask;
			int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock * 255) / drawPlayer.statLifeMax2, 255), 1);
			glowMaskColor = new Color(a, a, a, a);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 20)
			.AddTile(CraftingStation)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Ancient_Felnum_Helmet : Felnum_Helmet, IWikiArmorSet, INoSeperateWikiPage {
		public override LocalizedText Tooltip => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Felnum_Helmet.Tooltip"));
		protected override int CraftingStation => TileID.DemonAltar;
		public new string ArmorSetName => "Ancient_Felnum_Armor";
		public new int HeadItemID => Type;
		public new int BodyItemID => ModContent.ItemType<Ancient_Felnum_Breastplate>();
		public new int LegsItemID => ModContent.ItemType<Ancient_Felnum_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Ancient_Felnum_Breastplate : Felnum_Breastplate {
		public override LocalizedText Tooltip => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Felnum_Breastplate.Tooltip"));
		protected override int CraftingStation => TileID.DemonAltar;
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Ancient_Felnum_Greaves : Felnum_Greaves {
		public override LocalizedText Tooltip => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Felnum_Greaves.Tooltip"));
		protected override int CraftingStation => TileID.DemonAltar;
	}
}
