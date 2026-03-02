using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.Hooks;
using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using ModLiquidLib.Utils.Structs;
using Origins.Buffs;
using Origins.Core;
using Origins.Dusts;
using Origins.Liquids.Waterfalls;
using Origins.NPCs;
using Origins.NPCs.Riven;
using Origins.Tiles;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;
using static ModLiquidLib.ModLiquidLib;

namespace Origins.Liquids {
	[ReinitializeDuringResizeArrays]
	public class Amebic_Gel : ModLiquid {
		public static bool[] DoesntDissolveByAmebicGel = LiquidID_TLmod.Sets.Factory.CreateNamedSet(nameof(DoesntDissolveByAmebicGel))
			.Description("Liquids in this set wont get dissolved by or turn into Amebic Gel")
			.RegisterBoolSet(LiquidID.Shimmer);
		public static bool[] ConvertToAmebicGel = LiquidID_TLmod.Sets.Factory.CreateNamedSet(nameof(ConvertToAmebicGel))
			.Description("Liquids in this set will turn into Amebic Gel when merging")
			.RegisterBoolSet(LiquidID.Water);
		public static int ID { get; private set; }
		public static float GlowValue => Riven_Hive.NormalGlowValue.GetValue() * 0.5f + 0.25f;
		public override void SetStaticDefaults() {
			SplashDustType = Gooey_Water_Dust.ID;
			FishingPoolSizeMultiplier = 2f;
			UsesLavaCollisionForWet = true;
			ExtinguishesOnFireDebuffs = true;
			PlayerMovementMultiplier = 0.375f;
			StopWatchMPHMultiplier = PlayerMovementMultiplier;
			NPCMovementMultiplierDefault = PlayerMovementMultiplier;
			ProjectileMovementMultiplier = PlayerMovementMultiplier;
			AddMapEntry(FromHexRGB(0x00C4A7));
			ID = Type;
		}
		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<Amebic_Gel_Fall>().Slot;
		}
		public override LightMaskMode LiquidLightMaskMode(int i, int j) {
			return LightMaskMode.None;
		}
		public override bool EvaporatesInHell(int i, int j) => true;
		public override void RetroDrawEffects(int i, int j, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality) {
			/*drawData.liquidAlphaMultiplier *= 1.8f;
			if (drawData.liquidAlphaMultiplier > 1f) {
				drawData.liquidAlphaMultiplier = 1f;
			}*/
		}
		public override void ModifyLightMaskMode(int index, ref float r, ref float g, ref float b) {
			float glowValue = GlowValue;
			r = 0.1f * glowValue;
			g = 1.05f * glowValue;
			b = 1f * glowValue;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			byte liquidAmount = Main.tile[i, j].LiquidAmount;
			float mult = (liquidAmount > 200 ? 1 : liquidAmount / 200) * Riven_Hive.NormalGlowValue.GetValue();

			g += 0.9f * mult;
			b += 1f * mult;
		}
		public override void ModifyNearbyTiles(int i, int j, int liquidX, int liquidY) {
			Tile tile = Framing.GetTileSafely(i, j);
			if ((TileID.Sets.Grass[tile.TileType] || TileID.Sets.GrassSpecial[tile.TileType]) && ModContent.GetModTile(tile.TileType) is not IRivenTile) {
				using WorldGenOverride _ = new();
				WorldGen.KillTile(i, j, true);

				if (NetmodeActive.Server) NetMessage.SendTileSquare(-1, i, j, 1);
			}
		}
		public override void OnNPCCollision(NPC npc) {
			if (!npc.dontTakeDamage && !NetmodeActive.MultiplayerClient) {
				OriginGlobalNPC.InflictTorn(npc, 188, 750, 1f);
			}
		}
		public override void OnPlayerCollision(Player player) {
			OriginPlayer oP = player.OriginPlayer();
			OriginPlayer.InflictTorn(player, 188, 750, 1f, true);
			oP.GetAssimilation<Riven_Assimilation>().Percent += 0.001f;
		}
		public override void LiquidMergeSound(int i, int j, int otherLiquid, ref SoundStyle? collisionSound) {
			switch (otherLiquid) {
				case LiquidID.Shimmer:
				collisionSound = Main.rand.NextBool() ? SoundID.Shimmer1 : SoundID.Shimmer2;
				break;
				default:
				collisionSound = SoundID.LiquidsWaterLava;
				break;
			}
		}
		public override int LiquidMerge(int i, int j, int otherLiquid) {
			return ModContent.TileType<Amoeba_Fluid>();
		}
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			switch (otherLiquid) {
				case LiquidID.Shimmer:
				Tile tile = Main.tile[tileX, tileY];
				Tile liquid = Main.tile[liquidX, liquidY];
				if (tile.LiquidType == ID) Utils.Swap(ref tile, ref liquid);
				byte amt = 10;
				if (liquid.LiquidAmount >= amt) {
					tile.LiquidAmount = (byte)Math.Max(0, tile.LiquidAmount - 25);
					liquid.LiquidAmount = (byte)Math.Max(0, liquid.LiquidAmount - amt);
					if (!NetmodeActive.MultiplayerClient) {
						Vector2 pos = tile.GetTilePosition().ToWorldCoordinates();
						NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (int)pos.X, (int)pos.Y, ModContent.NPCType<Amebic_Slime>());
					}
				}
				return false;

