using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PegasusLib;
using PegasusLib.Graphics;

namespace Origins.Items.Accessories {
	public class Scribe_of_the_Meat_God : ModItem, ICustomWikiStat {
		public string[] Categories => [
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.damage = 65;
			Item.knockBack = 3;
			Item.shoot = Scribe_of_the_Meat_God_P.ID;
			Item.value = Item.sellPrice(gold: 6);
			Item.master = true;
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().meatScribeItem = Item;
			if (player.ownedProjectileCounts[Item.shoot] > 0) {
				player.GetModPlayer<OriginPlayer>().disableUseItem = true;
			}
		}
	}
	public class Scribe_of_the_Meat_God_P : ModProjectile {
		public const int max_updates = 4;
		public const int dash_duration = 30;
		public override string Texture => "Origins/Items/Accessories/Scribe_of_the_Meat_God_Use";
		public static AutoCastingAsset<Texture2D> Texture2 { get; private set; }
		public static AutoCastingAsset<Texture2D> Texture3 { get; private set; }
		public static AutoCastingAsset<Texture2D> Texture4 { get; private set; }
		public static AutoCastingAsset<Texture2D> Texture5 { get; private set; }
		public static int ID { get; private set; }
		Vector2[] hungries;
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				Texture2 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_of_the_Meat_God_Use2");
				Texture3 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_of_the_Meat_God_Use3");
				Texture4 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_of_the_Meat_God_Use4");
				Texture5 = Mod.Assets.Request<Texture2D>("Items/Accessories/Scribe_of_the_Meat_God_Use5");
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
			Projectile.MaxUpdates = max_updates;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = dash_duration * max_updates;
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
			if (Projectile.numUpdates == 0) {
				owner.mount.Dismount(owner);
				owner.GetModPlayer<OriginPlayer>().hideAllLayers = true;
				owner.endurance += (1 - owner.endurance) * 0.5f;
				owner.statDefense += 8;
			}
			if (++Projectile.frameCounter >= 14) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
			}
		}
		public override void OnKill(int timeLeft) {
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
			float yOffsetMult = 1;
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
									yOffset += (yFactor - xFactor) * 16 * yOffsetMult;
									//tile.TileColor = PaintID.DeepRedPaint;
								}
							} else if (!Main.tileSolidTop[tile.TileType]) {
								colliding = true;
								yOffsetMult = 0;
								//tile.TileColor = PaintID.DeepOrangePaint;
							}
						} else if (j == (int)maxY) {
							if (tile.Slope == (direction == 1 ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft)) {
								float yFactor = (1 - maxY % 1);
								float xFactor = (direction == 1 ? (x % 1) : (1 - x % 1));
								if (yFactor > xFactor) {
									colliding = true;
									yOffset -= (yFactor - xFactor) * 16 * yOffsetMult;
									//tile.TileColor = PaintID.DeepYellowPaint;
								}
							} else if(tile.IsHalfBlock) {
								colliding = true;
								yOffset -= 8 * yOffsetMult;
							} else if (!Main.tileSolidTop[tile.TileType]) {
								colliding = true;
								yOffsetMult = 0;
								//tile.TileColor = PaintID.DeepGreenPaint;
							}
						} else {
							if (tile.Slope == (direction == 1 ? SlopeType.SlopeUpRight : SlopeType.SlopeUpLeft)) {
								float xFactor = (direction == 1 ? (x % 1) : (1 - x % 1));
								colliding = true;
								yOffset += xFactor * 16 * yOffsetMult;
								//tile.TileColor = PaintID.DeepBluePaint;
							} else if (tile.Slope == (direction == 1 ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft)) {
								float xFactor = (direction == 1 ? (x % 1) : (1 - x % 1));
								colliding = true;
								yOffset -= xFactor * 16 * yOffsetMult;
								//tile.TileColor = PaintID.DeepPurplePaint;
							} else if (tile.IsHalfBlock && j + 1 == (int)maxY) {
								colliding = true;
								yOffset -= 8 * yOffsetMult;
							} else if (!Main.tileSolidTop[tile.TileType]) {
								colliding = true;
								yOffsetMult = 0;
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
			SpriteBatchState state = Main.spriteBatch.GetState();
			Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 dirVect = new(Math.Sign(Projectile.velocity.X), 1);
			int Jankify(int a, int b, float c) {
				return (int)((Projectile.timeLeft % a - Projectile.timeLeft % b) / c);
			}
			if (hungries is null) {
				hungries = new Vector2[Main.rand.Next(2, 4)];
				for (int i = 0; i < hungries.Length; i++) {
					hungries[i] = new Vector2(Main.rand.Next(28, 40), ((56 / hungries.Length) * i - 14) + Main.rand.Next(-3, 4) * 2);
				}
			}
			List<(Texture2D texture, Rectangle? frame, Vector2 position, float rotation, Vector2? origin)> textures = new() {
				(projTexture, projTexture.Frame(verticalFrames: 2, frameY: Projectile.frame % 2), new Vector2(0, 0), 0, null),
				(Texture2, null, new Vector2(13, -17 + Jankify(48, 24, 24 / 2) - 2), 0, null),
				(Texture2, null, new Vector2(11, 17 + Jankify(54, 27, 27 / 2) - 2), 0, null),
				(Texture3,
					Texture3.Value.Frame(verticalFrames: 2, frameY: (Projectile.frame / 2) % 2),
					new Vector2(Jankify(80, 30, 15) + 15, Jankify(60, 30, 15) - 2),
					0,
					null
				),
			};
			for (int i = 0; i < hungries.Length; i++) {
				Vector2 hungryPos = hungries[i] + new Vector2(0, Jankify(18 + i, 9 + i, 4.5f + i));
				Vector2 basePos = new(8, hungries[i].Y);
				textures.Insert(0, (Texture4,
					new Rectangle(0, 0, (int)hungryPos.X - 8, 6),
					basePos,
					((hungryPos - basePos) * dirVect).ToRotation(),
					new Vector2(0, 3)
				));
				textures.Add((Texture5,
					Texture5.Value.Frame(verticalFrames: 2, frameY: (Projectile.frame + ((Projectile.frameCounter + i * 5) / 14)) % 2),
					hungryPos,
					0,
					null
				));
			}
			Main.spriteBatch.Restart(state, samplerState:SamplerState.PointWrap);
			for (int i = 0; i < textures.Count; i++) {
				(Texture2D texture, Rectangle? frame, Vector2 position, float rotation, Vector2? origin) = textures[i];
				Main.EntitySpriteDraw(
					texture,
					Projectile.Center + (position * dirVect) - Main.screenPosition,
					frame,
					lightColor,
					Projectile.rotation + rotation,
					origin ?? ((frame?.Size() ?? texture.Size()) * 0.5f),
					Projectile.scale,
					Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			}
			Main.spriteBatch.Restart(state);
			return false;
		}
	}
}
