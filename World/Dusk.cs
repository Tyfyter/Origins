using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using static Origins.OriginExtensions;

namespace Origins.World {
	public class Dusk : ModBiome {
		public override int Music => Origins.Music.Dusk;
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneVoid = OriginSystem.voidTiles > 300;
			originPlayer.ZoneVoidProgress = Math.Min(OriginSystem.voidTiles - 200, 200) / 300f;
			LinearSmoothing(ref originPlayer.ZoneVoidProgressSmoothed, originPlayer.ZoneVoidProgress, OriginSystem.biomeShaderSmoothing);
			return originPlayer.ZoneVoidProgressSmoothed > 0;
		}
		public override void SpecialVisuals(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.ZoneVoidProgressSmoothed > 0) Filters.Scene["Origins:ZoneDusk"].GetShader().UseProgress(originPlayer.ZoneVoidProgressSmoothed);
			player.ManageSpecialBiomeVisuals("Origins:ZoneDusk", originPlayer.ZoneVoidProgressSmoothed > 0, player.Center);
		}
	}
}
