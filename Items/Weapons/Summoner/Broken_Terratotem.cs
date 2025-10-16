﻿using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Broken_Terratotem : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Artifact,
			WikiCategories.Minion
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 8;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 38;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Terratotem_Buff.ID;
			Item.shoot = Broken_Terratotem_Tab.ID;
			Item.noMelee = true;
			Item.ArmorPenetration += 2;
		}
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Terratotem_Tab.ID] + player.ownedProjectileCounts[Broken_Terratotem_Tab.ID] < Terratotem.MaxCount;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}

	public class Broken_Terratotem_Tab : Terratotem_Tab {
		public override string Texture => typeof(Terratotem_Tab).GetDefaultTMLName();
		public static new int ID { get; private set; }
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.minionSlots = 1f;
		}
		public override int GetMask() {
			GetBottom(out int count);
			switch (count) {
				case 1:
				case 2:
				case 5:
				return ModContent.ProjectileType<Broken_Terratotem_Mask_Small>();

				case 3:
				case 6:
				case 7:
				return ModContent.ProjectileType<Broken_Terratotem_Mask_Medium>();

				case 4:
				default:
				return ModContent.ProjectileType<Broken_Terratotem_Mask_Big>();
			}
		}
	}
	public class Broken_Terratotem_Mask_Small : Terratotem_Mask_Small {
		public override string Texture => typeof(Terratotem_Mask_Small).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.usesLocalNPCImmunity = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 20;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			base.ModifyHitNPC(target, ref modifiers);
		}
	}
	public class Broken_Terratotem_Mask_Medium : Terratotem_Mask_Medium {
		public override string Texture => typeof(Terratotem_Mask_Medium).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			base.ModifyHitNPC(target, ref modifiers);
		}
	}
	public class Broken_Terratotem_Mask_Big : Terratotem_Mask_Big {
		public override string Texture => typeof(Terratotem_Mask_Big).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			base.ModifyHitNPC(target, ref modifiers);
		}
	}
}
namespace Origins.Buffs {
	public class Terratotem_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Terratotem_Tab.ID,
			Broken_Terratotem_Tab.ID,
		];
		public override bool IsArtifact => true;
	}
}
