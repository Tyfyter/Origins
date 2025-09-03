using Origins.Items.Materials;
using Origins.Journal;
using Origins.Projectiles;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Items.Weapons.Ranged {
	public class Constellation : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Constellation_Entry).Name;
		public class Constellation_Entry : JournalEntry {
			public override string TextKey => "Constellation";
			public override JournalSortIndex SortIndex => new("Arabel", 3);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodBow);
			Item.damage = 22;
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
			float GID = Main.rand.NextFloat(float.Epsilon, 1);
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
			OriginsSets.Projectiles.WeakpointAnalyzerSpawnAction[Type] = (proj, i) => {
				proj.ai[2] = (proj.ai[2] + (i + 1) * 0.01f) % 1;
				if (proj.ai[2] == 0) proj.ai[2] = float.Epsilon;
			};
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

		private const float fade_time = 30f;
		/// <summary>
		/// ?
		/// </summary>
		private const float expand_ratio_proportions = 0.25f;

		public float GroupID => Projectile.ai[0];

		public bool GroupRoot {
			get => Projectile.ai[1] == 1;
			set => Projectile.ai[1] = value ? 1 : 0;
		}
		public bool IsLinked => Projectile.ai[0] == (int)Projectile.ai[0];

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicMissile;
		Projectile LinkedTo => Main.projectile.GetIfInRange(Projectile.GetByUUID(Projectile.owner, Projectile.ai[0]));
		Vector2 _linkPosition;
		public Vector2 LinkPosition {
			get {
				if (LinkedTo is Projectile proj) _linkPosition = proj.Center;
				return _linkPosition;
			}
		}
		public override void SetStaticDefaults() {
			ProjectileID.Sets.NeedsUUID[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.tileCollide = false;
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.timeLeft = 10 * 60;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.friendly = true;
		}

		public override void OnSpawn(IEntitySource source) {
			GroupRoot = true;
		}
		public override void AI() {
			if (Projectile.owner != Main.myPlayer) {
				Projectile.timeLeft = 5;
				if (IsLinked) {
					Projectile proj = LinkedTo;
					if (!(proj?.active ?? false) || proj.ModProjectile is not ConstellationNode) Projectile.Kill();
				}
			}
			if (Projectile.timeLeft > 5 * 60) Projectile.timeLeft = 5 * 60;

			if (IsLinked)
				Projectile.frameCounter++;
			else if (Projectile.IsLocallyOwned() && !Projectile.GetGlobalProjectile<OriginGlobalProj>().weakpointAnalyzerFake) {
				foreach (Projectile other in Main.ActiveProjectiles) {
					if (other == Projectile) continue;
					if (other.ai[0] == Projectile.ai[0] && Projectile.owner == other.owner && other.ModProjectile is ConstellationNode) {
						other.ai[0] = Projectile.projUUID;
						Projectile.ai[0] = other.projUUID;
						other.GetGlobalProjectile<OriginGlobalProj>().weakpointAnalyzerFake = false;
						Projectile.netUpdate = true;
						other.netUpdate = true;
						GroupRoot = false;
						break;
					}
				}
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCs.Add(index);
			base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
		}

		private float Progress => Projectile.timeLeft > fade_time ? MathF.Min(Projectile.frameCounter / fade_time, 1f) : MathF.Max(Projectile.timeLeft / fade_time, 0f);

		private float DrawWidth => MathHelper.Clamp(Progress * (1 + expand_ratio_proportions) - expand_ratio_proportions, 0f, 1f);

		public override bool PreDraw(ref Color lightColor) {
			if (GroupRoot) {
				DrawData data = new(
					TextureAssets.Projectile[Type].Value,
					Vector2.Zero,
					null,
					Color.White
				) {
					origin = TextureAssets.Projectile[Type].Size() * 0.5f,
					scale = new Vector2(Progress)
				};
				if (IsLinked) {
					Vector2 b = LinkPosition;
					if (b != default) {
						Vector2 ca = Projectile.Center - Main.screenPosition;
						Vector2 cb = b - Main.screenPosition;

						cb = Vector2.Lerp(ca, cb, Progress);

						ModContent.GetInstance<ConstellationDrawSystem>().AddDrawPoint(ca, cb, DrawWidth);
						OriginExtensions.DrawLightningArc(Main.spriteBatch,
							[ca, cb],
							colors: [
								(0.15f, new Color(80, 204, 219, 0) * 0.5f),
								(0.1f, new Color(80, 251, 255, 0) * 0.5f),
								(0.05f, new Color(200, 255, 255, 0) * 0.5f)
							]
						);
						data.position = b - Main.screenPosition;
						Main.EntitySpriteDraw(data);
						ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.ItemStack.Value, $"______\n{LinkedTo?.projUUID}\n{LinkedTo?.ai[0]}", data.position, Color.HotPink, 0, default, Vector2.One);
					}
				}
				data.position = Projectile.Center - Main.screenPosition;
				Main.EntitySpriteDraw(data);
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.ItemStack.Value, $"______\n{Projectile.projUUID}\n{Projectile.ai[0]}", data.position, Color.HotPink, 0, default, Vector2.One);
			}
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (GroupRoot && IsLinked) {
				Vector2 b = LinkPosition;
				if (b == default) return projHitbox.Intersects(targetHitbox);
				float _ = 0;
				Vector2 start = Projectile.Center;
				Vector2 end = Vector2.Lerp(start, b, Progress);
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, DrawWidth, ref _)) {
					return true;
				}
			}
			return projHitbox.Intersects(targetHitbox);
		}

		public record struct Constellation_Node(Vector2 Position, int Damage, float Knockback, float Speed, int Type);
	}
	public class ConstellationDrawSystem : ModSystem {

		private readonly List<(Vector2, Vector2, float)> drawPoints = [];

		public void AddDrawPoint(Vector2 start, Vector2 end, float progress) {
			drawPoints.Add((start, end, progress));
		}

		public override void PostDrawTiles() {
			Main.spriteBatch.Begin();
			foreach ((Vector2 start, Vector2 end, float progress) in drawPoints) {
				OriginExtensions.DrawConstellationLine(Main.spriteBatch, start, end, 20f * progress);
			}
			drawPoints.Clear();
			Main.spriteBatch.End();
		}
	}
}