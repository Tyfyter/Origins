using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles;
using Origins.Tiles.Ashen;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
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
			Item.DefaultToCanisterLauncher<Matrix_P>(32, 50, 6f, 46, 18, true);
			Item.value = Item.sellPrice(silver: 45);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<NE8>(), 15)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-4f, -2f);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound((Origins.Sounds.EnergyRipple), player.MountedCenter);
			return null;
		}
	}
	public class Matrix_P : ModProjectile {
		(Vector2 position, Vector2 velocity)[] nodes;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			//Projectile.tileCollide = true;
			Projectile.width = Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 0;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = false;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = Main.rand.NextFloat(0, MathHelper.TwoPi);
			Projectile.ai[1] = Main.rand.Next(3, 6);
			Projectile.ai[2] = 0;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Projectile.rotation = Projectile.ai[0];
			Projectile.ai[2]++;
			Projectile.position = Main.player[Projectile.owner].itemLocation;
			if (nodes is null) {
				nodes = new (Vector2, Vector2)[(int)Projectile.ai[1]];
			} else {
				for (int i = 0; i < GetNodeCount(); i++) {
					(Vector2 position, Vector2 velocity) = nodes[i];
					Vector2 vel2 = Collision.TileCollision(position - Vector2.One * 3, velocity, 6, 6, true, true);
					if (vel2.X != velocity.X) velocity.X *= -1;
					if (vel2.Y != velocity.Y) velocity.Y *= -1;
					nodes[i] = (position + velocity, velocity * 0.98f);
				}
				int nodeIndex = GetNodeCount();
				int oldNodeCount = GetNodeCount(Projectile.ai[2] - 1);
				if (nodeIndex != oldNodeCount) {
					nodes[oldNodeCount] = (Projectile.position, Projectile.velocity + GeometryUtils.Vec2FromPolar(1, (oldNodeCount / (Projectile.ai[1])) * MathHelper.TwoPi));
				}
			}
		}
		public int GetNodeCount(float? time = null) {
			time ??= Projectile.ai[2];
			return Math.Min(((int)time.Value) / 2, (int)Projectile.ai[1]);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			(Vector2, Vector2)[] lines = new (Vector2, Vector2)[GetNodeCount()];
			for (int i = 0; i < GetNodeCount(); i++) {
				lines[i] = (nodes[i].position, GetNodePosition(i + 1));
			}
			return CollisionExtensions.PolygonIntersectsRect(lines, targetHitbox);
		}
		public override bool PreDraw(ref Color lightColor) {
			for (int i = 0; i < GetNodeCount(); i++) {
				const int alpha = 128;
				Main.spriteBatch.DrawLightningArcBetween(
					nodes[i].position - Main.screenPosition,
					GetNodePosition(i + 1) - Main.screenPosition,
					Main.rand.NextFloat(-3, 3),
					precision: 0.1f,
					(0.15f, new Color(154, 56, 11, alpha) * 0.5f),
					(0.1f, new Color(255, 81, 0, alpha) * 0.5f),
					(0.05f, new Color(255, 177, 140, alpha) * 0.5f)
				);
			}
			for (int i = 0; i < GetNodeCount(); i++) {
				Vector2 pos = nodes[i].position;
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					pos - Main.screenPosition,
					null,
					Lighting.GetColor(pos.ToTileCoordinates()),
					((GetNodePosition(i - 1) + GetNodePosition(i + 1)) * 0.5f - GetNodePosition(i)).ToRotation() - MathHelper.PiOver2,
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
		Vector2 GetNodePosition(int index) => nodes[((index % GetNodeCount()) + GetNodeCount()) % GetNodeCount()].position;
		public override void OnKill(int timeLeft) {
			Vector2[] cachePositions = GetNodePositions();
			Vector2 min = new(float.PositiveInfinity);
			Vector2 max = new(float.NegativeInfinity);
			Projectile.position = Vector2.Zero;
			int length = GetNodeCount();
			for (int i = 0; i < length; i++) {
				min = Vector2.Min(min, cachePositions[i]);
				max = Vector2.Max(max, cachePositions[i]);
				Projectile.position += cachePositions[i] / length;
			}
			SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
			Rectangle checkPos = default;
			Rectangle area = OriginExtensions.BoxOf(min, max);
			for (int i = 0; i < 128; i++) {
				Vector2 pos = Main.rand.NextVector2FromRectangle(area);
				checkPos.X = (int)pos.X;
				checkPos.Y = (int)pos.Y;
				if (Colliding(default, checkPos) == true) {
					Dust.NewDustDirect(pos, 0, 0, DustID.Torch).velocity = (pos - Projectile.position).SafeNormalize(default) * Main.rand.NextFloat(0.5f, 2);
				}
			}
			ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
		}
		public Vector2[] GetNodePositions() {
			Vector2[] nodes = new Vector2[GetNodeCount()];
			for (int i = 0; i < nodes.Length; i++) {
				nodes[i] = GetNodePosition(i);
			}
			return nodes;
		}
	}
}
