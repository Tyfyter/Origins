using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Origins.Buffs;
using PegasusLib;
using Origins.NPCs.Felnum;
using Origins.CrossMod;

namespace Origins.Items.Tools {
	public class Lightning_Rod : ModItem {
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 2f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ReinforcedFishingPole);
			Item.damage = 20;
			Item.DamageType = DamageClass.Generic;
			Item.noMelee = true;
			//Sets the poles fishing power
			Item.fishingPole = 37;
			//Wooden Fishing Pole is 9f and Golden Fishing Rod is 17f
			Item.shootSpeed = 13.7f;
			Item.shoot = ModContent.ProjectileType<Lightning_Rod_Bobber>();
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			player.itemLocation -= new Vector2(player.direction * 10, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			int xPositionAdditive = 58;
			float yPositionAdditive = 31f;
			Vector2 lineOrigin = player.MountedCenter;
			lineOrigin.Y += player.gfxOffY;
			//This variable is used to account for Gravitation Potions
			float gravity = player.gravDir;

			if (gravity == -1f) {
				lineOrigin.Y -= 12f;
			}
			lineOrigin.X += xPositionAdditive * player.direction;
			if (player.direction < 0) {
				lineOrigin.X -= 13f;
			}
			lineOrigin.Y -= yPositionAdditive * gravity;
			position = player.RotatedRelativePoint(lineOrigin + new Vector2(8f), true) - new Vector2(8f);
			velocity = (Main.MouseWorld - position).SafeNormalize(default) * velocity.Length();
		}
		public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor) {
			int xPositionAdditive = 58;
			float yPositionAdditive = 31f;
			lineOriginOffset.X += xPositionAdditive;
			lineOriginOffset.Y -= yPositionAdditive;
		}
	}
	public class Lightning_Rod_Bobber : ModProjectile {
		public AutoLoadingAsset<Texture2D> glowTexture = typeof(Lightning_Rod_Bobber).GetDefaultTMLName() + "_Glow";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BobberReinforced);
			Projectile.DamageType = DamageClass.Generic;
			DrawOriginOffsetY = -8;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			if (Projectile.ai[0] == 1) {
				Projectile.usesLocalNPCImmunity = false;
				Rectangle biggerHitbox = Projectile.Hitbox;
				biggerHitbox.Inflate(14, 14);
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (item.active && biggerHitbox.Intersects(item.Hitbox)) {
						item.velocity = Projectile.velocity;
					}
				}
				Projectile.extraUpdates = 1;
			} else if (Projectile.ai[2] == 0) {
				int count = 0;
				Vector2 oldPos = Projectile.position;
				while (WorldGen.InWorld((int)(Projectile.position.X / 16), (int)(Projectile.position.Y / 16))) {
					if (Collision.SlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, fall: true) != new Vector4(Projectile.position, Projectile.velocity.X, Projectile.velocity.Y)) {
						Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
						break;
					}
					if (Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, fallThrough: true, fall2: true) != Projectile.velocity) {
						Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
						break;
					}
					Projectile.position += Projectile.velocity;
					if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height)) {
						break;
					}
					if (++count > 100) break;
				}
				SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds), Projectile.Center);
				Vector2 diff = oldPos - Projectile.position;
				float distance = diff.Length();
				for (int i = 0; i < distance; i += Main.rand.Next(8, 12)) {
					Dust.NewDust(Projectile.Center + diff * (i / distance), 0, 0, DustID.Electric, 0f, 0f, 0, Color.White, 0.5f);
				}
				Projectile.ai[2] = 1;
				if (Main.myPlayer == Projectile.owner) {
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, diff, ModContent.ProjectileType<Lightning_Rod_Bobber_Zap>(), Projectile.damage, Projectile.knockBack);
				}
			}
			Lighting.AddLight(Projectile.Center, 0.194f, 0.441f, 0.474f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[0] == 1) {
				target.DoCustomKnockback(Vector2.Lerp(target.velocity, Projectile.velocity, target.knockBackResist));
			}
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(90, 120));
		}
		public override bool PreDrawExtras() {
			//Change these two values in order to change the origin of where the line is being drawn
			int xPositionAdditive = 58;
			float yPositionAdditive = 31f;

			Player player = Main.player[Projectile.owner];
			if (!Projectile.bobber || player.inventory[player.selectedItem].holdStyle <= 0)
				return false;

			Vector2 lineOrigin = player.MountedCenter;
			lineOrigin.Y += player.gfxOffY;
			//This variable is used to account for Gravitation Potions
			float gravity = player.gravDir;

			if (gravity == -1f) {
				lineOrigin.Y -= 12f;
			}
			int type = player.inventory[player.selectedItem].type;
			if (type == ModContent.ItemType<Lightning_Rod>()) {
				lineOrigin.X += xPositionAdditive * player.direction;
				if (player.direction < 0) {
					lineOrigin.X -= 13f;
				}
				lineOrigin.Y -= yPositionAdditive * gravity;
			}
			lineOrigin = player.RotatedRelativePoint(lineOrigin + new Vector2(8f), true) - new Vector2(8f);
			Main.spriteBatch.DrawLightningArcBetween(lineOrigin - Main.screenPosition, Projectile.Center - Main.screenPosition + Vector2.UnitX * 4, Main.rand.NextFloat(-4, 4));
			return false;
		}
		public override void PostDraw(Color lightColor) {
			float halfAvgWidth = (glowTexture.Value.Width - Projectile.width) * 0.5f + Projectile.width * 0.5f;
			Main.EntitySpriteDraw(
				glowTexture,
				Projectile.position + new Vector2(halfAvgWidth + DrawOffsetX, (Projectile.height / 2) + Projectile.gfxOffY) - Main.screenPosition,
				null,
				new Color(250, 250, 250, Projectile.alpha),
				Projectile.rotation,
				new Vector2(halfAvgWidth, Projectile.height / 2  - DrawOriginOffsetY),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
		}
	}
	public class Lightning_Rod_Bobber_Zap : ModProjectile {
		public override string Texture => typeof(Felnum_Ore_Slime_Zap).GetDefaultTMLName() + "_Placeholder";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.timeLeft = 9;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity)) {
				return true;
			}
			return null;
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
			miscShaderData.UseSaturation(-1f);
			miscShaderData.UseOpacity(4);
			miscShaderData.Apply();
			int maxLength = (int)(Projectile.velocity.Length() / 2);
			float[] oldRot = new float[maxLength];
			Vector2[] oldPos = new Vector2[maxLength];
			Vector2 start = Projectile.Center, end = Projectile.Center + Projectile.velocity;
			float rotation = (start - end).ToRotation();
			for (int i = 0; i < maxLength; i++) {
				oldPos[i] = Vector2.Lerp(start, end, i / (float)maxLength);
				oldRot[i] = rotation;
			}
			_vertexStrip.PrepareStrip(oldPos, oldRot, _ => new Color(80, 204, 219, 0), _ => 12, -Main.screenPosition, maxLength, includeBacksides: false);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: (9 - Projectile.timeLeft) / 3);
			Vector2 direction = Projectile.velocity.SafeNormalize(default);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition - direction * 28,
				frame,
				new Color(255, 255, 255, 0),
				Projectile.velocity.ToRotation() + MathHelper.PiOver2,
				new Vector2(42, 72),
				1,
				SpriteEffects.None
			);
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(180, 240));
		}
	}
}