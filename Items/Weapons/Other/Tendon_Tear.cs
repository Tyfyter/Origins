using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
    public class Tendon_Tear : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tendon Tear");
			Tooltip.SetDefault("");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Musket);
            Item.damage = 6;
            Item.crit = -2;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.width = 70;
            Item.height = 26;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            int i = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            Projectile.NewProjectile(source, position, velocity, Tendon_Tear_Connected.ID, damage, knockback, player.whoAmI, ai1:i);
            return false;
        }
	}
	public class Tendon_Tear_Connected : ModProjectile {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Origins/Items/Weapons/Other/Tendon_Tear_P";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tendon Tear");
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.timeLeft = 14;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
		}
		public override void Kill(int timeLeft) {
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * 0.5f, Tendon_Tear_Swing.ID, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1:Projectile.ai[1]);
		}
	}
	public class Tendon_Tear_Swing : ModProjectile, IWhipProjectile {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Origins/Items/Weapons/Other/Tendon_Tear_P";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tendon Tear");
			// This makes the projectile use whip collision detection and allows flasks to be applied to it.
			ProjectileID.Sets.IsAWhip[Type] = true;
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
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			//Projectile.extraUpdates = 0;
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
					Projectile.velocity /= Projectile.MaxUpdates;
				}
			}

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = 15 * Projectile.MaxUpdates;

			if (++Timer >= swingTime) {
				Projectile.Kill();
				return;
			}

			// These two lines ensure that the timing of the owner's use animation is correct.
			if (Timer == swingTime / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(SoundID.Item153, points[^1]);
			}
		}

		public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			timeToFlyOut = 15 * Projectile.MaxUpdates;
			segments = 20;
			rangeMultiplier = 0.9f * Projectile.scale;
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(List<Vector2> list) {
			//Texture2D texture = TextureAssets.FishingLine.Value;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new Vector2(texture.Width / 2, 3);
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
				Rectangle frame = new Rectangle(0, progress + 2, 6, 2);
				progress += (int)dist;
				float rotation = diff.ToRotation();
				Color color = Color.Lerp(Lighting.GetColor(element.ToTileCoordinates(), Color.White), Color.White, 0.85f);
				Vector2 scale = Vector2.One;//new Vector2(1, dist / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation + MathHelper.Pi, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			FillWhipControlPoints(Projectile, list);
			for (int i = 0; i < list.Count - 1; i++) Dust.NewDustPerfect(list[i], 6);
			DrawLine(list);
			return false;
		}
		public void FillWhipControlPoints(Projectile proj, List<Vector2> controlPoints) {
			Projectile.GetWhipSettings(proj, out var timeToFlyOut, out var segments, out var rangeMultiplier);
			float num = proj.ai[0] / timeToFlyOut;
			float num10 = 0.5f;
			float num11 = 1f + num10;
			float num12 = (float)System.Math.PI * 10f * (1f - num * num11) * (float)(-proj.spriteDirection) / (float)segments;
			float num13 = num * num11;
			float num14 = 0f;
			if (num13 > 1f) {
				num14 = (num13 - 1f) / num10;
				num13 = MathHelper.Lerp(1f, 0f, num14);
			}
			float num15 = proj.ai[0] - 1f;
			Player player = Main.player[proj.owner];
			Item heldItem = Main.player[proj.owner].HeldItem;
			num15 = (float)(ContentSamples.ItemsByType[heldItem.type].useAnimation * 2) * num * player.whipRangeMultiplier;
			float num16 = proj.velocity.Length() * num15 * num13 * rangeMultiplier / (float)segments;
			float num17 = 1f;
			Vector2 playerArmPosition = Projectile.Center;
			Vector2 vector = playerArmPosition;
			float num2 = -(float)System.Math.PI / 2f;
			Vector2 value = vector;
			float num3 = (float)System.Math.PI / 2f + (float)System.Math.PI / 2f * (float)proj.spriteDirection;
			Vector2 value2 = vector;
			float num4 = (float)System.Math.PI / 2f;
			controlPoints.Add(playerArmPosition);
			for (int i = 0; i < segments; i++) {
				float num5 = (float)i / (float)segments;
				float num6 = num12 * num5 * num17;
				Vector2 vector2 = vector + num2.ToRotationVector2() * num16;
				Vector2 vector3 = value2 + num4.ToRotationVector2() * (num16 * 2f);
				Vector2 vector4 = value + num3.ToRotationVector2() * (num16 * 2f);
				float num7 = 1f - num13;
				float num8 = 1f - num7 * num7;
				Vector2 value3 = Vector2.Lerp(vector3, vector2, num8 * 0.9f + 0.1f);
				Vector2 value4 = Vector2.Lerp(vector4, value3, num8 * 0.7f + 0.3f);
				Vector2 spinningpoint = playerArmPosition + (value4 - playerArmPosition) * new Vector2(1f, num11);
				float num9 = num14;
				num9 *= num9;
				Vector2 item = spinningpoint.RotatedBy(proj.rotation + 4.712389f * num9 * (float)proj.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				num2 += num6;
				num4 += num6;
				num3 += num6;
				vector = vector2;
				value2 = vector3;
				value = vector4;
			}
		}
	}
}
