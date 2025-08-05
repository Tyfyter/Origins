using Origins.Items.Materials;
using Origins.Journal;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Constellation : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Constellation_Entry).Name;
		public class Constellation_Entry : JournalEntry {
			public override string TextKey => "Constellation";
			public override JournalSortIndex SortIndex => new("Arabel", 3);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodBow);
			Item.damage = 20;
			Item.width = 26;
			Item.height = 62;
			Item.shootSpeed += 4;
			Item.knockBack = float.Epsilon;
			Item.shoot = ModContent.ProjectileType<Constellation_P>();
			Item.value = Item.sellPrice(gold: 1, silver: 60);
			Item.rare = ItemRarityID.Orange;
			Item.useAnimation = 30;
			Item.useTime = 30;
		}
		public override Vector2? HoldoutOffset() => new(-4f, 0);
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float GID = Main.rand.NextFloat();
			for (int i = 0; i < 2; ++i) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)),
					Item.shoot,
					damage,
					knockback,
					ai2: GID
				);
			}
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
				float mouseLength = Projectile.Distance(Main.MouseWorld);
				float targetDist = mouseLength * Main.rand.NextFloat(0.75f, 1.25f);
				Projectile.localAI[1] = Projectile.velocity.Length();
				TargetFrames = targetDist / Projectile.localAI[1];
			}
		}
		public float TargetFrames {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
			Projectile.ai[1]++;

			if (Projectile.ai[1] > Projectile.ai[0]) {
				Projectile.velocity *= 0.9f; //slow and stop
				if (Projectile.ai[1] > Projectile.ai[0] + 30) { //die after slowing for 30 frames
					Projectile.Kill();
				}
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
				int type = ModContent.ProjectileType<ConstellationNode>();
				if (Projectile.TryGetOwner(out Player _)) {
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						Vector2.Zero,
						type,
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						ai0: Projectile.ai[2]
					);
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) => new(1f, 1f, 1f, 0.9f);
	}

	public class ConstellationNode : ModProjectile {

		private const float FADE_TIME = 30f;
		private const float EXPNDRPR = 0.25f;

		public float GroupID => Projectile.ai[0];

		public bool GroupRoot {
			get => Projectile.ai[1] == 1;
			set => Projectile.ai[1] = value ? 1 : 0;
		}

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicMissile;

		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.timeLeft = 10 * 60;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.friendly = true;
		}
		public List<Projectile> Nodes {
			get {
				List<Projectile> cns = new();
				foreach (Projectile p in Main.ActiveProjectiles) {
					if (p.ai[0] == Projectile.ai[0] && Projectile.owner == p.owner && p.ModProjectile is ConstellationNode cn) {
						cns.Add(p);
					}
				}
				return cns;
			}
		}
		public List<UnorderedTuple<int>> NodeConnections {
			get {
				List<UnorderedTuple<int>> connections = new();
				var n = Nodes.Count;
				for (int i = 0; i < n; i++) {
					for (int j = i + 1; j < n; j++) {  // Start from i+1 to avoid duplicates
						connections.Add((i, j));
					}
				}
				return connections;
			}
		}

		public override void OnSpawn(IEntitySource source) {
			GroupRoot = Nodes.Count == 1;
		}
		public override void AI() {
			if (Projectile.owner != Main.myPlayer) Projectile.timeLeft = 5;
			if (Projectile.timeLeft > 5 * 60) Projectile.timeLeft = 5 * 60;

			if (Nodes.Count > 1)
				Projectile.frameCounter++;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCs.Add(index);
			base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
		}

		private float Progress => Projectile.timeLeft > FADE_TIME ? MathF.Min(Projectile.frameCounter / FADE_TIME, 1f) : MathF.Max(Projectile.timeLeft / FADE_TIME, 0f);

		private float DrawWidth => MathHelper.Clamp(Progress * (1 + EXPNDRPR) - EXPNDRPR, 0f, 1f);

		public override bool PreDraw(ref Color lightColor) {
			if (GroupRoot) {
				if (Nodes.Count > 1) {
					for (int i = 0; i < NodeConnections.Count; i++) {
						var ca = Nodes[NodeConnections[i].a].Center - Main.screenPosition;
						var cb = Nodes[NodeConnections[i].b].Center - Main.screenPosition;

						cb = Vector2.Lerp(ca, cb, Progress);

						ModContent.GetInstance<ConstellationDrawSystem>().AddDrawPoint(ca, cb, DrawWidth);
					}
					for (int i = 0; i < NodeConnections.Count; i++) {
						var ca = Nodes[NodeConnections[i].a].Center - Main.screenPosition;
						var cb = Nodes[NodeConnections[i].b].Center - Main.screenPosition;
						cb = Vector2.Lerp(ca, cb, Progress);
						OriginExtensions.DrawLightningArc(Main.spriteBatch,
							[ca, cb],
							colors: [
								(0.15f, new Color(80, 204, 219, 0) * 0.5f),
								(0.1f, new Color(80, 251, 255, 0) * 0.5f),
								(0.05f, new Color(200, 255, 255, 0) * 0.5f)
							]
						);
					}
				}
				DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				Vector2.Zero,
				null,
				Color.White
				) {
					origin = TextureAssets.Projectile[Type].Size() * 0.5f,
					scale = new Vector2(Progress)
				};
				for (int i = 0; i < Nodes.Count; i++) {
					data.position = Nodes[i].Center - Main.screenPosition;
					Main.EntitySpriteDraw(data);
				}
			}
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (GroupRoot) {
				foreach (var c in NodeConnections) {
					float _ = 0;
					Vector2 start = Nodes[c.a].Center;
					Vector2 b = Nodes[c.b].Center;
					Vector2 end = Vector2.Lerp(start, b, Progress);
					if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, DrawWidth, ref _)) {
						return true;
					}
				}
			}
			return projHitbox.Intersects(targetHitbox);
		}

		public record struct Constellation_Node(Vector2 Position, int Damage, float Knockback, float Speed, int Type);
	}
	public class ConstellationDrawSystem : ModSystem {

		private List<(Vector2, Vector2, float)> drawPoints = [];

		public void AddDrawPoint(Vector2 start, Vector2 end, float progress) {
			drawPoints.Add((start, end, progress));
		}

		public override void PostDrawTiles() {
			Main.spriteBatch.Begin();
			foreach (var drawPoint in drawPoints) {
				OriginExtensions.DrawConstellationLine(Main.spriteBatch, drawPoint.Item1, drawPoint.Item2, 20f * drawPoint.Item3);
			}
			drawPoints.Clear();
			Main.spriteBatch.End();
		}
	}
}