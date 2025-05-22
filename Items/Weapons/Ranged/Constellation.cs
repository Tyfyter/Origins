using MagicStorage.Common.Systems.RecurrentRecipes;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.Donate;

namespace Origins.Items.Weapons.Ranged {
	public class Constellation : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodBow);
			Item.damage = 9;
			Item.width = 26;
			Item.height = 62;
			Item.knockBack = float.Epsilon;
			Item.shoot = ModContent.ProjectileType<Constellation_P>();
			Item.value = Item.sellPrice(copper: 30);
		}
		public override Vector2? HoldoutOffset() => new(-4f, 0);
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(12)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				Item.shoot,
				damage,
				knockback,
				ai0: type == ProjectileID.WoodenArrowFriendly ? Item.shoot : type
			);
			return false;
		}
	}
	public class Constellation_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 5;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse) {
				TargetPos = Main.MouseWorld;
				Projectile.localAI[1] = Projectile.velocity.Length();
			}
		}
		public Vector2 TargetPos {
			get => new(Projectile.ai[1], Projectile.ai[2]);
			set => (Projectile.ai[1], Projectile.ai[2]) = value;
		}
		public override void AI() {
			if (Projectile.ai[0] != 0) {
				Vector2 combined = Projectile.velocity * (TargetPos - Projectile.Center);
				if (combined.X <= 0 && combined.Y <= 0) Projectile.Kill();
			}
			float inShimmer = Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && Collision.shimmer ? 1 : 0;
			if (Projectile.localAI[0] != inShimmer) {
				Projectile.localAI[0] = inShimmer;
				if (inShimmer == 1) {
					Projectile.velocity.Y = -Projectile.velocity.Y;
					Projectile.ai[1] += (Projectile.Center.Y - Projectile.ai[1]) * 2;
				}
			}
			if (++Projectile.frameCounter >= 3) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 24 / 2;

			int HalfProjWidth = Projectile.width / 2;

			// Vanilla configuration for "hitbox towards the end"
			if (Projectile.spriteDirection == 1) {
				DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = (int)-DrawOriginOffsetX * 2;
				DrawOriginOffsetY = 0;
			} else {
				DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = 0;
				DrawOriginOffsetY = 0;
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.ai[0] != 0) {
				int type = ModContent.ProjectileType<Actual_Constellation>();
				List<Actual_Constellation> inRangeOf = [];
				Vector2 center = Projectile.Center;
				const float max_dist_sq = 16 * 16 * 15 * 15;
				int highestTimeLeft = 0;
				foreach (Projectile other in Main.ActiveProjectiles) {
					if (other.type == type && other.owner == Projectile.owner && other.ModProjectile is Actual_Constellation otherConstellation) {
						if (otherConstellation.nodes.Count < 5 && otherConstellation.nodes.Any(node => node.Position.DistanceSQ(center) < max_dist_sq)) {
							inRangeOf.Add(other.ModProjectile as Actual_Constellation);
							if (highestTimeLeft < other.timeLeft) highestTimeLeft = other.timeLeft;
						}
					}
				}
				if (inRangeOf.Count > 0) {
					if (inRangeOf.Count > 1) {
						inRangeOf[0].Merge(inRangeOf.Skip(1).SelectMany(c => c.nodes));
						for (int i = 1; i < inRangeOf.Count; i++) {
							inRangeOf[i].Projectile.active = false;
						}
					}
					inRangeOf[0].AddNode(Projectile.position, Projectile.damage, Projectile.knockBack, Projectile.localAI[1], (int)Projectile.ai[0]);
					inRangeOf[0].Projectile.timeLeft = highestTimeLeft + 60;
				} else {
					Projectile.SpawnProjectile(null,
						Projectile.Center,
						default,
						type,
						Projectile.damage,
						Projectile.knockBack,
						Projectile.ai[0],
						Projectile.localAI[1]
					);
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) => new(1f, 1f, 1f, 0.9f);
	}
	public class Actual_Constellation : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicMissile;
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.timeLeft = 5 * 60;
		}
		public List<Constellation_Node> nodes = [];
		public List<UnorderedTuple<int>> nodeConnections = [];
		public void Merge(IEnumerable<Constellation_Node> nodes) {
			foreach (Constellation_Node node in nodes) {
				AddNode(node);
			}
		}
		public void AddNode(Vector2 position, int damage, float knockback, float speed, int type) {
			AddNode(new(position, damage, knockback, speed, type));
		}
		public void AddNode(Constellation_Node node) {
			int newNode = nodes.Count;
			nodes.Add(node);
			for (int i = 0; i < nodes.Count; i++) {
				const float max_dist_sq = 16 * 16 * 15 * 15;
				if (nodes[i].Position.DistanceSQ(node.Position) < max_dist_sq) {
					UnorderedTuple<int> tuple = new(newNode, i);
					if (!nodeConnections.Contains(tuple)) nodeConnections.Add(tuple);
				}
			}
		}
		public override void AI() {
			if (nodes.Count == 0) nodes.Add(new(Projectile.position, Projectile.damage, Projectile.knockBack, Projectile.ai[1], (int)Projectile.ai[0]));
			if (Projectile.owner != Main.myPlayer) Projectile.timeLeft = 5;
			if (Projectile.timeLeft > 5 * 60) Projectile.timeLeft = 5 * 60;
			if (nodes.Count >= 5) Projectile.timeLeft -= 20;
		}
		public override void OnKill(int timeLeft) {
			Vector2 targetPos = Vector2.Zero;
			float targetWeight = 16 * 20;
			bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
				Vector2 currentPos = target.Center;
				float dist = nodes.Min(node => Math.Abs(node.Position.X - currentPos.X) + Math.Abs(node.Position.Y - currentPos.Y));
				if (target is Player) dist *= 2.5f;
				if (dist < targetWeight) {
					targetWeight = dist;
					targetPos = currentPos;
					return true;
				}
				return false;
			});
			for (int i = 0; i < nodes.Count; i++) {
				Vector2 dir = foundTarget ? nodes[i].Position.DirectionTo(targetPos) : Main.rand.NextVector2CircularEdge(1, 1);
				Projectile.SpawnProjectile(null,
					nodes[i].Position,
					dir * Projectile.ai[1],
					(int)Projectile.ai[0],
					Projectile.damage,
					Projectile.knockBack
				);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			for (int i = 0; i < nodeConnections.Count; i++) {
				OriginExtensions.DrawDebugLineSprite(nodes[nodeConnections[i].a].Position, nodes[nodeConnections[i].b].Position, Color.White, -Main.screenPosition);
			}
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				Vector2.Zero,
				null,
				Color.White
			) {
				origin = TextureAssets.Projectile[Type].Size() * 0.5f
			};
			for (int i = 0; i < nodes.Count; i++) {
				data.position = nodes[i].Position - Main.screenPosition;
				Main.EntitySpriteDraw(data);
			}
			return false;
		}
		public record struct Constellation_Node(Vector2 Position, int Damage, float Knockback, float Speed, int Type);
	}
}
