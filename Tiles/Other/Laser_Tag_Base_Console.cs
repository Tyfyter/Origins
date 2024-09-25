using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Projectiles;
using Origins.UI;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;
using Terraria.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Origins.Tiles.Other {
	public class Laser_Tag_Base_Console : ModTile, IGlowingModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(81, 81, 81), CreateMapEntryName());
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => CheckInteract(true, i, j);
		public override bool RightClick(int i, int j) => CheckInteract(false, i, j);
		public static bool CheckInteract(bool justCheck, int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameY != 0) tile = Framing.GetTileSafely(i, --j);
			if (!Laser_Tag_Console.LaserTagGameActive) {
				if (!justCheck) {
					tile.TileFrameX = (short)((tile.TileFrameX + 1) % 6);
					if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendTileSquare(-1, i, j, TileChangeType.None);
				}
				return true;
			}
			//if (Main.netMode == NetmodeID.SinglePlayer) return false;
			if (!OriginPlayer.LocalOriginPlayer.laserTagVest) return false;
			if (Main.LocalPlayer.team != tile.TileFrameX && Laser_Tag_Console.LaserTagRules.RespawnTime >= 0 && !OriginPlayer.LocalOriginPlayer.laserTagVestActive && OriginPlayer.LocalOriginPlayer.laserTagRespawnDelay <= 0) {
				if (!justCheck) {
					OriginPlayer.LocalOriginPlayer.laserTagHP = Laser_Tag_Console.LaserTagRules.HP;
					OriginPlayer.LocalOriginPlayer.laserTagVestActive = true;
					if (Main.netMode != NetmodeID.SinglePlayer) {
						ModPacket packet = Origins.instance.GetPacket();
						packet.Write(Origins.NetMessageType.laser_tag_respawn);
						packet.Write(Main.myPlayer);
						packet.Send(-1, Main.myPlayer);
					}
				}
			}
			if (Laser_Tag_Console.LaserTagRules.CTG) {
				if (Main.LocalPlayer.team == tile.TileFrameX) {
					if (Main.LocalPlayer.ownedLargeGems != 0) {
						for (int team = 0; team < 6; team++) {
							if (justCheck) {
								if (Main.LocalPlayer.HasItem(Laser_Tag_Console.GetLargeGem(team))) return true;
							} else {
								if (Main.LocalPlayer.ConsumeItem(Laser_Tag_Console.GetLargeGem(team))) {
									//Main.NewText(team != Main.LocalPlayer.team);
									Laser_Tag_Console.ScorePoint(Main.LocalPlayer, false);
								}
							}
						}
					}
				} else {
					if (Laser_Tag_Console.LaserTagTeamGems[tile.TileFrameX] == -1) {
						if (!justCheck) Main.LocalPlayer.QuickSpawnItem(Entity.GetSource_None(), Laser_Tag_Console.GetLargeGem(tile.TileFrameX));
						return true;
					}
				}
			}
			return false;
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			frameXOffset = -Framing.GetTileSafely(i, j).TileFrameX;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = Main.teamColor[drawData.tileFrameX];
			drawData.glowTexture = glowTexture;
			drawData.glowSourceRect = new(0, drawData.tileFrameY, 16, 16);
			drawData.addFrX = -drawData.tileFrameX;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameY == 0 && Laser_Tag_Console.LaserTagTeamGems[tile.TileFrameX] == -1) {//Laser_Tag_Console.LaserTagRules.CTG && 
				Vector2 offset = new(Main.offScreenRange, Main.offScreenRange);
				if (Main.drawToScreen) {
					offset = Vector2.Zero;
				}
				int gemID = Laser_Tag_Console.GetLargeGemID(tile.TileFrameX);
				Vector2 position = new Vector2(i * 16 + 8, j * 16 - 32) + offset - Main.screenPosition;
				Lighting.AddLight(position, Main.teamColor[tile.TileFrameX].ToVector3());
				spriteBatch.Draw(
					TextureAssets.Gem[gemID].Value,
					position,
					null,
					new Color(250, 250, 250, Main.mouseTextColor / 2),
					0,
					TextureAssets.Gem[gemID].Size() / 2f,
					(Main.mouseTextColor / 1000f + 0.8f) * 1,
					0,
				0);
			}
		}
		readonly AutoLoadingAsset<Texture2D> glowTexture = typeof(Laser_Tag_Base_Console).GetDefaultTMLName() + "_Glow";
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public Color GlowColor => new(196, 196, 196, 100);
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, Main.teamColor[tile.TileFrameX].ToVector3());
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Laser_Tag_Base_Console_Item : Laser_Tag_Console_Item {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Laser_Tag_Base_Console>());
		}
	}
}