				default:
				if (!DoesntDissolveByAmebicGel[otherLiquid]) {
					UpdateLiquids(liquidX, liquidY, tileX, tileY, otherLiquid);
					return false;
				}
				return true;
			}
		}
		public static void UpdateLiquids(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			//tile variables, these help us edit the liquid at certain tile positions
			Tile leftTile = Main.tile[tileX - 1, tileY];
			Tile rightTile = Main.tile[tileX + 1, tileY];
			Tile upTile = Main.tile[tileX, tileY - 1];
			Tile tile = Main.tile[tileX, tileY];
			Tile liquidTile = Main.tile[liquidX, liquidY];

			void ReduceLiquid(Tile tile1, Tile tile2) {
				if (tile2.LiquidType == ID) Utils.Swap(ref tile1, ref tile2);
				if (ConvertToAmebicGel[tile2.LiquidType]) {
					AssimilateLiquid(tile2.X(), tile2.Y());
					return;
				}
				byte reductionDivisor = 1;
				tile1.LiquidAmount -= (byte)(Math.Min(leftTile.LiquidAmount, liquidTile.LiquidAmount) / reductionDivisor);
				tile2.LiquidAmount = 0;
			}

			//Checks the type of merging
			//
			//For more context:
			//Liquid to Liquid merging is split up into 2 types, 
			// * Top/Side merging
			// * Down merging
			//
			//Here we get which type the merging is based on the liquidY relitive to the tileY.
			//This is because liquidY and tileY are different in the down merging, but the same in the up/side merging
			if (liquidY == tileY) {
				//This is up/side merging for the liquid

				//Here we remove the liquid when merging
				//Majority of this code determines whether a liquid merge is spawned or not by checking the surrounding liquid amounts
				if (leftTile.LiquidType != ID) ReduceLiquid(liquidTile, leftTile);
				if (rightTile.LiquidType != ID) ReduceLiquid(liquidTile, rightTile);
				if (upTile.LiquidType != ID) ReduceLiquid(liquidTile, upTile);

				//check is the nearby amount is more than 24, and the other liquid is not this liquid
				if (otherLiquid == ID) {
					return;
				}

				// don't do anything to delete liquid
				// doNothing();

				//play the liquid merge sound
				if (!WorldGen.gen) {
					LiquidHooks.PlayLiquidChangeSound(tileX, tileY, ID, otherLiquid);
				}
				if (Main.netMode == NetmodeID.Server) {
					ModPacket packet = ModContent.GetInstance<ModLiquidLib.ModLiquidLib>().GetPacket();
					packet.Write((byte)MessageType.SyncCollisionSounds);
					packet.Write(tileX);
					packet.Write(tileY);
					packet.Write(ID);
					packet.Write(otherLiquid);
					packet.Send();
				}
				//frame the tile/update the tile/s nearby
				Oil.UpdateAdjacentLiquids(tileX, tileY);
				//sync changes in multiplayer
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendTileSquare(-1, tileX - 1, tileY - 1, 3);
				}
			} else {
				//This is down merging for the liquid

				//remove the liquid amount 
				ReduceLiquid(liquidTile, tile);
				tile.LiquidType = ID;
				//play liquid merge sound
				if (!WorldGen.gen) {
					LiquidHooks.PlayLiquidChangeSound(tileX, tileY, ID, otherLiquid);
				}
				if (Main.netMode == NetmodeID.Server) {
					ModPacket packet = ModContent.GetInstance<ModLiquidLib.ModLiquidLib>().GetPacket();
					packet.Write((byte)MessageType.SyncCollisionSounds);
					packet.Write(tileX);
					packet.Write(tileY);
					packet.Write(ID);
					packet.Write(otherLiquid);
					packet.Send();
				}
				//frame the tile/s around the tile
				Oil.UpdateAdjacentLiquids(tileX, tileY);
				//sync tile changes around
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendTileSquare(-1, tileX - 1, tileY, 3);
				}
			}
		}
		public static void AssimilateLiquid(int i, int j) {
			Main.tile[i, j].SetLiquidType(ID);
			Oil.UpdateAdjacentLiquids(i, j);
		}
	}
}
