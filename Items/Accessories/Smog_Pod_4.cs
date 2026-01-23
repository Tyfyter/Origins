using Origins.Layers;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
[AutoloadEquip(EquipType.Back)]
public class Smog_Pod_4 : ModItem {
	public static Vector2 GetRocketShootPosition(Player player) {
		return player.MountedCenter - player.Directions(8, 8);
	}
	public static Vector2 GetRocketShootVelocity(Player player) {
		Vector2 velocity = (Main.rand.NextVector2Circular(4f, 4f) + Main.rand.NextVector2CircularEdge(10f, 10f)).Abs(out Vector2 signs);
		MinMax(ref velocity.X, ref velocity.Y);
		signs.X = -player.direction;
		signs.Y = -player.gravDir;
		return velocity * signs;
	}
	public static float HomingTightness => 0.12f;
	public override void SetStaticDefaults() {
		Accessory_Glow_Layer.AddGlowMasks(Item, EquipType.Back);
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Yellow;
		Item.damage = 60;
		Item.DamageType = DamageClass.Generic;
		Item.shoot = ModContent.ProjectileType<Smog_Pod_4_Rocket>();
		Item.knockBack = 1;
		Item.useAnimation = Item.useTime = 8;
		Item.reuseDelay = 5 * 60;
		Item.useLimitPerAnimation = 4;
		Item.value = Item.sellPrice(gold: 5);
	}
	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.lifeRegen += 10;
		player.OriginPlayer().smogPod = Item;
	}
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Accessory, 2));
	}
	public override void ModifyTooltips(List<TooltipLine> tooltips) {
		tooltips.SubstituteKeybind(Keybindings.SmogPod);
	}
	public static void BuffPlayer(Player player, float distance) {
		bool reallyClose = distance <= 16 * 5;
		for (int i = reallyClose ? 2 : 1; i > 0; i--) {
			Dust dust = Dust.NewDustDirect(
				player.position,
				player.width,
				player.height,
				DustID.OrangeTorch,
				SpeedY: -2
			);
			dust.velocity *= 0.5f;
			dust.noGravity = true;
		}

		// temp code:
		player.lifeRegen += reallyClose ? 40 : 10;

		//player.AddBuff(reallyClose ? ModContent.BuffType<>() : ModContent.BuffType<>(), 5);
	}
}
public class Smog_Pod_4_Rocket : ModProjectile {
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Generic;
		Projectile.width = 10;
		Projectile.height = 10;
		Projectile.aiStyle = -1;
		Projectile.friendly = true;
		Projectile.timeLeft = 900;
		Projectile.localNPCHitCooldown = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.appliesImmunityTimeOnSingleHits = true;
		Projectile.penetrate = -1;
		Projectile.Opacity = 0;
	}
	public override void AI() {
		Vector2 targetPos = new(Projectile.ai[0], Projectile.ai[1]);
		if (Projectile.alpha <= 0) {
			Vector2 targetVelocity = (targetPos - Projectile.Center).Normalized(out _);

			targetVelocity *= 16f;
			float speed = Projectile.velocity.Length();
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, Smog_Pod_4.HomingTightness * Math.Min(Projectile.ai[2] / 15, 1)).Normalized(out float newSpeed) * float.Max(speed, newSpeed);
		}

		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		Projectile.alpha.Cooldown(0, 85);
		if (Projectile.alpha > 0) return;
		ref Vector2 dustVel = ref Dust.NewDustPerfect(Projectile.Center - Projectile.velocity, DustID.Torch).velocity;
		dustVel *= 0.5f;
		dustVel -= Projectile.velocity * 0.25f * Math.Min(++Projectile.ai[2] / 15f, 1);
	}
	public override void OnKill(int timeLeft) {
		ExplosiveGlobalProjectile.DoExplosion(Projectile, 48, false, SoundID.Item14);
		Projectile.SpawnProjectile(
			Projectile.GetSource_Death(),
			Projectile.Center,
			default,
			ModContent.ProjectileType<Smog_Pod_4_Rod>(),
			Projectile.damage,
			Projectile.knockBack
		);
	}
}
public class Smog_Pod_4_Rod : ModProjectile {
	public override void SetStaticDefaults() {
		Main.projFrames[Type] = 11;
	}
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Generic;
		Projectile.width = 18;
		Projectile.height = 182;
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 60 * 10;
		Projectile.penetrate = -1;
		Projectile.Opacity = 0;
		Projectile.tileCollide = false;
	}
	public override void OnSpawn(IEntitySource source) {
		Projectile.Bottom = Projectile.Center + Vector2.UnitY * CollisionExt.Raymarch(Projectile.Center, Vector2.UnitY, Projectile.height);
		Projectile.netUpdate = true;
	}
	public override void AI() {
		Player player = Main.player[Projectile.owner];

		foreach (Player healee in Main.ActivePlayers) {
			if (healee.team == player.team) {
				Vector2 nearestPoint = healee.Center.Clamp(Projectile.Hitbox);
				float distSQ = nearestPoint.Clamp(healee.Hitbox).DistanceSQ(nearestPoint);
				if (distSQ < 16 * 16 * 15 * 15) Min(ref player.OriginPlayer().nearestSmogPod, distSQ);
			}
		}

		Projectile.alpha.Cooldown(0, 85);
		Max(ref Projectile.alpha, 255 - Projectile.timeLeft);
		if (Projectile.alpha > 0) return;
		if (Projectile.frame < Main.projFrames[Type] - 1 && Projectile.frameCounter.CycleUp(5)) Projectile.frame.Warmup(Main.projFrames[Type]);
	}
	public override void OnKill(int timeLeft) {
	}
}
