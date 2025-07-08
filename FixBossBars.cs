using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria;
using Terraria.ModLoader;

namespace Origins {
	public class ModBossBar : Terraria.ModLoader.ModBossBar {
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return Asset<Texture2D>.Empty;
		}
		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) {
			if (Main.npc.GetIfInRange(info.npcIndexToAimAt).BossBar != this) return false;
			return null;
		}
	}
}
