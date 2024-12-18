using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Partybringer : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher"
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Partybringer_P>(14, 50, 8f, 46, 28, true);
			Item.value = Item.sellPrice(silver: 24);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration += 1;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, -8f);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
	}
	public class Partybringer_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Partybringer_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Partybringer_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.tileCollide = true;
			Projectile.width = Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 300;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.02f * (MathF.Pow((++Projectile.ai[0]) / 30, 0.5f) + 1);
			Projectile.rotation += Projectile.direction * 0.15f;
		}
	}
}
