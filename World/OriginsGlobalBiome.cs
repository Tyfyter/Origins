using AltLibrary;
using AltLibrary.Common.AltBiomes;
using Origins.Questing;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.World {
	public class OriginsGlobalBiome : GlobalBiome {
		[ThreadStatic]
		public static bool isConversionFromProjectile;
		public static bool isConvertingProjectilePlayerOwned = false;
		public override void PostConvertTile(AltBiome oldBiome, AltBiome newBiome, int i, int j) {
			if (WorldGen.generatingWorld) return;
			if (newBiome?.BiomeType != oldBiome?.BiomeType) {
				if (isConversionFromProjectile && isConvertingProjectilePlayerOwned) {
					if (ModContent.GetInstance<Cleansing_Station_Quest>() is Cleansing_Station_Quest cleansingStationQuest && cleansingStationQuest.ActiveForLocalPlayer) {
						if (oldBiome?.BiomeType == BiomeType.Evil) {
							cleansingStationQuest.UpdateProgress(1);
						} else if (newBiome?.BiomeType == BiomeType.Evil) {
							cleansingStationQuest.UpdateProgress(-1);
						}
					} else if (ModContent.GetInstance<Bloombomb_Quest>() is Bloombomb_Quest bloombombQuest && bloombombQuest.ActiveForLocalPlayer) {
						if (oldBiome?.BiomeType == BiomeType.Evil) {
							bloombombQuest.UpdateProgress(1);
						} else if (newBiome?.BiomeType == BiomeType.Evil) {
							bloombombQuest.UpdateProgress(-1);
						}
					}
				}
			}
        }
	}
}
