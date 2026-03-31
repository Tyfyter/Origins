using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.Hooks;
using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using ModLiquidLib.Utils.Structs;
using Origins.Buffs;
using Origins.Liquids.Waterfalls;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;
using static ModLiquidLib.ModLiquidLib;

namespace Origins.Liquids {
	public class Brine : ModLiquid {
		public static int ID { get; private set; }
		public override void Load() {
			MonoModHooks.Add(typeof(LiquidLoader).GetMethod(nameof(LiquidLoader.LiquidMergeTilesType)),
				(orig_LiquidMergeTilesType orig, int i, int j, int type, int otherLiquid) => {
					TreatAsWater(ref type, ref otherLiquid);
					return orig(i, j, type, otherLiquid);
				});
			MonoModHooks.Add(typeof(LiquidLoader).GetMethod(nameof(LiquidLoader.LiquidMergeSounds)),
				(orig_LiquidMergeSounds orig, int i, int j, int type, int otherLiquid, ref SoundStyle? collisionSound) => {
					TreatAsWater(ref type, ref otherLiquid);
					orig(i, j, type, otherLiquid, ref collisionSound);
				});
			MonoModHooks.Add(typeof(LiquidLoader).GetMethod(nameof(LiquidLoader.PreLiquidMerge)),
				(orig_PreLiquidMerge orig, int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid) => {
					TreatAsWater(ref type, ref otherLiquid);
					return orig(liquidX, liquidY, tileX, tileY, type, otherLiquid);
				});
		}
		static void TreatAsWater(ref int type, ref int otherLiquid) {
			if (type == LiquidID.Water || otherLiquid == LiquidID.Water) return;
			if (otherLiquid == ID) otherLiquid = LiquidID.Water;
			if (type == ID) type = LiquidID.Water;
		}
		public override void SetStaticDefaults() {
			Amebic_Gel.ConvertToAmebicGel[Type] = true;
			LiquidID_TLmod.Sets.UsesWaterFishingLootPool[Type] = true;
			SplashDustType = DustID.Water_Jungle;
			UsesLavaCollisionForWet = true;
			PlayerMovementMultiplier = 0.5f;
			StopWatchMPHMultiplier = PlayerMovementMultiplier;
			NPCMovementMultiplierDefault = PlayerMovementMultiplier;
			ProjectileMovementMultiplier = PlayerMovementMultiplier;
			AddMapEntry(FromHexRGB(0x00583F));
			ID = Type;
		}
		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<Brine_Fall>().Slot;
		}
		public override LightMaskMode LiquidLightMaskMode(int i, int j) {
			return LightMaskMode.Water;
		}
		public override bool EvaporatesInHell(int i, int j) => true;
		public override void RetroDrawEffects(int i, int j, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality) {
			/*drawData.liquidAlphaMultiplier *= 1.8f;
			if (drawData.liquidAlphaMultiplier > 1f) {
				drawData.liquidAlphaMultiplier = 1f;
			}*/
		}
		public override void OnNPCCollision(NPC npc) {
			if (!npc.dontTakeDamage && !NetmodeActive.MultiplayerClient) npc.AddBuff(Toxic_Shock_Debuff.ID, 300);
		}
		public override void OnPlayerCollision(Player player) {
			player.AddBuff(Toxic_Shock_Debuff.ID, 300);
		}
		public override int LiquidMerge(int i, int j, int otherLiquid) {
			switch (otherLiquid) {
				case LiquidID.Lava: return TileID.Obsidian;
				case LiquidID.Honey: return TileID.HoneyBlock;
				case LiquidID.Shimmer: return TileID.ShimmerBlock;
			}
			return TileID.Stone;
		}
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			if (otherLiquid == LiquidID.Water) {
				// convert water to brine?
				UpdateLiquids(liquidX, liquidY, tileX, tileY, otherLiquid);
				return false;
			}
			return true;
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
				if (tile2.LiquidType == LiquidID.Water) AssimilateLiquid(tile2.X(), tile2.Y());
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
		public override void LiquidMergeSound(int i, int j, int otherLiquid, ref SoundStyle? collisionSound) {
			if (otherLiquid == LiquidID.Water) collisionSound = null;
			else LiquidLoader.LiquidMergeSounds(i, j, LiquidID.Water, otherLiquid, ref collisionSound);
		}
		public override bool PlayerLiquidMovement(Player player, bool fallThrough, bool ignorePlats) {
			if (player.merman || player.ignoreWater || player.trident) {
				player.DryCollision(fallThrough, ignorePlats);
				Vector2 oldVelocity = player.velocity;
				if (player.mount.Active && player.velocity.Y != 0f) {
					if (player.mount.IsConsideredASlimeMount && !player.SlimeDontHyperJump) {
						player.velocity.X = 0f;
						player.DryCollision(fallThrough, ignorePlats);
						player.velocity.X = oldVelocity.X;
					} else if (player.mount.Type == MountID.PogoStick) {
						player.velocity.X = 0f;
						player.DryCollision(fallThrough, ignorePlats);
						player.velocity.X = oldVelocity.X;
					}
				}
				return false;
			}
			return LiquidLoader.PlayerLiquidMovement(LiquidID.Water, player, fallThrough, ignorePlats);
		}
		delegate int? hook_LiquidMergeTilesType(orig_LiquidMergeTilesType orig, int i, int j, int type, int otherLiquid);
		delegate int? orig_LiquidMergeTilesType(int i, int j, int type, int otherLiquid);
		delegate void hook_LiquidMergeSounds(orig_LiquidMergeTilesType orig, int i, int j, int type, int otherLiquid, ref SoundStyle? collisionSound);
		delegate void orig_LiquidMergeSounds(int i, int j, int type, int otherLiquid, ref SoundStyle? collisionSound);
		delegate bool hook_PreLiquidMerge(orig_LiquidMergeTilesType orig, int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid);
		delegate bool orig_PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid);
	}
}
