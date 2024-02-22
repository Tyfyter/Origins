using AltLibrary;
using AltLibrary.Common.AltBiomes;
using Origins.Questing;
using Terraria;
using Terraria.ModLoader;

namespace Origins.World {
	public class OriginsGlobalBiome : GlobalBiome {
		public override void PostConvertTile(AltBiome oldBiome, AltBiome newBiome, int i, int j) {
			if (WorldGen.generatingWorld) return;
			if (newBiome?.BiomeType != oldBiome?.BiomeType && ModContent.GetInstance<Cleansing_Station_Quest>() is Cleansing_Station_Quest cleansingStationQuest && cleansingStationQuest.ActiveForLocalPlayer) {
				if (oldBiome?.BiomeType == BiomeType.Evil) {
					cleansingStationQuest.UpdateProgress(1);
				} else if (newBiome?.BiomeType == BiomeType.Evil) {
					cleansingStationQuest.UpdateProgress(-1);
				}
			}
        }
	}
}
