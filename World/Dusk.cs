using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using static Origins.OriginExtensions;

namespace Origins.World.BiomeData {
	public class Dusk : ModBiome {
		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
		public override int Music => Origins.Music.Dusk;
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneVoid = OriginSystem.voidTiles > 300;
			originPlayer.ZoneVoidProgress = Math.Clamp(OriginSystem.voidTiles - 200, 0, 200) / 300f;
			LinearSmoothing(ref originPlayer.ZoneVoidProgressSmoothed, originPlayer.ZoneVoidProgress, OriginSystem.biomeShaderSmoothing);
			return originPlayer.ZoneVoid;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			Filters.Scene["Origins:ZoneDusk"].GetShader().UseProgress(originPlayer.ZoneVoidProgressSmoothed);
			player.ManageSpecialBiomeVisuals("Origins:ZoneDusk", originPlayer.ZoneVoidProgressSmoothed > 0, player.Center);
		}
	}
}
