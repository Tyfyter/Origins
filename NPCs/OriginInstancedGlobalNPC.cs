using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public partial class OriginGlobalNPC : GlobalNPC {
        protected override bool CloneNewInstances => true;
        public override bool InstancePerEntity => true;
        internal int shrapnelCount = 0;
        internal int shrapnelTime = 0;
        internal int shockTime = 0;
        internal int rasterizedTime = 0;
        internal int toxicShockTime = 0;
        internal List<int> infusionSpikes;
        internal bool amebolizeDebuff = false;
        public bool tornDebuff = false;
        int tornTime = 0;
        int tornTargetTime = 180;
        float tornTarget = 0.7f;
        public bool slowDebuff = false;
        public bool oldSlowDebuff = false;
        public override void ResetEffects(NPC npc) {
            int rasterized = npc.FindBuffIndex(Rasterized_Debuff.ID);
            if (rasterized >= 0) {
                rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 16), npc.buffTime[rasterized] - 1);
			} else {
                rasterizedTime = 0;
			}
            amebolizeDebuff = false;
            if (tornDebuff) {
                if (tornTime < tornTargetTime) {
                    tornTime++;
                }
            } else if (tornTime > 0 && --tornTime <= 0) {
                tornTargetTime = 180;
                tornTarget = 0.7f;
            }
            tornDebuff = false;
			if (oldSlowDebuff && !slowDebuff) {
                npc.velocity *= 0.7f;
			}
            oldSlowDebuff = slowDebuff;
            slowDebuff = false;
        }
        public override void AI(NPC npc) {
            if(shrapnelTime>0) {
                if(--shrapnelTime<1) {
                    shrapnelCount = 0;
                }
            }
        }
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
            if (amebolizeDebuff) {
                if (npc.lifeRegen > 0) {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 4;
            }
        }
		public static void AddInfusionSpike(NPC npc, int projectileID) {
            OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
            if (globalNPC.infusionSpikes is null) globalNPC.infusionSpikes = new List<int>();
            globalNPC.infusionSpikes.Add(projectileID);
			if (globalNPC.infusionSpikes.Count >= 7) {
                float damage = 0;
                Projectile proj = null;
                for (int i = 0; i < globalNPC.infusionSpikes.Count; i++) {
                    proj = Main.projectile[globalNPC.infusionSpikes[i]];
                    damage += proj.damage * 0.55f;
                    proj.Kill();
                }
                Projectile.NewProjectile(proj.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<Defiled_Spike_Explosion>(), (int)damage, 0, proj.owner, 7);
                globalNPC.infusionSpikes.Clear();
            }

        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if(rasterizedTime > 0) {
	            Origins.rasterizeShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
	            Origins.rasterizeShader.Shader.Parameters["uOffset"].SetValue(npc.velocity.WithMaxLength(4) * 0.0625f * rasterizedTime);
	            Origins.rasterizeShader.Shader.Parameters["uWorldPosition"].SetValue(npc.position);
                Origins.rasterizeShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(TextureAssets.Npc[npc.type].Value.Width, TextureAssets.Npc[npc.type].Value.Height, 0));
                Main.graphics.GraphicsDevice.Textures[1] = Origins.cellNoiseTexture;
			    spriteBatch.Restart(SpriteSortMode.Immediate, effect: Origins.rasterizeShader.Shader);
                return true;
            }
            if(npc.HasBuff(Solvent_Debuff.ID)) {
                Origins.solventShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                Origins.solventShader.Shader.Parameters["uSaturation"].SetValue(Main.npcFrameCount[npc.type]);
                Main.graphics.GraphicsDevice.Textures[1] = Origins.cellNoiseTexture;
                spriteBatch.Restart(SpriteSortMode.Immediate, effect: Origins.solventShader.Shader);
                return true;
            }
            return true;
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if(npc.HasBuff(Solvent_Debuff.ID) || rasterizedTime > 0) {
			    spriteBatch.Restart();
			}
        }
        public static void InflictTorn(NPC npc, int duration, int targetTime = 180, float targetSeverity = 0.7f) {
            npc.AddBuff(Torn_Buff.ID, duration);
            OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
            if (targetSeverity < globalNPC.tornTarget) {
                globalNPC.tornTargetTime = targetTime;
                globalNPC.tornTarget = Math.Max(targetSeverity, float.Epsilon);
            }
        }
    }
}
