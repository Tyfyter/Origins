using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Blind_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Darkness;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.TryGetGlobalNPC(out Blind_Debuff_Global global)) global.blinded = true;
		}
	}
	public class Blind_Debuff_Global : GlobalNPC {
		public override bool InstancePerEntity => true;
		public bool blinded = false;
		public bool blindable = false;
		NPCAimedTarget lastTarget;
		public override void Load() {
			On_NPC.GetTargetData += static (orig, self, ignorePlayerTankPets) => {
				if (self.TryGetGlobalNPC(out Blind_Debuff_Global global)) {
					global.blindable = true;
					if (global.blinded) return global.lastTarget;
					return global.lastTarget = orig(self, ignorePlayerTankPets);
				}
				return orig(self, ignorePlayerTankPets);
			};
		}
		public override void ResetEffects(NPC npc) {
			blinded = false;
		}
		AutoLoadingAsset<Texture2D> blindIndicator = "Origins/Textures/Enemy_Blind_Indicator";
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (blindable && blinded) {
				Main.EntitySpriteDraw(
					blindIndicator,
					npc.Top - new Vector2(0, 24) - screenPos,
					null,
					new Color(225, 180, 255, 180),
					0,
					new Vector2(14, 9),
					1,
					SpriteEffects.None
				);
			}
		}
	}
}
