using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
namespace Origins.Items.Weapons.Demolitionist {
	public class Matrix : ModItem, ICustomWikiStat {
		static short glowmask;
		public string[] Categories => [
			"Launcher"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.damage = 100;
			Item.useTime = 50;
			Item.useAnimation = 50;
			Item.shoot = ModContent.ProjectileType<Matrix_P>();
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Lime;
			Item.autoReuse = true;
			Item.glowMask = glowmask;
			Item.ArmorPenetration += 1;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
	}
	public class Matrix_P : ModProjectile {
		(Vector2 position, Vector2 velocity)[] nodes;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.width = Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 0;
			Projectile.timeLeft = 300;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = Main.rand.NextFloat(0, MathHelper.TwoPi);
			Projectile.ai[1] = Main.rand.Next(3, 6);
			Projectile.ai[2] = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.ai[0];
			Projectile.ai[2]++;
			if (nodes is null) {
				nodes = new (Vector2, Vector2)[(int)Projectile.ai[1]];
				for (int i = 0; i < nodes.Length; i++) {
					nodes[i] = (Projectile.position, Projectile.velocity + GeometryUtils.Vec2FromPolar(1, (i / (Projectile.ai[1])) * MathHelper.TwoPi));
				}
				Projectile.velocity = Vector2.Zero;
			} else {
				for (int i = 0; i < nodes.Length; i++) {
					(Vector2 position, Vector2 velocity) = nodes[i];
					nodes[i] = (position + velocity, velocity * 0.98f);
				}
			}
			//Projectile.velocity *= 0.98f;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2[] nodes = GetNodePositions();
			(Vector2, Vector2)[] lines = new (Vector2, Vector2)[nodes.Length];
			for (int i = 0; i < nodes.Length; i++) {
				lines[i] = (nodes[i].position, nodes[(i + 1) % nodes.Length].position);
			}
			return CollisionExtensions.PolygonIntersectsRect(lines, targetHitbox);
		}
		public override bool PreDraw(ref Color lightColor) {
			//Vector2[] nodes = GetNodePositions();
			for (int i = 0; i < nodes.Length; i++) {
				const int alpha = 128;
				Main.spriteBatch.DrawLightningArcBetween(
					nodes[i].position - Main.screenPosition,
					nodes[(i + 1) % nodes.Length].position - Main.screenPosition,
					Main.rand.NextFloat(-3, 3),
					precision: 0.1f,
					(0.15f, new Color(154, 56, 11, alpha) * 0.5f),
					(0.1f, new Color(255, 81, 0, alpha) * 0.5f),
					(0.05f, new Color(255, 177, 140, alpha) * 0.5f)
				);
			}
			for (int i = 0; i < nodes.Length; i++) {
				Vector2 pos = nodes[i].position;
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					pos - Main.screenPosition,
					null,
					Lighting.GetColor(pos.ToTileCoordinates()),
					0,//nodes[i].ToRotation(),
					TextureAssets.Projectile[Type].Size() * 0.5f,
					1,
					SpriteEffects.None
				);
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.timeLeft = 1;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
			Rectangle checkPos = default;
			float maxDist = GetNodePositions()[0].Length();
			for (int i = 0; i < 48; i++) {
				Vector2 pos = Projectile.position + Main.rand.NextVector2Circular(maxDist, maxDist);
				checkPos.X = (int)pos.X;
				checkPos.Y = (int)pos.Y;
				if (Colliding(default, checkPos) == true) {
					Dust.NewDustDirect(pos, 0, 0, DustID.Torch).velocity = (pos - Projectile.position).SafeNormalize(default) * Main.rand.NextFloat(1);
				}
			}
		}
		Vector2[] cachePositions = [];
		int cacheTime = -1;
		public Vector2[] GetNodePositions() {
			if (Projectile.ai[2] != cacheTime) {
				cacheTime = (int)Projectile.ai[2];
				Vector2[] nodes = new Vector2[(int)Projectile.ai[1]];
				for (int i = 0; i < nodes.Length; i++) {
					nodes[i] = new Vector2(0, Math.Min(MathF.Pow(Projectile.ai[2] * 0.05f, 0.5f), 1) * 48).RotatedBy(Projectile.rotation + (i / (Projectile.ai[1])) * MathHelper.TwoPi);
				}
				cachePositions = nodes;
			}
			return cachePositions;
		}
	}
}
