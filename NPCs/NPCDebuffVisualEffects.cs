using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Summoner;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.NPCs {
	public class NPCDebuffVisualEffects : GlobalNPC, PegasusLib.IDrawNPCEffect {
		static List<ArmorShaderData> shaders = [];
		public override void Load() => shaders = [];
		public override void Unload() => shaders = null;
		public void PrepareToDrawNPC(NPC npc) {
			npc.GetGlobalNPC<OriginGlobalNPC>().FillShaders(npc, shaders);
			if (shaders.Count != 0) Origins.shaderOroboros.Capture();
			if (!Lighting.NotRetro) Main.spriteBatch.Restart(Main.spriteBatch.GetState().FixedCulling());
		}
		public void FinishDrawingNPC(NPC npc) {
			if (shaders.Count != 0) {
				for (int i = 0; i < shaders.Count; i++) {
					Origins.shaderOroboros.Stack(shaders[i], npc);
				}
				Origins.shaderOroboros.Release();
			}
		}
	}
}
