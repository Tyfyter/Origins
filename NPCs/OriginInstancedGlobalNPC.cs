using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public partial class OriginGlobalNPC : GlobalNPC {
        public override bool CloneNewInstances => true;
        public override bool InstancePerEntity => true;
        internal int shrapnelCount = 0;
        internal int shrapnelTime = 0;
        internal int shockTime = 0;
        internal int rasterizedTime = 0;
        public override void ResetEffects(NPC npc) {
            int rasterized = npc.FindBuffIndex(Rasterized_Debuff.ID);
            if (rasterized >= 0) {
                rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 16), npc.buffTime[rasterized] - 1);
            }
        }
        public override void AI(NPC npc) {
            if(shrapnelTime>0) {
                if(--shrapnelTime<1) {
                    shrapnelCount = 0;
                }
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
            if(rasterizedTime > 0) {
			    spriteBatch.End();
	            Origins.rasterizeShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTime);
	            Origins.rasterizeShader.Shader.Parameters["uOffset"].SetValue(npc.velocity.WithMaxLength(4) * 0.0625f * rasterizedTime);
	            Origins.rasterizeShader.Shader.Parameters["uWorldPosition"].SetValue(npc.position);
                Origins.rasterizeShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(Main.npcTexture[npc.type].Width, Main.npcTexture[npc.type].Height, 0));
                Main.graphics.GraphicsDevice.Textures[1] = Origins.cellNoiseTexture;
			    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, Origins.rasterizeShader.Shader, Main.GameViewMatrix.ZoomMatrix);
                return true;
            }
            if(npc.HasBuff(Solvent_Debuff.ID)) {
			    spriteBatch.End();
	            Origins.solventShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTime);
                Origins.solventShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(npc.frame.Y,npc.frame.Height,Main.npcTexture[npc.type].Height));
                Main.graphics.GraphicsDevice.Textures[1] = Origins.cellNoiseTexture;
			    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, Origins.solventShader.Shader, Main.GameViewMatrix.ZoomMatrix);
                return true;
            }
            return true;
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
            if(npc.HasBuff(Solvent_Debuff.ID) || rasterizedTime > 0) {
			    spriteBatch.End();
			    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            }
        }
    }
}
