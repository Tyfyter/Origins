using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Flagellash : ModItem {
		static short glowmask;
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			// Call this method to quickly set some of the properties below.
			Item.DefaultToWhip(ModContent.ProjectileType<Flagellash_P>(), 13, 2, 4, 42);

			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 15)
            .AddTile(TileID.Anvils)
            .Register();
        }
        public override bool MeleePrefix() => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Flagellash_P.pitch = Main.rand.NextFloat(0.1f, 0.5f);
			float scale = player.GetAdjustedItemScale(Item);
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI).scale *= scale;
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, ai2: 1).scale *= scale;
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, ai2: 2).scale *= scale;
			return false;
		}
	}
	public class Flagellash_P : ModProjectile, IWhipProjectile {
		internal static float pitch = 0;
		float _pitch = 0;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.timeLeft = 3600;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			_pitch = pitch;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override bool PreAI() {
			if (Projectile.timeLeft > 3600) {
				return false;
			}
			return true;
		}
		public override void AI() {
			if (Projectile.ai[2] != 0) {
				Projectile.timeLeft += (int)Projectile.ai[2] * 7;
				Projectile.ai[2] = 0;
			}
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

			if (Timer >= swingTime || owner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the owner's use animation is correct.
			if (Projectile.ai[1] == 1) {
				owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
				owner.itemTime = owner.itemAnimation;
			}

			if (Timer == swingTime / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(Origins.Sounds.MultiWhip.WithPitchOffset(_pitch), points[^1]);
			}
		}

		public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			timeToFlyOut = (Main.player[Projectile.owner].itemAnimationMax) * Projectile.MaxUpdates;
			segments = 20;
			rangeMultiplier = 0.9f * Projectile.scale;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Flagellash_Buff_0.ID + (int)Projectile.ai[1], 240);
			Projectile.damage = (int)(Projectile.damage * 0.68);
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(List<Vector2> list) {
			//Texture2D texture = TextureAssets.FishingLine.Value;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new(texture.Width / 2, 3);
			int progress = -2;
			Vector2 pos = list[0];
			for (int i = 0; i < list.Count; i++) {
				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}
				if (diff.HasNaNs()) {
					continue;
				}

				float dist = (diff.Length() + 2);
				if (progress + dist >= texture.Width - 2) {
					progress = 0;
				}
				if (i == list.Count - 1) {
					progress = texture.Width - (int)dist;
				}
				Rectangle frame = new(0, progress + 2, 6, (int)dist);
				progress += (int)dist;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Color.Lerp(Lighting.GetColor(element.ToTileCoordinates()), Color.White, 0.85f);
				Vector2 scale = Vector2.One;//new Vector2(1, dist / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation + MathHelper.Pi, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = [];
			Projectile.FillWhipControlPoints(Projectile, list);
			DrawLine(list);
			return false;
		}
	}
}
