using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Petrified_Tree_Leaf1 : ModDust {
		public static List<int> dustIDs = [];
		public override void SetStaticDefaults() {
			dustIDs.Add(Type);
		}
		public override void OnSpawn(Dust dust) {
			dust.fadeIn = Gore.goreTime;
		}
		public override bool Update(Dust dust) {
			Vector2 vector = dust.position + new Vector2(12f) / 2f - new Vector2(4f) / 2f;
			vector.Y -= 4f;
			Vector2 vector2 = dust.position - vector;
			if (dust.velocity.Y < 0f) {
				Vector2 newXVelocity = new(dust.velocity.X, -0.2f);
				int size = (int)(4 * 0.9f);
				Point tilePos = (new Vector2(size) / 2f + vector).ToTileCoordinates();
				if (!WorldGen.InWorld(tilePos.X, tilePos.Y)) {
					dust.active = false;
					return false;
				}
				Tile tile = Main.tile[tilePos.X, tilePos.Y];
				if (tile == null) {
					dust.active = false;
					return false;
				}
				Rectangle tileRect = new(tilePos.X * 16, tilePos.Y * 16 + tile.LiquidAmount / 16, 16, 16 - tile.LiquidAmount / 16);
				Rectangle dustRect = new((int)vector.X, (int)vector.Y + 6, size, size);
				bool flag = tile != null && tile.LiquidAmount > 0 && tileRect.Intersects(dustRect);
				if (dust.velocity.Y > -1 && !WorldGen.SolidTile(tilePos.X, tilePos.Y + 1) && !flag) {
					dust.velocity.Y = 0.1f;
					dust.fadeIn = 0;
					dust.alpha += 20;
				}
				if (flag) {
					switch (tile.LiquidType) {
						case LiquidID.Honey:
						newXVelocity.X = Main.WindForVisuals;
						break;
						case LiquidID.Lava:
						newXVelocity.X = 0;
						break;
						case LiquidID.Water:
						dust.active = false;
						for (int i = 0; i < 5; i++) {
							Dust.NewDust(dust.position, size, size, DustID.Smoke, 0f, -0.2f);
						}
						break;
						default:
						dust.velocity.Y -= 2f;
						break;
					}
				}
				newXVelocity = Collision.TileCollision(vector, newXVelocity, size, size);
				if (flag) {
					dust.rotation = newXVelocity.ToRotation() + MathHelper.PiOver2;
				}
				newXVelocity.X *= 0.94f;
				if (!flag || (newXVelocity.X > -0.01f && newXVelocity.X < 0.01f)) {
					newXVelocity.X = 0f;
				}
				if (dust.fadeIn > 0) {
					dust.fadeIn -= 1;
				} else {
					dust.alpha += 1;
				}
				dust.velocity.X = newXVelocity.X;
				dust.position += dust.velocity;
			} else {
				dust.velocity.Y += MathHelper.Pi / 180f;
				Vector2 velocity = new(Vector2.UnitY.RotatedBy(dust.velocity.Y).X * 1f, Math.Abs(Vector2.UnitY.RotatedBy(dust.velocity.Y).Y) * 1f);
				int size = 4;
				if (dust.position.Y < Main.worldSurface * 16.0) {
					velocity.X += Main.WindForVisuals * 4f;
				}
				Vector2 oldVelocity = velocity;
				velocity = Collision.TileCollision(vector, velocity, size, size);
				(dust.position, velocity) = Collision.SlopeCollision(vector, velocity, size, size, 1f);
				dust.position += vector2;
				if (velocity != oldVelocity) {
					dust.velocity.Y = -1f;
				}
				Point tilePos = (new Vector2(6, 6) * 0.5f + dust.position).ToTileCoordinates();
				if (!WorldGen.InWorld(tilePos.X, tilePos.Y)) {
					dust.active = false;
					return false;
				}
				Tile tile = Main.tile[tilePos.X, tilePos.Y];
				if (tile == null) {
					dust.active = false;
					return false;
				}
				Rectangle tileRect = new(tilePos.X * 16, tilePos.Y * 16 + tile.LiquidAmount / 16, 16, 16 - tile.LiquidAmount / 16);
				Rectangle dustRect = new((int)vector.X, (int)vector.Y + 6, size, size);
				if (tile != null && tile.LiquidAmount > 0 && tileRect.Intersects(dustRect)) {
					switch (tile.LiquidType) {
						case LiquidID.Honey:
						dust.velocity.X = Main.WindForVisuals;
						dust.velocity.Y = -1f;
						break;
						case LiquidID.Lava:
						dust.velocity.Y = -1f;
						break;
						case LiquidID.Water:
						int dustCount = 0;
						for (int i = 0; i < 5; i++) {
							if (Dust.NewDust(dust.position, 6, 6, DustID.Smoke, 0f, -0.2f) != 6000) dustCount++;
						}
						if (dustCount >= 5) dust.active = false;
						break;
						default:
						dust.velocity.Y -= 2f;
						break;
					}
				}
				dust.position += velocity;
				dust.rotation = velocity.ToRotation() + (float)Math.PI / 2f;
				if (dust.fadeIn > 0) {
					dust.fadeIn -= 1;
				} else {
					dust.alpha += 1;
				}
			}
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return false;
		}
		public override bool PreDraw(Dust dust) {
			TangelaVisual.DrawTangela(
				Texture2D.Value,
				dust.position - Main.screenPosition,
				dust.frame,
				dust.rotation,
				new Vector2(4f, 4f),
				Vector2.One * dust.scale,
				SpriteEffects.None,
				dust.dustIndex
			);
			return false;
		}
	}
	public class Petrified_Tree_Leaf2 : Petrified_Tree_Leaf1 { }
	public class Petrified_Tree_Leaf3 : Petrified_Tree_Leaf1 { }
}