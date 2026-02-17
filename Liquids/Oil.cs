using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils.Structs;
using Origins.Buffs;
using Origins.Dusts;
using Origins.Liquids.Waterfalls;
using Origins.Tiles.Ashen;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Liquids {
	public class Oil : ModLiquid {
		public static int CanBurnBeBlockedThreshold => 191;
		public override string Texture => base.Texture.Replace("Burning_", string.Empty);
		public override string BlockTexture => base.BlockTexture.Replace("Burning_", string.Empty);
		public override string SlopeTexture => base.SlopeTexture.Replace("Burning_", string.Empty);
		public static int ID { get; private set; }
		public virtual Color MapColor => FromHexRGB(0x0A0A0A);
		public override void SetStaticDefaults() {
			LiquidRenderer.VISCOSITY_MASK[Type] = 50;
			LiquidRenderer.WATERFALL_LENGTH[Type] = 10;
			LiquidRenderer.DEFAULT_OPACITY[Type] = 0.98f;
			LiquidRenderer.WATERFALL_LENGTH[Type] = 4;
			SlopeOpacity = 1f;
			LiquidfallOpacityMultiplier = 0.8f;
			WaterRippleMultiplier = 0.3f;
			SplashDustType = ModContent.DustType<White_Water_Dust>();
			SplashSound = SoundID.SplashWeak;
			FallDelay = 0;
			ChecksForDrowning = true;
			AllowEmitBreathBubbles = false;
			PlayerMovementMultiplier = 0.375f;
			StopWatchMPHMultiplier = PlayerMovementMultiplier;
			NPCMovementMultiplierDefault = PlayerMovementMultiplier;
			ProjectileMovementMultiplier = PlayerMovementMultiplier;
			ExtinguishesOnFireDebuffs = false;
			AddMapEntry(MapColor);
			if (GetType() == typeof(Oil)) ID = Type;
		}
		//ChooseWaterfallStyle allows for the selection of what waterfall style this liquid chooses when next to a slope.
		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<Oil_Fall>().Slot;
		}
		//LiquidLightMaskMode is how the game decides what lightMaskMode to use when this liquid is over a tile
		//We set this to none, this is due to the liquid emitting light, needing no special lightMaskMode for its interaction with light.
		public override LightMaskMode LiquidLightMaskMode(int i, int j) {
			return LightMaskMode.Honey;
		}
		//Using EvaporatesInHell, we are able to choose whether this liquid evaporates in hell, based on a condition.
		//For custom evaporation, use UpdateLiquid override.
		public override bool EvaporatesInHell(int i, int j) {
			if (j > Main.UnderworldLayer) {
				Main.tile[i, j].SetLiquidType(Burning_Oil.ID);
			}
			return false;
		}
		//Using RetroDrawEffects, we can do stuff only during the rendering of liquids in the retro lighting style.
		//Here we set the opacity we want during retro lighting so that its consistant with the opacity of the liquid when not in the retro lighting style
		//NOTE: Despite being having RETRO in the name, this also applies to the "Trippy" Lighting style as well.
		public override void RetroDrawEffects(int i, int j, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality) {
			drawData.liquidAlphaMultiplier *= 1.8f;
			if (drawData.liquidAlphaMultiplier > 1f) {
				drawData.liquidAlphaMultiplier = 1f;
			}
		}
		//Here we use the OnNPCCollision and OnPlayerCollision hooks to apply effects to both entities
		//Firstly, we apply the dryad's ward debuff to NPCs
		public override void OnNPCCollision(NPC npc) {
			//Make sure that the NPC can take damage, and the game is not a player on a server
			if (!npc.dontTakeDamage && !NetmodeActive.MultiplayerClient) npc.AddBuff(BuffID.Oiled, 3 * 60);
		}
		//Secondly, we apply the 2nd tier of Well Fed for 30 seconds
		public override void OnPlayerCollision(Player player) {
			player.AddBuff(ModContent.BuffType<Oiled_Debuff>(), 3 * 60);
		}
		public override void OnProjectileCollision(Projectile proj) {
			if (OriginsSets.Projectiles.FireProjectiles[proj.type]) {
				foreach (Point pos in ContentExtensions.LiquidCollision(proj.position, proj.width, proj.height)) {
					Tile tile = Main.tile[pos];
					if (tile.LiquidType == Oil.ID) {
						tile.LiquidType = Burning_Oil.ID;
						UpdateAdjacentLiquids(pos.X, pos.Y);
					}
				}
			}
		}
		//Here we animate our liquid seperately from other liquids in the game.
		//Instead of having our liquid animate normally, we animate it simiarly, except the liquid is animated almost half as slow
		public override void AnimateLiquid(GameTime gameTime, ref int frame, ref float frameState) {
			float frameSpeed = Main.windSpeedCurrent * 25f;

			frameSpeed = Main.windSpeedCurrent * 15f;
			frameSpeed = ((!(frameSpeed < 0f)) ? (frameSpeed + 5f) : (frameSpeed - 5f));
			frameSpeed = MathF.Abs(frameSpeed);
			frameState += frameSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (frameState < 0f)
				frameState += 16f;

			frameState %= 16f;

			frame = (int)frameState;
		}
		//These methods provide a way to edit the waves produced by a liquid
		//both modify the waves to be slightly larger and offset slightly less when moving through
		public override void NPCRippleModifier(NPC npc, ref float rippleStrength, ref float rippleOffset) {
			if (!npc.wet)
				rippleOffset = -1f;

			float factor = ((float)(int)npc.wetCount / 9f);

			rippleStrength += 0.25f * factor;

			rippleStrength *= 0.5f;
		}

		public override void PlayerRippleModifier(Player player, ref float rippleStrength, ref float rippleOffset) {
			if (!player.wet)
				rippleOffset = -1f;

			float factor = ((float)(int)player.wetCount / 9f);

			rippleStrength += 0.5f * factor;

			rippleStrength *= 0.75f;
		}
		public override int LiquidMerge(int i, int j, int otherLiquid) {
			switch (otherLiquid) {
				case LiquidID.Shimmer: return ModContent.TileType<Super_Sludge>();
			}
			return ModContent.TileType<Murky_Sludge>();
		}
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			switch (otherLiquid) {
				case LiquidID.Shimmer: return true;
			}
			if (Type == Burning_Oil.ID) {
				IgniteOil(tileX - 1, tileY);
				IgniteOil(tileX + 1, tileY);
				IgniteOil(tileX, tileY);
				IgniteOil(liquidX, liquidY);
			} else {
				switch (otherLiquid) {
					case LiquidID.Lava:
					IgniteOil(tileX, tileY);
					IgniteOil(liquidX, liquidY);
					break;

					default:
					if (otherLiquid == Burning_Oil.ID) goto case LiquidID.Lava;
					break;
				}
			}
			return false;
		}
		public static void IgniteOil(int i, int j) {
			Tile tile = Main.tile[i, j];
			if (tile.LiquidType == ID && (tile.LiquidAmount <= CanBurnBeBlockedThreshold || !BlocksIgniting(Main.tile[i, j - 1]))) {
				tile.LiquidType = Burning_Oil.ID;
				UpdateAdjacentLiquids(i, j);
			}
		}
		public static bool BlocksIgniting(Tile tile) {
			return (tile.HasTile && Main.tileSolid[tile.TileType]) || tile.LiquidAmount > CanBurnBeBlockedThreshold;
		}
		public static void UpdateAdjacentLiquids(int i, int j) {
			static void TileFrame(int i, int j) {
				if (Main.tile[i, j].LiquidAmount > 0) WorldGen.TileFrame(i, j);
			}
			TileFrame(i - 1, j - 1);
			TileFrame(i - 1, j);
			TileFrame(i - 1, j + 1);
			TileFrame(i, j - 1);
			TileFrame(i, j);
			TileFrame(i, j + 1);
			TileFrame(i + 1, j - 1);
			TileFrame(i + 1, j);
			TileFrame(i + 1, j + 1);
		}
		#region Splashing
		public void CreateDust(Entity entity) {
			int type = OriginsModIntegrations.CheckAprilFools() ? ModContent.DustType<Black_Smoke_Dust>() : SplashDustType;
			Color color = OriginsModIntegrations.CheckAprilFools() ? default : FromHexRGB(0x1B1B1B);
			Dust dust = Dust.NewDustDirect(new Vector2(entity.position.X - 6f, entity.position.Y + (entity.height / 2) - 8f), entity.width + 12, 24, type, newColor: color);
			dust.velocity.Y -= 1f;
			dust.velocity.X *= 2.5f;
			dust.scale = 1.3f;
			dust.alpha = 100;
			dust.noGravity = true;
		}
		public override bool OnPlayerSplash(Player player, bool isEnter) {
			for (int i = 0; i < 20; i++) {
				CreateDust(player);
			}
			SoundEngine.PlaySound(SplashSound, player.position);
			return false;
		}
		public override bool OnNPCSplash(NPC npc, bool isEnter) {
			for (int i = 0; i < 10; i++) {
				CreateDust(npc);
			}
			//only play the sound if the npc isnt a slime, mouse, tortoise, or if it has no gravity
			if (npc.aiStyle != NPCAIStyleID.Slime &&
					npc.type != NPCID.BlueSlime && npc.type != NPCID.MotherSlime && npc.type != NPCID.IceSlime && npc.type != NPCID.LavaSlime &&
					npc.type != NPCID.Mouse &&
					npc.aiStyle != Terraria.ID.NPCAIStyleID.GiantTortoise &&
					!npc.noGravity) {
				SoundEngine.PlaySound(SplashSound, npc.position);
			}
			return false;
		}

		public override bool OnProjectileSplash(Projectile proj, bool isEnter) {
			for (int i = 0; i < 10; i++) {
				CreateDust(proj);
			}
			SoundEngine.PlaySound(SplashSound, proj.position);
			return false;
		}

		public override bool OnItemSplash(Item item, bool isEnter) {
			for (int i = 0; i < 5; i++) {
				CreateDust(item);
			}
			SoundEngine.PlaySound(SplashSound, item.position);
			return false;
		}
		#endregion
	}
	public class Burning_Oil : Oil {
		public new static int ID { get; private set; }
		public override Color MapColor => FromHexRGB(0xFA8A0A);
		//Temp, so that a difference can be seen
		public override string Texture => "Origins/Water/Riven_Water_Style";
		public override string BlockTexture => "Terraria/Images/Liquid_12";
		public override string SlopeTexture => "Terraria/Images/LiquidSlope_12";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			LiquidID_TLmod.Sets.CountsAsLiquidSource[Type][Oil.ID] = true;
			UsesLavaCollisionForWet = true;
			ID = Type;
		}
		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<Burning_Oil_Fall>().Slot;
		}
		public override void OnNPCCollision(NPC npc) {
			if (!npc.dontTakeDamage && !NetmodeActive.MultiplayerClient) {
				npc.AddBuff(BuffID.Oiled, 3 * 60);
				npc.AddBuff(BuffID.OnFire3, 2 * 60);
			}
		}
		public override void OnPlayerCollision(Player player) {
			player.AddBuff(ModContent.BuffType<Oiled_Debuff>(), 3 * 60);
			player.AddBuff(BuffID.OnFire, 2 * 60);
		}
		public override void OnProjectileCollision(Projectile proj) { }
		public override bool UpdateLiquid(int i, int j, Liquid liquid) {
			Tile tile = Main.tile[i, j];
			if (tile.LiquidAmount > CanBurnBeBlockedThreshold && BlocksIgniting(Main.tile[i, j - 1]) && (Liquid.quickFall || Main.rand.NextBool(3))) {
				tile.LiquidType = Oil.ID;
			} else {
				const int rate = 2;
				if (tile.LiquidAmount > rate) {
					tile.LiquidAmount -= rate;
				} else {
					tile.LiquidAmount = 0;
				}
				UpdateAdjacentLiquids(i, j);
			}
			return base.UpdateLiquid(i, j, liquid);
		}
		public override void EmitEffects(int i, int j, LiquidRenderer.LiquidCache liquidCache) {
			int amount = Main.tile[i, j].LiquidAmount;
			if (amount == 0) return;
			if (Main.rand.NextBool(10000 / amount)) Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat() * (255 - amount)) * 16, DustID.Torch);
		}
		public override bool PreDraw(int i, int j, LiquidRenderer.LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw) {
			return base.PreDraw(i, j, liquidDrawCache, drawOffset, isBackgroundDraw);
		}
		public override void PostDraw(int i, int j, LiquidRenderer.LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw) {
			base.PostDraw(i, j, liquidDrawCache, drawOffset, isBackgroundDraw);
		}
	}
}
