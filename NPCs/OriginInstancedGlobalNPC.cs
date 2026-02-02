using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Magic;
using Origins.NPCs.Defiled;
using Origins.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
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
		[CloneByReference] internal List<int> infusionSpikes;
		internal bool amebolizeDebuff = false;
		internal bool beeAfraidDebuff = false;
		internal bool hibernalIncantationDebuff = false;
		internal bool jointPopDebuff = false;
		internal bool ziptieDebuff = false;
		internal bool mildewWhipDebuff = false;
		internal bool ocotilloFingerDebuff = false;
		internal bool acridSpoutDebuff = false;
		internal bool accretionRibbonDebuff = false;
		internal bool brineIncantationDebuff = false;
		internal bool injectIncantationDebuff = false;
		internal bool mildewIncantationDebuff = false;
		public bool tornDebuff = false;
		public float tornCurrentSeverity = 0;
		public float tornSeverityRate = 0.3f / 180;
		const float tornSeverityDecayRate = 0.3f / 180;
		public float tornTarget = 0.7f;
		public Vector2 tornOffset = default;
		public bool slowDebuff = false;
		public bool silencedDebuff = false;
		public bool weakDebuff = false;
		public const float weakDebuffAmount = 0.5f;
		public const float weakDebuffAmountBoss = 0.2f;
		public bool brokenArmorDebuff = false;
		public bool barnacleBuff = false;
		public bool oldSlowDebuff = false;
		public bool shadeFire = false;
		public bool soulhideWeakenedDebuff = false;
		public bool cavitationDebuff = false;
		public bool staticShock = false;
		public bool miniStaticShock = false;
		public bool staticShockDamage = false;
		public int staticShockTime = 0;
		public bool electrified = false;
		public const float soulhideWeakenAmount = 0.15f;
		public bool weakenedOnSpawn = false;
		public bool amberDebuff = false;
		public bool onSoMuchFire = false;
		public Vector2 preAIVelocity = default;
		public int priorityMailTime = 0;
		public int prevPriorityMailTime = 0;
		public bool transformingThroughDeath = false;
		public int birdedTime = 0;
		public int birdedDamage = 0;
		public bool airBird = false;
		public bool deadBird = false;
		public int sonarDynamiteTime = 0;
		public bool lazyCloakShimmer = false;
		public int shinedownDamage = 0;
		public float shinedownSpeed = 1;
		public int sentinelDamage = 0;
		public float sentinelSpeed = 1;
		public bool amnesticRose = false;
		public int amnesticRoseGooProj = 0;
		public bool tetanus = false;
		public override void ResetEffects(NPC npc) {
			int rasterized = npc.FindBuffIndex(Rasterized_Debuff.ID);
			if (rasterized >= 0) {
				rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 16), npc.buffTime[rasterized] - 1);
				if (Origins.RasterizeAdjustment.TryGetValue(npc.type, out (int maxLevel, float accelerationFactor, float velocityFactor) adjustment)) {
					rasterizedTime = Math.Min(rasterizedTime, adjustment.maxLevel);
				}
			} else {
				rasterizedTime = 0;
			}
			jointPopDebuff = false;
			ziptieDebuff = false;
			mildewWhipDebuff = false;
			ocotilloFingerDebuff = false;
			acridSpoutDebuff = false;
			accretionRibbonDebuff = false;
			amebolizeDebuff = false;
			beeAfraidDebuff = false;
			hibernalIncantationDebuff = false;
			brineIncantationDebuff = false;
			injectIncantationDebuff = false;
			mildewIncantationDebuff = false;
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
			silencedDebuff = false;
			weakDebuff = false;
			brokenArmorDebuff = false;
			shadeFire = false;
			soulhideWeakenedDebuff = false;
			cavitationDebuff = false;
			staticShock = false;
			miniStaticShock = false;
			staticShockDamage = false;
			electrified = false;
			amberDebuff = false;
			onSoMuchFire = false;
			if (priorityMailTime > 0) priorityMailTime--;
			if (birdedTime > 0) birdedTime--;
			if (birdedTime <= 0) airBird = false;
			if (sonarDynamiteTime > 0) sonarDynamiteTime--;
			if (deadBird) {
				npc.noTileCollide = false;
				if (birdedTime <= 0) {
					npc.StrikeInstantKill();
				}
			}
			lazyCloakShimmer = false;
			amnesticRose = false;
			tetanus = false;
		}
		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (priorityMailTime > 0) {
				drawColor.R = (byte)Math.Max(drawColor.R - 85, 0);
				drawColor.G = (byte)Math.Min(drawColor.G + 50, 255);
				drawColor.B = (byte)Math.Min(drawColor.B + 85, 255);
			}
			if (sonarDynamiteTime > 0) {
				drawColor.R = (byte)Math.Max(drawColor.R - 150, 0);
				drawColor.G = 255;
				drawColor.B = 255;
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
			if (soulhideWeakenedDebuff && !weakenedOnSpawn) {
				modifiers.SourceDamage *= (1f - soulhideWeakenAmount);
			}
			if (barnacleBuff) {
				modifiers.SourceDamage *= Main.masterMode ? 1.5f : (Main.expertMode ? 1.55f : 1.6f);
			}
			if (weakDebuff) {
				modifiers.SourceDamage *= (1f - (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type] ? weakDebuffAmountBoss : weakDebuffAmount));
			}
		}
		public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) {
			if (soulhideWeakenedDebuff && !weakenedOnSpawn) {
				modifiers.SourceDamage *= (1f - soulhideWeakenAmount);
			}
			if (barnacleBuff) {
				modifiers.SourceDamage *= 1.7f;
			}
			if (weakDebuff) {
				modifiers.SourceDamage *= (1f - (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type] ? weakDebuffAmountBoss : weakDebuffAmount));
			}
		}
		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (source is EntitySource_Parent parentSource) {
				if (parentSource.Entity is NPC parentNPC) {
					OriginGlobalNPC parentGlobal = parentNPC.GetGlobalNPC<OriginGlobalNPC>();
					if ((!npc.chaseable || npc.lifeMax <= 5) && parentGlobal.soulhideWeakenedDebuff) {
						npc.damage = (int)(npc.damage * (1f - soulhideWeakenAmount));
						weakenedOnSpawn = true;
					}
					if (parentGlobal.silencedDebuff && NPCID.Sets.ProjectileNPC[npc.type]) {
						npc.life = 0;
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
			if (onSoMuchFire) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 20;
				damage += 3;
			}
			if (shadeFire) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 15;
				damage += 1;
				if (Main.rand.Next(5) < 3) {
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
			if (cavitationDebuff) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 66;
				if (damage < 3) damage = 3;
				/*Dust dust = Dust.NewDustDirect(new Vector2(npc.position.X - 2f, npc.position.Y - 2f), npc.width + 4, npc.height + 4, DustID.BreatheBubble, npc.velocity.X * 0.3f, npc.velocity.Y * 0.3f, 220, Color.White, 1.75f);
				dust.noGravity = true;
				dust.velocity *= 0.75f;
				dust.velocity.X *= 0.75f;
				dust.velocity.Y -= 1f;
				if (Main.rand.NextBool(4)) {
					dust.noGravity = false;
					dust.scale *= 0.5f;
				}*/
			}
			if (tetanus) {
				Min(ref npc.lifeRegen, 0);
				npc.lifeRegen -= Tetanus_Debuff.DPS * 2;
				if (damage < Tetanus_Debuff.DPS) damage = Tetanus_Debuff.DPS;
			}
			if (staticShock || miniStaticShock || staticShockDamage) {
				if (npc.lifeRegen > 0) {
					npc.lifeRegen = 0;
				}
				float damageMult = 1 + npc.wet.ToInt() + ((staticShock || miniStaticShock) && staticShockDamage).ToInt() + IDefiledEnemy.GetZapWeakness(npc);
				npc.lifeRegen -= Main.rand.RandomRound(8 * damageMult);
				Max(ref damage, Main.rand.RandomRound(3 * damageMult));
			}
			if (electrified) {
				float damageMult = 1 + npc.wet.ToInt() + IDefiledEnemy.GetZapWeakness(npc);
				npc.lifeRegen -= Main.rand.RandomRound(40 * damageMult);
				Max(ref damage, Main.rand.RandomRound(20 * damageMult));
			}
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				npc.lifeRegen -= 15;
				damage += 2;
				if (npc.HasBuff(Toxic_Shock_Strengthen_Debuff.ID)) {
					npc.lifeRegen -= 15;
					damage += 2;
				}
			}
			if (shinedownDamage > 0) {
				int displayedDamage = Main.rand.RandomRound(shinedownDamage / (shinedownSpeed * 1.5f));
				if (damage < displayedDamage) damage = displayedDamage;
				npc.lifeRegenCount -= shinedownDamage * 4;
				shinedownDamage = 0;
			}
			if (sentinelDamage > 0) {
				int displayedDamage = Main.rand.RandomRound(sentinelDamage / (sentinelSpeed * 1.5f));
				if (damage < displayedDamage) damage = displayedDamage;
				npc.lifeRegenCount -= sentinelDamage * 4;
				sentinelDamage = 0;
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
					damage += proj.damage * 0.75f;
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
		AutoLoadingAsset<Texture2D> slowIndicator = "Origins/Buffs/Indicators/Enemy_Slow";
		AutoLoadingAsset<Texture2D> silencedIndicator = "Origins/Buffs/Indicators/Enemy_Silenced";
		AutoLoadingAsset<Texture2D> blindIndicator = "Origins/Buffs/Indicators/Enemy_Blind";
		AutoLoadingAsset<Texture2D> weakIndicator = "Origins/Buffs/Indicators/Enemy_Weak";
		AutoLoadingAsset<Texture2D> brokenArmorIndicator = "Origins/Buffs/Indicators/Enemy_Broken_Armor";
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			List<Texture2D> indicators = [];
			Vector2 pos = Vector2.Zero;
			void AddIndicator(Texture2D indicator) {
				indicators.Add(indicator);
				pos.X -= indicator.Width * 0.5f;
			}
			if (slowDebuff) AddIndicator(slowIndicator);
			if (silencedDebuff) AddIndicator(silencedIndicator);
			if (npc.TryGetGlobalNPC(out Blind_Debuff_Global blindGlobal) && blindGlobal.IsReallyBlinded) AddIndicator(blindIndicator);
			if (weakDebuff) AddIndicator(weakIndicator);
			if (brokenArmorDebuff) AddIndicator(brokenArmorIndicator);
			for (int i = 0; i < indicators.Count; i++) {
				Texture2D indicator = indicators[i];
				pos.X += indicator.Width * 0.5f;
				Main.EntitySpriteDraw(
					indicator,
					npc.Top + pos - new Vector2(0, 24) - screenPos,
					null,
					new Color(225, 180, 255, 180),
					0,
					indicator.Size() * 0.5f,
					1,
					SpriteEffects.None
				);
				pos.X += indicator.Width * 0.5f;
			}
		}
		public void FillShaders(NPC npc, List<ArmorShaderData> shaders) {
			shaders.Clear();
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(Acrid_Dye.ShaderID, null);
				shaderData.UseTargetPosition(((npc.Center - Main.screenPosition) / Main.ScreenSize.ToVector2()).Apply(Main.GameViewMatrix.Effects, Vector2.One));
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
