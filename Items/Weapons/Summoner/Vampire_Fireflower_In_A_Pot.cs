using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
using Origins.Items.Accessories;
using Origins.Items.Other.Consumables.Broths;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.NPCs;
using Origins.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Vampire_Fireflower_In_A_Pot : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 20;
			Item.knockBack = 1f;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 14;
			Item.shootSpeed = 9f;
			Item.width = 14;
			Item.height = 23;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Vampire_Sunflower_Buff.ID;
			Item.shoot = Vampire_Fireflower.ID;
			Item.noMelee = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Sunflower_In_A_Pot>()
			.AddIngredient(ItemID.BrokenHeroSword)
			.AddIngredient<Messy_Magma_Leech>()
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Vampire_Sunflower_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Vampire_Fireflower.ID
		];
		public override bool IsArtifact => true;
		public override string Texture => "Origins/Buffs/Alt_Sunflower_Buff";
	}
}
namespace Origins.Items.Weapons.Summoner.Minions {
	public class Vampire_Fireflower : Sunflower_Sunny {
		public override bool DiesHorriblyInLava => false;
		public override int ProjectileType => ModContent.ProjectileType<Vampire_Sunflower_P>();
		public override int ProjectileTime => 9;
		public override int BuffToCheck => Vampire_Sunflower_Buff.ID;
		public static new int ID { get; private set; }
		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(ProjectileID.DandelionSeed);
			Texture2D wingTexture = TextureAssets.Projectile[ProjectileID.DandelionSeed].Value;
			Draw(lightColor, TextureAssets.Projectile[Type].Value, null, wingTexture);
			return false;
		}
		protected override void BuffAllies(int team) {
			int buff = 0;
			if (Main.dayTime) buff = BuffID.Sunflower;
			if (Main.bloodMoon || Main.eclipse) buff = ModContent.BuffType<Crazy_Buff>();
			foreach (Player healee in Main.ActivePlayers) {
				if (healee.team == team && Projectile.Center.Clamp(healee.Hitbox).WithinRange(Projectile.Center, 16 * 30.5f)) {
					healee.lifeRegen += 2;
					healee.AddBuff(buff, 2);
					Dust dust = Dust.NewDustDirect(
						healee.position,
						healee.width,
						healee.height,
						DustID.YellowTorch,
						SpeedY: -2
					);
					dust.velocity *= 0.5f;
					dust.noGravity = true;
				}
			}
		}
		internal static void UpdateDOTLifesteal() {
			if (Main.LocalPlayer is not null) Main.LocalPlayer.lifeRegenCount += 5;
		}
	}
	public class Vampire_Sunflower_P : Sunflower_Sunny_P {
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Sunflower_Sunny_P";
		public override Color? GetAlpha(Color lightColor) => new(255, 80, 0, 100);
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			int time = 60 * Main.rand.Next(3, 6);
			target.AddBuff(BuffID.OnFire3, time);
			Max(ref target.GetGlobalNPC<OriginGlobalNPC>().vampireFireflowerTime, time);
		}
	}
	public class Crazy_Buff : ModBuff {
		public override string Texture => $"Terraria/Images/Buff_{BuffID.Sunflower}";
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type].Add(BuffID.Sunflower);
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer oP = player.OriginPlayer();
			player.moveSpeed += 0.15f;
			oP.moveSpeedMult *= 1.3f;
			oP.spawnRateMultiplier *= 1.17f;
			oP.maxSpawnsMultiplier *= 1.2f;
		}
	}
}
