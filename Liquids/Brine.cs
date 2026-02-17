using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils.Structs;
using Origins.Buffs;
using Origins.Liquids.Waterfalls;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Liquids {
	public class Brine : ModLiquid {
		public static int ID { get; private set; }
		public override void Load() {
			MonoModHooks.Add(typeof(LiquidLoader).GetMethod(nameof(LiquidLoader.LiquidMergeTilesType)),
				(orig_LiquidMergeTilesType orig, int i, int j, int type, int otherLiquid) => {
					if (otherLiquid == ID) otherLiquid = LiquidID.Water;
					return orig(i, j, type, otherLiquid);
			});
			MonoModHooks.Add(typeof(LiquidLoader).GetMethod(nameof(LiquidLoader.LiquidMergeSounds)),
				(orig_LiquidMergeSounds orig, int i, int j, int type, int otherLiquid, ref SoundStyle? collisionSound) => {
					if (otherLiquid == ID) otherLiquid = LiquidID.Water;
					orig(i, j, type, otherLiquid, ref collisionSound);
				});
			MonoModHooks.Add(typeof(LiquidLoader).GetMethod(nameof(LiquidLoader.PreLiquidMerge)),
				(orig_PreLiquidMerge orig, int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid) => {
					if (otherLiquid == ID) otherLiquid = LiquidID.Water;
					return orig(liquidX, liquidY, tileX, tileY, type, otherLiquid);
				});
		}
		public override void SetStaticDefaults() {
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
		public override bool EvaporatesInHell(int i, int j) {
			return true;
		}
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
				return false;
			}
			return true;
		}
		delegate int? hook_LiquidMergeTilesType(orig_LiquidMergeTilesType orig, int i, int j, int type, int otherLiquid);
		delegate int? orig_LiquidMergeTilesType(int i, int j, int type, int otherLiquid);
		delegate void hook_LiquidMergeSounds(orig_LiquidMergeTilesType orig, int i, int j, int type, int otherLiquid, ref SoundStyle? collisionSound);
		delegate void orig_LiquidMergeSounds(int i, int j, int type, int otherLiquid, ref SoundStyle? collisionSound);
		delegate bool hook_PreLiquidMerge(orig_LiquidMergeTilesType orig, int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid);
		delegate bool orig_PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid);
	}
}
