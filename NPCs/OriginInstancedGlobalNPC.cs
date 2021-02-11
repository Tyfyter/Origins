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
        public override void AI(NPC npc) {
            if(shrapnelTime>0) {
                if(--shrapnelTime<1) {
                    shrapnelCount = 0;
                }
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
            if(npc.HasBuff(SolventDebuff.ID)) {
			    spriteBatch.End();
                //Origins.solventShader.Shader.Parameters["uProgress"].SetValue();
			    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, Origins.solventShader.Shader, Main.GameViewMatrix.ZoomMatrix);
                Main.graphics.GraphicsDevice.Textures[1] = Origins.cellNoiseTexture;
            }
            return true;
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
            if(npc.HasBuff(SolventDebuff.ID)) {
			    spriteBatch.End();
			    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            }
        }
    }
}
