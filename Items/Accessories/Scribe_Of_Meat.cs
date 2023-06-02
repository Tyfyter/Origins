using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Scribe_Of_Meat : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Scribe of the Meat God");
			Tooltip.SetDefault("Transform into a miniature wall of flesh during a dash\n'You're made of meat'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 6);
			Item.master = true;
			Item.rare = ItemRarityID.Master;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().meatScribe = true;
		}
	}
	public class Scribe_Of_Meat_P : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Scribe_Of_Meat_Use";
		public static AutoCastingAsset<Texture2D> Texture2 { get; private set; }
		public static AutoCastingAsset<Texture2D> Texture3 { get; private set; }
		public static AutoCastingAsset<Texture2D> Texture4 { get; private set; }
		public static AutoCastingAsset<Texture2D> Texture5 { get; private set; }
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Scribe of the Meat God");
			if (!Main.dedServ) {
				Texture2 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_Of_Meat_Use2");
				Texture3 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_Of_Meat_Use3");
				Texture4 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_Of_Meat_Use4");
				Texture5 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_Of_Meat_Use5");
			}
			Main.projFrames[Type] = 2;
			ID = Type;
		}
		public override void Unload() {
			Texture2 = null;
			Texture3 = null;
			Texture4 = null;
			Texture5 = null;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
			Projectile.width = 42;
			Projectile.height = 58;
			Projectile.damage = 137;
			Projectile.DamageType = DamageClass.Generic;
			Projectile.alpha = 0;
			Projectile.extraUpdates = 1;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 40;
			Projectile.penetrate = -1;
			Projectile.knockBack = 14;
			Projectile.tileCollide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			owner.Center = Projectile.Center;
			owner.velocity = Projectile.velocity;
			owner.mount.Dismount(owner);
			owner.GetModPlayer<OriginPlayer>().hideAllLayers = true;
			owner.endurance += (1 - owner.endurance) * 0.5f;
			if (++Projectile.frameCounter >= 7) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= 2) {
					Projectile.frame = 0;
				}
			}
		}
		public override void Kill(int timeLeft) {
			// dusts and/or gores here
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			//hitbox.Inflate(4, 12);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			height = 42;
			int direction = Math.Sign(Projectile.velocity.X);
			float x = (Projectile.Center.X + width * 0.5f * direction) / 16;
			int i0 = (int)x;
			float minY = ((Projectile.Center.Y - height * 0.5f) / 16);
			float maxY = ((Projectile.Center.Y + height * 0.5f) / 16);
			Tile tile;
			bool colliding = false;
			float yOffset = 0;
			for (int i1 = 0; i1 < 1; i1++) {
				int i = i0 + i1 * direction;
				for (int j = (int)minY; j < (int)maxY; j++) {
					tile = Framing.GetTileSafely(i, j);
					if (tile.HasSolidTile()) {
						if (j == (int)minY) {
							if (tile.Slope == (direction == 1 ? SlopeType.SlopeUpRight : SlopeType.SlopeUpLeft)) {
								float yFactor = (minY % 1);
								float xFactor = (direction == 1 ? (x % 1) : (1 - x % 1));
								if (yFactor > xFactor) {
									colliding = true;
									yOffset += (yFactor - xFactor) * 16;
									//tile.TileColor = PaintID.DeepRedPaint;
								}
							} else {
								colliding = true;
								//tile.TileColor = PaintID.DeepOrangePaint;
							}
						} else if (j == (int)maxY) {
							if (tile.Slope == (direction == 1 ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft)) {
								float yFactor = (1 - maxY % 1);
								float xFactor = (direction == 1 ? (x % 1) : (1 - x % 1));
								if (yFactor > xFactor) {
									colliding = true;
									yOffset -= (yFactor - xFactor) * 16;
									//tile.TileColor = PaintID.DeepYellowPaint;
								}
							} else if(tile.IsHalfBlock) {
								colliding = true;
								yOffset -= 8;
							} else {
								colliding = true;
								//tile.TileColor = PaintID.DeepGreenPaint;
							}
						} else {
							if (tile.Slope == (direction == 1 ? SlopeType.SlopeUpRight : SlopeType.SlopeUpLeft)) {
								float xFactor = (direction == 1 ? (x % 1) : (1 - x % 1));
								colliding = true;
								yOffset += xFactor * 16;
								//tile.TileColor = PaintID.DeepBluePaint;
							} else if (tile.Slope == (direction == 1 ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft)) {
								float xFactor = (direction == 1 ? (x % 1) : (1 - x % 1));
								colliding = true;
								yOffset -= xFactor * 16;
								//tile.TileColor = PaintID.DeepPurplePaint;
							} else if (tile.IsHalfBlock && j + 1 == (int)maxY) {
								colliding = true;
								yOffset -= 8;
							} else {
								colliding = true;
								//tile.TileColor = PaintID.DeepVioletPaint;
							}
						}
					}
				}
			}
			if (colliding) {
				if (yOffset != 0) {
					Projectile.position.Y += yOffset;
				} else {
					Projectile.Kill();
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;
			(Texture2D, Rectangle?, Vector2)[] textures = new (Texture2D, Rectangle?, Vector2)[] {
				(projTexture, projTexture.Frame(verticalFrames: 2, frameY: Projectile.frame), new Vector2(0, 0)),
				(Texture2, null, new Vector2(14, -16)),
				(Texture2, null, new Vector2(12, 16)),
			};
			for (int i = 0; i < textures.Length; i++) {
				(Texture2D texture, Rectangle? frame, Vector2 position) = textures[i];
				Main.EntitySpriteDraw(
					texture,
					Projectile.Center + (position * new Vector2(Math.Sign(Projectile.velocity.X), 1)) - Main.screenPosition,
					frame,
					lightColor,
					Projectile.rotation,
					(frame?.Size() ?? texture.Size()) * 0.5f,
					Projectile.scale,
					Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			}
			return false;
		}
	}
}
