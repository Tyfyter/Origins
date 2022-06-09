using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Weapons.Defiled {
	public class Bone_Latcher : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bone Latcher");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			// Call this method to quickly set some of the properties below.
			Item.DefaultToWhip(ModContent.ProjectileType<Bone_Latcher_P>(), 20, 2, 4, 35);

			Item.DamageType = DamageClass.Melee;
			Item.damage = 20;
			Item.rare = ItemRarityID.Green;
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			if (Item.noUseGraphic) {
				Item.noUseGraphic = false;
				Item.Prefix(-2);
				Item.noUseGraphic = true;
				return Item.prefix;
			}
			return -1;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI).scale *= Item.scale;
			return false;
		}
		public class Bone_Latcher_P : ModProjectile, IWhipProjectile {
			public override void SetStaticDefaults() {
				DisplayName.SetDefault("Bone Latcher");
				// This makes the projectile use whip collision detection and allows flasks to be applied to it.
				ProjectileID.Sets.IsAWhip[Type] = true;
			}

			public override void SetDefaults() {
				Projectile.DefaultToWhip();
				Projectile.DamageType = DamageClass.Melee;
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

			private float Timer {
				get => Projectile.ai[0];
				set => Projectile.ai[0] = value;
			}

			public override void AI() {
				Player owner = Main.player[Projectile.owner];
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

				Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

				Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

				float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

				if (Timer >= swingTime || owner.itemAnimation <= 0) {
					Projectile.Kill();
					return;
				}

				owner.heldProj = Projectile.whoAmI;

				// These two lines ensure that the timing of the owner's use animation is correct.
				owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
				owner.itemTime = owner.itemAnimation;

				if (Timer == swingTime / 2) {
					// Plays a whipcrack sound at the tip of the whip.
					List<Vector2> points = Projectile.WhipPointsForCollision;
					Projectile.FillWhipControlPoints(Projectile, points);
					SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
				}
			}

			public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
				timeToFlyOut = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
				segments = 20;
				rangeMultiplier = 1.1f * Projectile.scale;
			}
			public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
				float range = 192 * Projectile.scale;
				if (target.DistanceSQ(Main.player[Projectile.owner].Center) > range * range) {
					hitDirection *= -1;
					knockback *= 1.5f;
				}
			}

			// This method draws a line between all points of the whip, in case there's empty space between the sprites.
			private void DrawLine(List<Vector2> list) {
				Texture2D texture = TextureAssets.FishingLine.Value;
				Rectangle frame = texture.Frame();
				Vector2 origin = new Vector2(frame.Width / 2, 2);

				Vector2 pos = list[0];
				for (int i = 0; i < list.Count - 1; i++) {
					Vector2 element = list[i];
					Vector2 diff = list[i + 1] - element;

					float rotation = diff.ToRotation() - MathHelper.PiOver2;
					Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
					Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

					Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

					pos += diff;
				}
			}

			public override bool PreDraw(ref Color lightColor) {
				List<Vector2> list = new List<Vector2>();
				Projectile.FillWhipControlPoints(Projectile, list);

				DrawLine(list);

				SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

				Main.instance.LoadProjectile(Type);
				Texture2D texture = TextureAssets.Projectile[Type].Value;

				Vector2 pos = list[0];

				for (int i = 0; i < list.Count - 1; i++) {
					// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
					// You can change them if they don't!
					Rectangle frame = new Rectangle(0, 0, 48, 28);
					Vector2 origin = new Vector2(24, 14);
					Vector2 scale = new Vector2(0.85f) * Projectile.scale;

					if (i == list.Count - 2) {
						frame.Y = 112;
					} else if (i > 10) {
						frame.Y = 84;
					} else if (i > 5) {
						frame.Y = 56;
					} else if (i > 0) {
						frame.Y = 28;
					}

					Vector2 element = list[i];
					Vector2 diff = list[i + 1] - element;

					float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
					Color color = Lighting.GetColor(element.ToTileCoordinates());

					Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

					pos += diff;
				}
				return false;
			}
		}
	}
}
