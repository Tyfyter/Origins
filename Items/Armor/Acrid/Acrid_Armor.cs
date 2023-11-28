using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Acrid {
    [AutoloadEquip(EquipType.Head)]
	public class Acrid_Dome : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Dome_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 2;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClasses.Explosive) += 10;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.1f;
			if (player.wet) {
				player.GetCritChance(DamageClasses.Explosive) += 8;
				player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.08f;
				player.nightVision = true;
			}
			Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount / 60f))));
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Acrid_Breastplate>() && legs.type == ModContent.ItemType<Acrid_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.GetModPlayer<OriginPlayer>().acridSet = true;
			player.setBonus = "All attacks inflict 'Toxic Shock'\nImmunity to acid venom, poisoned and toxic shock\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.breathMax += 63;
			if (Main.GameUpdateCount % 4 != 0) {
				player.ignoreWater = true;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 13);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public string MergedArmorSetName => "Acrid_Armor";
		public string ArmorSetName => "Explosive_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public IEnumerable<int> SharedPageItems {
			get {
				yield return ModContent.ItemType<Acrid_Headgear>();
				yield return ModContent.ItemType<Acrid_Helm>();
				yield return ModContent.ItemType<Acrid_Mask>();
				yield return ModContent.ItemType<Acrid_Visor>();
			}
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Headgear : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Headgear_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Magic) += 0.1f;
			player.manaCost -= 0.25f;
			if (player.wet) {
				player.GetDamage(DamageClass.Magic) += 0.08f;
				player.manaCost -= 0.1f;
				player.nightVision = true;
			}
			Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount / 60f))));
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Acrid_Breastplate>() && legs.type == ModContent.ItemType<Acrid_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.GetModPlayer<OriginPlayer>().acridSet = true;
			player.setBonus = "All attacks inflict 'Toxic Shock'\nImmunity to 'Toxic Shock', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.breathMax += 63;
			if (Main.GameUpdateCount % 4 != 0) {
				player.ignoreWater = true;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 13);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public string ArmorSetName => "Magic_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Helm : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Helm_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Melee) += 0.1f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.1f;
			if (player.wet) {
				player.GetDamage(DamageClass.Melee) += 0.08f;
				player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
				player.nightVision = true;
			}
			Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount / 60f))));
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Acrid_Breastplate>() && legs.type == ModContent.ItemType<Acrid_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.GetModPlayer<OriginPlayer>().acridSet = true;
			player.setBonus = "All attacks inflict 'Toxic Shock'\nImmunity to 'Toxic Shock', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.breathMax += 63;
			if (Main.GameUpdateCount % 4 != 0) {
				player.ignoreWater = true;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 13);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public string ArmorSetName => "Melee_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Mask_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 1;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Summon) += 0.1f;
			player.maxMinions += 1;
			if (player.wet) {
				player.GetDamage(DamageClass.Summon) += 0.08f;
				player.maxMinions += 1;
				player.nightVision = true;
			}
			Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount / 60f))));
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Acrid_Breastplate>() && legs.type == ModContent.ItemType<Acrid_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.GetModPlayer<OriginPlayer>().acridSet = true;
			player.setBonus = "All attacks inflict 'Toxic Shock'\nImmunity to 'Toxic Shock', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.breathMax += 63;
			if (Main.GameUpdateCount % 4 != 0) {
				player.ignoreWater = true;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 13);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public string ArmorSetName => "Summon_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Visor : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Visor_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 9;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.2f;
			if (player.wet) {
				player.GetAttackSpeed(DamageClass.Ranged) += 0.16f;
				player.nightVision = true;
			}
			Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount / 60f))));
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Acrid_Breastplate>() && legs.type == ModContent.ItemType<Acrid_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.GetModPlayer<OriginPlayer>().acridSet = true;
			player.setBonus = "All attacks inflict 'Toxic Shock'\nImmunity to 'Toxic Shock', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.breathMax += 63;
			if (Main.GameUpdateCount % 4 != 0) {
				player.ignoreWater = true;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 13);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public string ArmorSetName => "Ranged_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Body)]
	public class Acrid_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegenCount += 2;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 26);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 30);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Acrid_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 14;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.accFlipper = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 20);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 20);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
