using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils.Structs;
using Origins.Dusts;
using Origins.Liquids.Waterfalls;
using Origins.Tiles.Ashen;
using System;
using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Liquids {
	public class Oil : ModLiquid {
		public override string Texture => base.Texture.Replace("Burning_", string.Empty);
		public override string BlockTexture => base.BlockTexture.Replace("Burning_", string.Empty);
		public override string SlopeTexture => base.SlopeTexture.Replace("Burning_", string.Empty);
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			LiquidRenderer.VISCOSITY_MASK[Type] = 50;
			LiquidRenderer.WATERFALL_LENGTH[Type] = 10;
			LiquidRenderer.DEFAULT_OPACITY[Type] = 0.98f;
			LiquidID_TLmod.Sets.CanBeAbsorbedBy[Type].Add(ItemID.SuperAbsorbantSponge);
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
			ID = Type;
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
			//Here, our liquid in the bottom half of the underworld evaporates, while in the upper half does not evaporate
			if (j > Main.UnderworldLayer) {
				Main.tile[i, j].SetLiquidType(ID);
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
			player.AddBuff(BuffID.Oiled, 3 * 60);
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
				case LiquidID.Shimmer: return ModContent.TileType<Murky_Sludge>(); // TODO: replace with "Super Sludge"
			}
			return ModContent.TileType<Murky_Sludge>();
		}
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			if (otherLiquid == ID) Main.tile[tileX, tileY].SetLiquidType(ID);
			switch (otherLiquid) {
				case LiquidID.Lava:
				Main.tile[tileX, tileY].SetLiquidType(ID);
				break;

				case LiquidID.Shimmer: return true;
			}
			return false;
		}
	}
	public class Burning_Oil : Oil { // TODO: figure out a way to un-burn the oil
		public new static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			LiquidID_TLmod.Sets.CanBeAbsorbedBy[Type].Remove(ItemID.SuperAbsorbantSponge);
			LiquidID_TLmod.Sets.CanBeAbsorbedBy[Type].Add(ItemID.LavaAbsorbantSponge);
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
			player.AddBuff(BuffID.Oiled, 3 * 60);
			player.AddBuff(BuffID.OnFire3, 2 * 60);
		}
		public override bool UpdateLiquid(int i, int j, Liquid liquid) {
			if (Main.tile[i, j].LiquidAmount >= 127 && Main.rand.NextBool(200)) Dust.NewDust(new Vector2(i, j) * 16, 1, 1, DustID.Torch);
			return true;
		}
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			return false;
		}
	}
}
