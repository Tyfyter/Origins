using Origins.Buffs;
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
			"Artifact",
			"Minion"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 14;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 38;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Terratotem_Buff.ID;
			Item.shoot = Terratotem_Tab.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Terratotem_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Terratotem_Tab.ID,
		];
		public override bool IsArtifact => true;
	}
}
