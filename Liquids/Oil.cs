using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using ModLiquidLib.Utils.Structs;
using Origins.Dusts;
using Origins.Liquids.Waterfalls;
using PegasusLib.Networking;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Liquids {
	public class Oil : ModLiquid {
		public override void SetStaticDefaults() {
			LiquidRenderer.VISCOSITY_MASK[Type] = 100;
			LiquidRenderer.WATERFALL_LENGTH[Type] = 10;
			LiquidRenderer.DEFAULT_OPACITY[Type] = 0.98f;
			SlopeOpacity = 1f;
			LiquidfallOpacityMultiplier = 0.8f;
			WaterRippleMultiplier = 0.3f;
			SplashDustType = ModContent.DustType<Black_Smoke_Dust>();
			SplashSound = SoundID.SplashWeak;
			FallDelay = 8;
			ChecksForDrowning = true;
			AllowEmitBreathBubbles = false;
			PlayerMovementMultiplier = 0.375f;
			StopWatchMPHMultiplier = PlayerMovementMultiplier;
			NPCMovementMultiplierDefault = PlayerMovementMultiplier;
			ProjectileMovementMultiplier = PlayerMovementMultiplier;
			ExtinguishesOnFireDebuffs = false;
			AddMapEntry(FromHexRGB(0x0A0A0A));
		}
		//ChooseWaterfallStyle allows for the selection of what waterfall style this liquid chooses when next to a slope.
		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<OilFall>().Slot;
		}
		//LiquidLightMaskMode is how the game decides what lightMaskMode to use when this liquid is over a tile
		//We set this to none, this is due to the liquid emitting light, needing no special lightMaskMode for its interaction with light.
		public override LightMaskMode LiquidLightMaskMode(int i, int j) {
			return LightMaskMode.Honey;
		}
		//Using EvaporatesInHell, we are able to choose whether this liquid evaporates in hell, based on a condition.
		//For custom evaporation, use UpdateLiquid override.
		public override bool EvaporatesInHell(int i, int j) {
			//Here, our liquid in the bottom half of the underworld evaporates, while in the upper half does not evaporate
			if (j > Main.UnderworldLayer + 10) {
				return true;
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
			if (!NetmodeActive.MultiplayerClient) {
				if (npc.HasBuff(BuffID.OnFire)) npc.buffTime[npc.FindBuffIndex(BuffID.OnFire)]++;
			}
		}
		//Secondly, we apply the 2nd tier of Well Fed for 30 seconds
		public override void OnPlayerCollision(Player player) {
			if (!NetmodeActive.MultiplayerClient) {
				if (player.HasBuff(BuffID.OnFire)) player.buffTime[player.FindBuffIndex(BuffID.OnFire)]++;
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
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			switch (otherLiquid) {
				case LiquidID.Shimmer:
				Main.tile[tileX, tileY].SetLiquidType(LiquidID.Water);
				break;
			}
			//new Oil_Liquid_Merge(tileX, tileY, otherLiquid).Perform();
			return false;
		}
	}
	public record class Oil_Liquid_Merge(int TileX, int TileY, int OtherLiquid) : SyncedAction {
		public Oil_Liquid_Merge() : this(default, default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			TileX = reader.ReadByte(),
			TileY = reader.ReadByte(),
			OtherLiquid = reader.ReadByte()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)TileX);
			writer.Write((byte)TileY);
			writer.Write((byte)OtherLiquid);
		}

		protected override void Perform() {
			switch (OtherLiquid) {
				case LiquidID.Shimmer:
				Main.tile[TileX, TileY].SetLiquidType(LiquidID.Water);
				break;
			}
		}
	}
}
