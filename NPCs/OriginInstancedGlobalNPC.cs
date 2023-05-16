using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
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
		public bool barnacleBuff = false;
		public bool oldSlowDebuff = false;
		public bool weakShadowflameDebuff = false;
		public bool soulhideWeakenedDebuff = false;
		public const float soulhideWeakenAmount = 0.15f;
		public bool weakenedOnSpawn = false;
		public override void ResetEffects(NPC npc) {
			int rasterized = npc.FindBuffIndex(Rasterized_Debuff.ID);
			if (rasterized >= 0) {
				rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 16), npc.buffTime[rasterized] - 1);
				if (Origins.RasterizeAdjustment.TryGetValue(npc.type, out var adjustment)) {
					rasterizedTime = Math.Min(rasterizedTime, adjustment.Item1);
					npc.oldVelocity = Vector2.Lerp(npc.oldVelocity, npc.velocity, adjustment.Item2);
				}
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
			if (barnacleBuff) {
				npc.damage *= (int)1.7f;
                npc.defense *= (int)1.25f;
				npc.velocity *= 1.01f;
            } else {
				npc.damage += 0;
				npc.defense += 0;
				npc.velocity *= 1f;
			}
			oldSlowDebuff = slowDebuff;
			slowDebuff = false;
			weakShadowflameDebuff = false;
			soulhideWeakenedDebuff = false;
		}
		public override void AI(NPC npc) {
			if (shrapnelTime > 0) {
				if (--shrapnelTime < 1) {
					shrapnelCount = 0;
				}
			}
			if (Main.rand.NextBool(10) && npc.HasBuff(BuffID.Bleeding)) {
				Dust dust10 = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood);
				dust10.velocity.Y += 0.5f;
				dust10.velocity *= 0.25f;
			}
		}
		public override void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit) {
			if (weakenedOnSpawn) return;
			if (soulhideWeakenedDebuff) {
				damage = (int)(damage * (1f - soulhideWeakenAmount));
			}
		}
		public override void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit) {
			if (weakenedOnSpawn) return;
			if (soulhideWeakenedDebuff) {
				damage = (int)(damage * (1f - soulhideWeakenAmount));
			}
		}
		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (source is EntitySource_Parent parentSource) {
				if (parentSource.Entity is NPC parentNPC) {
					if ((!npc.chaseable || npc.lifeMax <= 5) && parentNPC.GetGlobalNPC<OriginGlobalNPC>().soulhideWeakenedDebuff) {
						npc.damage = (int)(npc.damage * (1f - soulhideWeakenAmount));
						weakenedOnSpawn = true;
					}
				}
			}
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.lifeRegen > 0 && npc.HasBuff(BuffID.Bleeding)) {
				npc.lifeRegen = 0;
			}
			if (amebolizeDebuff) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 4;
			}
			if (weakShadowflameDebuff) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 15;
				damage += 1;
				if(Main.rand.Next(5) < 3) {
					Dust dust = Dust.NewDustDirect(new Vector2(npc.position.X - 2f, npc.position.Y - 2f), npc.width + 4, npc.height + 4, DustID.Shadowflame, npc.velocity.X * 0.3f, npc.velocity.Y * 0.3f, 220, Color.White, 1.75f);
					dust.noGravity = true;
					dust.velocity *= 0.75f;
					dust.velocity.X *= 0.75f;
					dust.velocity.Y -= 1f;
					if (Main.rand.NextBool(4)) {
						dust.noGravity = false;
						dust.scale *= 0.5f;
					}
				}
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
			if (rasterizedTime > 0) {
				Origins.rasterizeShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
				Origins.rasterizeShader.Shader.Parameters["uOffset"].SetValue(npc.velocity.WithMaxLength(4) * 0.0625f * rasterizedTime);
				Origins.rasterizeShader.Shader.Parameters["uWorldPosition"].SetValue(npc.position);
				Origins.rasterizeShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(TextureAssets.Npc[npc.type].Value.Width, TextureAssets.Npc[npc.type].Value.Height, 0));
				Main.graphics.GraphicsDevice.Textures[1] = Origins.cellNoiseTexture;
				spriteBatch.Restart(SpriteSortMode.Immediate, effect: Origins.rasterizeShader.Shader);
				return true;
			}
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				Origins.solventShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
				Origins.solventShader.Shader.Parameters["uSaturation"].SetValue(Main.npcFrameCount[npc.type]);
				Main.graphics.GraphicsDevice.Textures[1] = Origins.cellNoiseTexture;
				spriteBatch.Restart(SpriteSortMode.Immediate, effect: Origins.solventShader.Shader);
				return true;
			}
			return true;
		}
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (npc.HasBuff(Toxic_Shock_Debuff.ID) || rasterizedTime > 0) {
				spriteBatch.Restart();
			}
		}
		public static void InflictTorn(NPC npc, int duration, int targetTime = 180, float targetSeverity = 0.7f, OriginPlayer source = null) {
			if (source is not null) {
				if (source.taintedFlesh) {
					targetSeverity *= 0.9f;
				}
				if (source.taintedFlesh2) {
					targetSeverity *= 0.81f;
				}
				if (source.rivenSetBoost) {
					targetSeverity -= 0.15f;
				}
			}
			bool hadTorn = npc.HasBuff(Torn_Buff.ID);
			npc.AddBuff(Torn_Buff.ID, duration);
			OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
			if (!hadTorn || targetSeverity < globalNPC.tornTarget) {
				globalNPC.tornTargetTime = targetTime;
				globalNPC.tornTarget = Math.Max(targetSeverity, float.Epsilon);
			}
		}
	}
}
