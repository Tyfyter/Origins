using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Acrid {
	public abstract class Acrid_Helmet_Base : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ArmorSet",
			"ExplosiveBoostGear",
			"ToxicSource"
		];
		public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
			Origins.AddGlowMask(this);
			ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
		}
		public abstract override void UpdateEquip(Player player);
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Acrid_Breastplate>() && legs.type == ModContent.ItemType<Acrid_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.GetModPlayer<OriginPlayer>().acridSet = true;
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Acrid");
			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Venom] = true;
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.AddMaxBreath(180);
			if (Main.GameUpdateCount % 4 != 0) {
				player.ignoreWater = true;
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 13)
			.AddIngredient(ModContent.ItemType<Materials.Rubber>(), 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}

	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Dome : Acrid_Helmet_Base, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.defense = 2;
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
	public class Acrid_Headgear : Acrid_Helmet_Base, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.defense = 4;
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
		public string ArmorSetName => "Magic_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Helm : Acrid_Helmet_Base, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.defense = 18;
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
		public string ArmorSetName => "Melee_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Mask : Acrid_Helmet_Base, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.defense = 1;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Summon) += 0.1f;
			player.maxMinions += 2;
			if (player.wet) {
				player.GetDamage(DamageClass.Summon) += 0.08f;
				player.maxMinions += 1;
				player.nightVision = true;
			}
			Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount / 60f))));
		}
		public string ArmorSetName => "Summon_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Head)]
	public class Acrid_Visor : Acrid_Helmet_Base, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.defense = 9;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.2f;
			if (player.wet) {
				player.GetAttackSpeed(DamageClass.Ranged) += 0.16f;
				player.nightVision = true;
			}
			Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount / 60f))));
		}
		public string ArmorSetName => "Ranged_Acrid_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Acrid_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Acrid_Greaves>();
		public bool SharedPageSecondary => true;
	}
	[AutoloadEquip(EquipType.Body)]
	public class Acrid_Breastplate : ModItem, INoSeperateWikiPage {
        public override void SetStaticDefaults() {
			Origins.AddBreastplateGlowmask(this);
			ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = false;
		}
        public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.sellPrice(gold: 2, silver: 40);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegenCount += 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 26)
			.AddIngredient(ModContent.ItemType<Materials.Rubber>(), 30)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Acrid_Greaves : ModItem, INoSeperateWikiPage {
        public override void SetStaticDefaults() {
			Origins.AddLeggingGlowMask(this);
			Origins.AddGlowMask(this);
			ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = false;
		}
        public override void SetDefaults() {
			Item.defense = 14;
			Item.value = Item.sellPrice(gold: 1, silver: 80);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.accFlipper = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 20)
			.AddIngredient(ModContent.ItemType<Materials.Rubber>(), 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
