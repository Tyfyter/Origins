using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Acrid {
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Dome : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Dome");
			Tooltip.SetDefault("10% increased explosive critical chance and throwing velocity\nEffect stronger when submerged\nEmit a small aura of light");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Dome_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 2;
			Item.value = Item.buyPrice(gold: 5);
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
			player.setBonus = "Immunity to 'Toxic Shock', 'Solvent', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			//player.buffImmune[BuffID.ToxicShock] = true;
			//player.buffImmune[BuffID.Solvent] = true;
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
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Headgear : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Headgear");
			Tooltip.SetDefault("10% increased magic damage and 25% reduced mana cost\nEffect stronger when submerged\nEmit a small aura of light");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Headgear_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.buyPrice(gold: 5);
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
			player.setBonus = "Immunity to 'Toxic Shock', 'Solvent', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			//player.buffImmune[BuffID.ToxicShock] = true;
			//player.buffImmune[BuffID.Solvent] = true;
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
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Helm : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Helm");
			Tooltip.SetDefault("10% increased melee damage and speed\nEffect stronger when submerged\nEmit a small aura of light");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Helm_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.buyPrice(gold: 5);
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
			player.setBonus = "Immunity to 'Toxic Shock', 'Solvent', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			//player.buffImmune[BuffID.ToxicShock] = true;
			//player.buffImmune[BuffID.Solvent] = true;
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
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Mask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Mask");
			Tooltip.SetDefault("10% increased summoning damage\n+1 minion slot\nEffects stronger when submerged\nEmit a small aura of light");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Mask_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 1;
			Item.value = Item.buyPrice(gold: 5);
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
			player.setBonus = "Immunity to 'Toxic Shock', 'Solvent', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			//player.buffImmune[BuffID.ToxicShock] = true;
			//player.buffImmune[BuffID.Solvent] = true;
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
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Visor : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Visor");
			Tooltip.SetDefault("20% increased ranged attack speed\nEffect stronger when submerged\nEmit a small aura of light");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Visor_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 9;
			Item.value = Item.buyPrice(gold: 5);
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
			player.setBonus = "Immunity to 'Toxic Shock', 'Solvent', 'Acid Venom', and 'Poisoned' debuffs\nExtends underwater breathing";
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			//player.buffImmune[BuffID.ToxicShock] = true;
			//player.buffImmune[BuffID.Solvent] = true;
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
	}
	[AutoloadEquip(EquipType.Body)]
	public class Acrid_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Breastplate");
			Tooltip.SetDefault("Increased life regeneration");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.buyPrice(gold: 5);
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
	public class Acrid_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Greaves");
			Tooltip.SetDefault("Grants the ability to swim");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 14;
			Item.value = Item.buyPrice(gold: 5);
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
