using AltLibrary;
using AltLibrary.Common.AltBiomes;
using Origins.Questing;
using Terraria;
using Terraria.ModLoader;

namespace Origins.World {
	public class OriginsGlobalBiome : GlobalBiome {
		public override void PostConvertTile(AltBiome oldBiome, AltBiome newBiome, int i, int j) {
			if (WorldGen.generatingWorld) return;
			Cleansing_Station_Quest cleansingStationQuest = ModContent.GetInstance<Cleansing_Station_Quest>();
			if (newBiome?.BiomeType != oldBiome?.BiomeType && cleansingStationQuest.LocalPlayerStarted) {
				if (oldBiome?.BiomeType == BiomeType.Evil) {
					cleansingStationQuest.UpdateProgress(1);
				} else if (newBiome?.BiomeType == BiomeType.Evil) {
					cleansingStationQuest.UpdateProgress(-1);
				}
			}
		}
	}
}
