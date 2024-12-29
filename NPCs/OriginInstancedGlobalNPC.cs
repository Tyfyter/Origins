using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Magic;
using Origins.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public partial class OriginGlobalNPC : GlobalNPC {
		protected override bool CloneNewInstances => true;
		public override bool InstancePerEntity => true;
		internal int shockTime = 0;
		internal int rasterizedTime = 0;
		internal int toxicShockStunTime = 0;
		internal List<int> infusionSpikes;
		internal bool amebolizeDebuff = false;
		internal bool beeAfraidDebuff = false;
		internal bool hibernalIncantationDebuff = false;
		internal bool jointPopDebuff = false;
		internal bool ziptieDebuff = false;
		internal bool beeIncantationDebuff = false;
		public bool tornDebuff = false;
		public float tornCurrentSeverity = 0;
		public float tornSeverityRate = 0.3f / 180;
		const float tornSeverityDecayRate = 0.3f / 180;
		public float tornTarget = 0.7f;
		public Vector2 tornOffset = default;
		public bool slowDebuff = false;
		public bool barnacleBuff = false;
		public bool oldSlowDebuff = false;
		public bool weakShadowflameDebuff = false;
		public bool soulhideWeakenedDebuff = false;
		public const float soulhideWeakenAmount = 0.15f;
		public bool weakenedOnSpawn = false;
		public bool amberDebuff = false;
		public Vector2 preAIVelocity = default;
		public int priorityMailTime = 0;
		public int prevPriorityMailTime = 0;
		public bool transformingThroughDeath = false;
		public int birdedTime = 0;
		public int birdedDamage = 0;
		public bool airBird = false;
		public bool deadBird = false;
		public override void ResetEffects(NPC npc) {
			int rasterized = npc.FindBuffIndex(Rasterized_Debuff.ID);
			if (rasterized >= 0) {
				rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 16), npc.buffTime[rasterized] - 1);
				if (Origins.RasterizeAdjustment.TryGetValue(npc.type, out var adjustment)) {
					rasterizedTime = Math.Min(rasterizedTime, adjustment.maxLevel);
				}
			} else {
				rasterizedTime = 0;
			}
			amebolizeDebuff = false;
			beeAfraidDebuff = false;
			hibernalIncantationDebuff = false;
			jointPopDebuff = false;
			ziptieDebuff = false;
			beeIncantationDebuff = false;
			if (tornDebuff) {
				OriginExtensions.LinearSmoothing(ref tornCurrentSeverity, tornTarget, tornSeverityRate);
				if (tornCurrentSeverity >= 1 && Main.netMode != NetmodeID.MultiplayerClient) {
					npc.StrikeInstantKill();
				}
			} else if (tornCurrentSeverity > 0) {
				tornCurrentSeverity -= tornSeverityDecayRate;
				if (tornCurrentSeverity <= 0) {
					tornTarget = 0f;
				}
			}
			tornDebuff = false;
			if (oldSlowDebuff && !slowDebuff) {
				npc.velocity *= 0.7f;
			}
			if (barnacleBuff) {
				barnacleBuff = false;
			}
			oldSlowDebuff = slowDebuff;
			slowDebuff = false;
			weakShadowflameDebuff = false;
			soulhideWeakenedDebuff = false;
			amberDebuff = false;
			if (priorityMailTime > 0) priorityMailTime--;
			if (birdedTime > 0) birdedTime--;
			if (birdedTime <= 0) airBird = false;
			if (deadBird) {
				npc.noTileCollide = false;
				if (birdedTime <= 0) {
					npc.StrikeInstantKill();
				}
			}
		}
		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (priorityMailTime > 0) {
				drawColor.R = (byte)Math.Max(drawColor.R - 85, 0);
				drawColor.G = (byte)Math.Min(drawColor.G + 50, 255);
				drawColor.B = (byte)Math.Min(drawColor.B + 85, 255);
			}
			if (Missing_File_UI.drawingMissingFileUI) {
				drawColor = Missing_File_UI.currentNPCColor;
			}
		}
		public override void AI(NPC npc) {
			if (Main.rand.NextBool(10) && npc.HasBuff(BuffID.Bleeding)) {
				Dust dust10 = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood);
				dust10.velocity.Y += 0.5f;
				dust10.velocity *= 0.25f;
			}
		}
		public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) {
			if (weakenedOnSpawn) return;
			if (soulhideWeakenedDebuff) {
				modifiers.SourceDamage *= (1f - soulhideWeakenAmount);
			}
			if (barnacleBuff) {
				modifiers.SourceDamage *= Main.masterMode ? 1.5f: (Main.expertMode ? 1.55f : 1.6f);
			}
		}
		public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) {
			if (weakenedOnSpawn) return;
			if (soulhideWeakenedDebuff) {
				modifiers.SourceDamage *= (1f - soulhideWeakenAmount);
			}
			if (barnacleBuff) {
				modifiers.SourceDamage *= 1.7f;
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
			if (npc.HasBuff(BuffID.Bleeding)) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 2;
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
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				npc.lifeRegen -= 8;
				damage += 1;
				if (npc.HasBuff(Toxic_Shock_Strengthen_Debuff.ID)) {
					npc.lifeRegen -= 8;
					damage += 1;
				}
			}
		}
		public static void AddInfusionSpike(NPC npc, int projectileID) {
			OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
			globalNPC.infusionSpikes ??= [];
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
			if (tornCurrentSeverity > 0 && !GraphicsUtils.drawingEffect) {
				Torn_Debuff.cachedTornNPCs.Add(npc);
				Torn_Debuff.anyActiveTorn = true;
			}
			return true;
		}
		AutoLoadingAsset<Texture2D> slowIndicator = "Origins/Textures/Enemy_Slow_Indicator";
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (slowDebuff) {
				Main.EntitySpriteDraw(
					slowIndicator,
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
		public void FillShaders(NPC npc, List<ArmorShaderData> shaders) {
			shaders.Clear();
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(Acrid_Dye.ShaderID, null);
				shaderData.UseTargetPosition((npc.Center - Main.screenPosition) / Main.ScreenSize.ToVector2());
				shaders.Add(shaderData);
			}
			if (rasterizedTime > 0) shaders.Add(GameShaders.Armor.GetSecondaryShader(Rasterized_Dye.ShaderID, null));
		}
		public static void InflictTorn(NPC npc, int duration, int targetTime = 180, float targetSeverity = 0.3f, OriginPlayer source = null) {
			if (source is not null) {
				targetSeverity = source.tornStrengthBoost.ApplyTo(targetSeverity);
			}
			OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
			int buffIndex = npc.FindBuffIndex(Torn_Debuff.ID);
			if (buffIndex < 0 || (targetSeverity.CompareTo(globalNPC.tornTarget) + (duration.CompareTo(npc.buffTime[buffIndex]) & 1) > 0)) {
				if (buffIndex < 0) globalNPC.tornOffset = new Vector2(Main.rand.NextFloat());
				npc.AddBuff(Torn_Debuff.ID, duration);
				globalNPC.tornSeverityRate = targetSeverity / targetTime;
				globalNPC.tornTarget = targetSeverity;
				npc.netUpdate = true;
			}
			/*bool hadTorn = npc.HasBuff(Torn_Debuff.ID);
			npc.AddBuff(Torn_Debuff.ID, duration);
			if (!hadTorn || targetSeverity < globalNPC.tornTarget) {
				globalNPC.tornTargetTime = targetTime;
				globalNPC.tornTarget = Math.Max(targetSeverity, float.Epsilon);
			}*/
		}
		public static void InflictImpedingShrapnel(NPC npc, int duration) {
			if (npc.life > 0) {
				npc.AddBuff(Impeding_Shrapnel_Debuff.ID, duration);
			} else if (!npc.HasBuff(Impeding_Shrapnel_Debuff.ID)) {
				Impeding_Shrapnel_Debuff.SpawnShrapnel(npc, duration);
			}
		}
	}
}
