using Origins.Core;
using Origins.Layers;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
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
	public override bool WeaponPrefix() => true;
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Accessory, 2));
	}
	public static void BuffPlayer(Player player, float distance) {
		bool reallyClose = distance <= 16 * 5;
		player.OriginPlayer().smogPodStrength = reallyClose ? 2 : 1;
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

		player.buffImmune[BuffID.Darkness] = true;
		player.buffImmune[BuffID.Obstructed] = true;
		player.buffImmune[BuffID.WindPushed] = true;
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
	public override bool OnTileCollide(Vector2 oldVelocity) {
		if (Projectile.velocity.Y > oldVelocity.Y) Projectile.localAI[0] = 1;
		return base.OnTileCollide(oldVelocity);
	}
	public override void OnKill(int timeLeft) {
		ExplosiveGlobalProjectile.DoExplosion(Projectile, 48, false, SoundID.Item14);
		Projectile.SpawnProjectile(
			Projectile.GetSource_Death(),
			Projectile.Center,
			default,
			ModContent.ProjectileType<Smog_Pod_4_Rod>(),
			Projectile.damage,
			Projectile.knockBack,
			Projectile.localAI[0]
		);
		OriginExtensions.FadeOutOldProjectilesAtLimit([ModContent.ProjectileType<Smog_Pod_4_Rod>()], 20, 255);
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
		Projectile.timeLeft = 60 * 60;
		Projectile.penetrate = -1;
		Projectile.Opacity = 0;
		Projectile.tileCollide = false;
		Projectile.hide = true;
	}
	public override void OnSpawn(IEntitySource source) {
		float dirMult = (Projectile.ai[0] == 0).ToDirectionInt();
		Projectile.Center += Vector2.UnitY * (CollisionExt.Raymarch(Projectile.Center, Vector2.UnitY * dirMult, Projectile.height) + 4) * dirMult;
		(Projectile.ai[1], Projectile.ai[2]) = Projectile.Center;
		Projectile.position -= Vector2.UnitY * (Projectile.height * 0.5f) * dirMult;
		Projectile.netUpdate = true;
	}
	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
		behindNPCsAndTiles.Add(index);
	}
	Arm[] arms;
	Arm[] Arms {
		get {
			if (arms is null) {
				float dirMult = (Projectile.ai[0] == 0).ToDirectionInt();
				arms = new Arm[7];
				Vector2 pos = new(Projectile.ai[1], Projectile.ai[2]);
				for (int i = 0; i < arms.Length; i++) {
					arms[i] = new() { start = pos };
					pos -= Vector2.UnitY * dirMult * Projectile.height / arms.Length;
					arms[i].end = pos;
				}
			}
			return arms;
		}
	}
	public override void AI() {
		float mult = Projectile.Opacity;
		Player player = Main.player[Projectile.owner];

		foreach (Player healee in Main.ActivePlayers) {
			if (healee.team == player.team) {
				Vector2 nearestPoint = healee.Center.Clamp(Projectile.Hitbox);
				float distSQ = nearestPoint.Clamp(healee.Hitbox).DistanceSQ(nearestPoint);
				if (distSQ < 16 * 16 * 15 * 15) Min(ref player.OriginPlayer().nearestSmogPod, distSQ);
			}
		}
		Projectile.rotation = Projectile.ai[0] * MathHelper.Pi - MathHelper.PiOver2;
		if (Main.SettingsEnabled_TilesSwayInWind) {
			for (int i = 1; i < Arms.Length; i++) {
				arms[i].MoveByStart(arms[i - 1].end);
				float dot = Math.Abs(Vector2.Dot((arms[i].end - arms[i].start).Normalized(out _), Vector2.UnitY));
				float windFactor = Main.instance.TilesRenderer.GetWindCycle((int)arms[i].start.X / 16, (int)arms[i].start.Y / 16, Main.instance.TilesRenderer._treeWindCounter);
				arms[i].end = arms[i].end.RotatedBy(windFactor * 0.001f * (i * 0.5f + 1) * dot, arms[i].start);
			}
		}

		for (int i = 0; i < arms.Length; i++) {
			float targetRotation = i <= 0 ? Projectile.rotation : (arms[i - 1].end - arms[i - 1].start).ToRotation();
			float angleDiff = GeometryUtils.AngleDif((arms[i].end - arms[i].start).ToRotation(), targetRotation, out int dir);
			arms[i].end = arms[i].end.RotatedBy(Math.Min(angleDiff * 0.05f + 0.05f, angleDiff * 0.1f) * dir, arms[i].start);
		}
		Lighting.AddLight(arms[^1].end, 3 * mult, 1.86f * mult, 0.3f * mult);

		Projectile.alpha.Cooldown(0, 85);
		Max(ref Projectile.alpha, 255 - Projectile.timeLeft);
		if (Projectile.alpha > 0) return;
		if (Projectile.frame < Main.projFrames[Type] - 1 && Projectile.frameCounter.CycleUp(5)) Projectile.frame.Warmup(Main.projFrames[Type]);
	}
	private static VertexStrip _vertexStrip = new();
	float[] rot;
	Vector2[] pos;
	Color[] colors;
	public override bool PreDraw(ref Color lightColor) {
		rot ??= new float[Arms.Length + 1];
		pos ??= new Vector2[arms.Length + 1];
		colors ??= new Color[arms.Length + 1];
		for (int i = 0; i < arms.Length; i++) {
			rot[i] = (arms[i].end - arms[i].start).ToRotation();
			pos[i] = arms[i].start;
			colors[i] = Lighting.GetColor(arms[i].start.ToTileCoordinates()) * Projectile.Opacity;
		}
		rot[^1] = rot[^2];
		pos[^1] = arms[^1].end;
		colors[^1] = Lighting.GetColor(arms[^1].start.ToTileCoordinates()) * Projectile.Opacity;
		GameShaders.Misc["Origins:Identity"].UseImage0(TextureAssets.Projectile[Type])
		.Apply(null,
			new("uSourceRect0", new Vector4(0, Projectile.frame / (float)Main.projFrames[Type], 1, 1f / Main.projFrames[Type])),
			new("uUVMatrix0", Vector2.UnitY, -Vector2.UnitX, Vector2.UnitY)
		);
		_vertexStrip.PrepareStrip(pos, rot, StripColors, _ => Projectile.width * 0.5f, -Main.screenPosition, arms.Length + 1, includeBacksides: true);
		_vertexStrip.DrawTrail();
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		return false;
	}
	private Color StripColors(float progressOnStrip) => colors[(int)(progressOnStrip * arms.Length)];
	public class Arm {
		public Vector2 start;
		public Vector2 end;
		public void MoveByStart(Vector2 target) {
			Vector2 diff = target - start;
			start += diff;
			end += diff;
		}
		public void MoveByEnd(Vector2 target) {
			Vector2 diff = target - end;
			start += diff;
			end += diff;
		}
		public void DoIKMove(Vector2 target, float maxSpeed) {
			Vector2 diff = target - end;
			Vector2 newPos = end + diff.WithMaxLength(maxSpeed);
			float angleDiff = GeometryUtils.AngleDif((end - start).ToRotation(), (target - start).ToRotation(), out int dir);
			end = end.RotatedBy(angleDiff * dir, start);
			MoveByEnd(newPos);
		}
	}
}
