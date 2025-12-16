using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Journal;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Tendon_Tear : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Tendon_Tear_Entry).Name;
		public class Tendon_Tear_Entry : JournalEntry {
			public override string TextKey => "Tendon_Tear";
			public override JournalSortIndex SortIndex => new("The_Crimson", 10);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.damage = 18;
			Item.crit = -2;
			Item.useAnimation = 38;
			Item.useTime = 38;
			Item.width = 86;
			Item.height = 22;
			Item.UseSound = Origins.Sounds.HeavyCannon;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IllegalGunParts)
			.AddIngredient(ItemID.CrimtaneBar, 8)
			.AddIngredient(ItemID.TissueSample, 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int i = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			float len = velocity.Length() * Main.projectile[i].MaxUpdates;

			Projectile.NewProjectile(source, position, velocity, Tendon_Tear_Swing.ID, damage, knockback, player.whoAmI, ai0: -(728 / len), ai1: i);
			return false;
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(-20, 0);
        }
    }
	public class Tendon_Tear_Swing : ModProjectile, IWhipProjectile {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Items/Weapons/Ranged/Tendon_Tear_P";
		public override void SetStaticDefaults() {
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 3600;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = false;
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.
			if (Projectile.ai[1] >= 0 && Projectile.ai[1] < Main.maxProjectiles) {
				Projectile ownerProj = Main.projectile[(int)Projectile.ai[1]];
				Projectile.Center = ownerProj.Center;
				if (!ownerProj.active) {
					Projectile.ai[1] = -1;
					if (Timer < 0) Timer = 0;
					Projectile.velocity *= 0.25f;
				}
			}

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = 15 * Projectile.MaxUpdates;

			if (++Timer >= swingTime) {
				Projectile.Kill();
				return;
			}

			// These two lines ensure that the timing of the owner's use animation is correct.
			if (Timer == swingTime / 2 || Timer == -15) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				FillWhipControlPoints(points);
				SoundEngine.PlaySound(SoundID.Item153, points[^1]);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.damage = (int)(Projectile.damage * 0.8);
		}
		public override void CutTiles() {
			if (Timer < 0) return;
			List<Vector2> whipPointsForCollision = Projectile.WhipPointsForCollision;
			whipPointsForCollision.Clear();
			FillWhipControlPoints(whipPointsForCollision);
			Vector2 value = new(Projectile.width * Projectile.scale / 2f, 0f);
			for (int i = 0; i < whipPointsForCollision.Count; i++) {
				DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
				Utils.PlotTileLine(whipPointsForCollision[i] - value, whipPointsForCollision[i] + value, Projectile.height * Projectile.scale, DelegateMethods.CutTiles);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Timer < 0) return false;
			List<Vector2> whipPointsForCollision = Projectile.WhipPointsForCollision;
			whipPointsForCollision.Clear();
			FillWhipControlPoints(whipPointsForCollision);
			for (int n = 0; n < whipPointsForCollision.Count; n++) {
				Point point = whipPointsForCollision[n].ToPoint();
				projHitbox.Location = new Point(point.X - projHitbox.Width / 2, point.Y - projHitbox.Height / 2);
				if (projHitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			timeToFlyOut = 15 * Projectile.MaxUpdates;
			segments = 15;
			rangeMultiplier = 1;
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(List<Vector2> list) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new(texture.Width / 2, 3);
			int progress = -2;
			Vector2 pos = list[0];
			for (int i = 0; i < list.Count - 1; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;
				if (diff.HasNaNs()) {
					continue;
				}

				float dist = (diff.Length() + 2);
				if (progress + dist >= texture.Height - 2) {
					progress = 0;
				}
				if (i == list.Count - 2) {
					progress = texture.Height - (int)dist;
				}
				Rectangle frame = new(0, progress + 2, 8, (int)dist);
				progress += (int)dist;
				float rotation = diff.ToRotation();
				Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
				Vector2 scale = new(0.5f, 1f);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation + MathHelper.PiOver2, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new();
			FillWhipControlPoints(list);
			DrawLine(list);
			return false;
		}
		//https://youtu.be/PP5fBKo9998
		public void FillWhipControlPoints(List<Vector2> controlPoints) {
			GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier);
			float progress = MathHelper.Max(Projectile.ai[0], 7) / timeToFlyOut;
			float oneHalf = 0.5f;
			float threeHalves = 1f + oneHalf;
			float rotSpeed = MathHelper.Pi * 10f * (1f - progress * threeHalves) * (-Projectile.spriteDirection) / segments;
			float num13 = progress * threeHalves;
			float num14 = 0f;
			if (num13 > 1f) {
				num14 = (num13 - 1f) / oneHalf;
				num13 = MathHelper.Lerp(1f, 0f, num14);
			}
			float range = 12 * 20 * progress * num13 * rangeMultiplier / segments;
			Vector2 basePosition = Projectile.Center;
			Vector2 pos0 = basePosition;
			float rot0 = -MathHelper.PiOver2;

			Vector2 pos1 = basePosition;
			float rot1 = MathHelper.PiOver2;

			Vector2 pos2 = basePosition;
			float rot2 = MathHelper.PiOver2 * (Projectile.spriteDirection + 1);
			controlPoints.Add(basePosition);
			for (int i = 0; i < segments; i++) {
				float segPercent = i / (float)segments;
				float num6 = rotSpeed * segPercent;
				Vector2 nextPos0 = pos0 + OriginExtensions.Vec2FromPolar(rot0, range);
				Vector2 nextPos1 = pos1 + OriginExtensions.Vec2FromPolar(rot1, range * 2f);
				Vector2 nextPos2 = pos2 + OriginExtensions.Vec2FromPolar(rot2, range * 2f);
				float num7 = 1f - num13;
				float num8 = 1f - num7 * num7;
				Vector2 value4 = Vector2.Lerp(nextPos2, Vector2.Lerp(nextPos1, nextPos0, num8 * 0.9f + 0.1f), num8 * 0.7f + 0.3f);
				Vector2 spinningpoint = basePosition + (value4 - basePosition) * new Vector2(1f, threeHalves);
				float num9 = num14;
				num9 *= num9;
				Vector2 point = spinningpoint.RotatedBy(Projectile.rotation + 4.712389f * num9 * Projectile.spriteDirection, basePosition);
				controlPoints.Add(point);
				rot0 += num6;
				rot1 += num6;
				rot2 += num6;
				pos0 = nextPos0;
				pos1 = nextPos1;
				pos2 = nextPos2;
			}
			if (Projectile.ai[0] < 0) {
				float val = MathHelper.Min(-Projectile.ai[0] / 15, 1);
				Vector2 connectTarget = Main.player[Projectile.owner].MountedCenter;
				for (int i = 0; i <= segments; i++) {
					float segPercent = i / (float)segments;
					controlPoints[i] = Vector2.Lerp(controlPoints[i], connectTarget, segPercent * val);
				}
			}
		}
	}
}
