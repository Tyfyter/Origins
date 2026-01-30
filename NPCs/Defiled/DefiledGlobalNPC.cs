using Origins.Buffs;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.NPCs.Defiled {
	public class DefiledGlobalNPC : GlobalNPC {
		public static Dictionary<int, int> NPCTransformations { get; private set; } = [];
		public override bool InstancePerEntity => true;
		public NPC broadcasterHoldingThisNPC;
		public override void Load() {
			hasErroredAboutWrongNPC = [];
		}
		public override void Unload() {
			NPCTransformations = null;
			hasErroredAboutWrongNPC = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			if (entity.ModNPC is IDefiledEnemy defiledEnemy) {
				if (defiledEnemy.Assimilation is AssimilationAmount amount) {
					BiomeNPCGlobals.NPCAssimilationAmounts[entity.type] = new() {
						[ModContent.GetInstance<Defiled_Assimilation>().AssimilationType] = amount
					};
				}
				return true;
			}
			return false;
		}
		static HashSet<int> hasErroredAboutWrongNPC;
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy) {
				if (npc.life < npc.lifeMax && defiledEnemy.Mana > 0) {
					defiledEnemy.Regenerate(out int lifeRegen);
					if (!npc.HasBuff(BuffID.Bleeding)) npc.lifeRegen += lifeRegen;
				}
			} else {
				if (!hasErroredAboutWrongNPC.Add(npc.type)) {
					Origins.LogError($"something has gone extremely wrong and a non-defiled enemy ({npc.ModNPC}) has a DefiledGlobalNPC");
				}
			}
			if (npc.poisoned) {
				npc.lifeRegen += 6;
			}
			if (npc.onFire) {
				npc.lifeRegen += 4;
			}
			if (npc.onFire2) {// cursed inferno
				npc.lifeRegen += 24;
			}
			if (npc.onFire3) {// hellfire
				npc.lifeRegen += 15;
			}
			if (npc.onFrostBurn) {
				npc.lifeRegen -= 4;
				damage += 1;
			}
			if (npc.onFrostBurn2) {
				npc.lifeRegen -= 16;
				damage += 4;
			}
			/* vanilla electrified debuff can't be inflicted on enemies, so it might cause issues if we actually use it on them, instead we'll make a new debuff like with slowed 
			if (npc.HasBuff(BuffID.Electrified)) {
				npc.lifeRegen -= 20;
			}
			if (npc.HasBuff(BuffID.Electrified) && npc.wet) {
				npc.lifeRegen -= 40;
			}*/
			if (npc.shadowFlame) {
				npc.lifeRegen += 15;
			}
			if (npc.oiled && (npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame)) {
				npc.lifeRegen += 10;
			}
			if (npc.daybreak) {
				npc.lifeRegen += 25 * 2;
				damage -= 5;
			}
			if (npc.javelined) {
				npc.lifeRegen -= 6;
				damage += 3;
			}
			if (npc.dryadBane) {
				const float baseDPS = 2;
				int totalDPS = (int)(baseDPS * BiomeNPCGlobals.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
		public override void PostAI(NPC npc) {
			if (broadcasterHoldingThisNPC?.ModNPC is Defiled_Broadcaster broadcaster) {
				if (broadcasterHoldingThisNPC.ai[2] != 0 || broadcasterHoldingThisNPC.ai[1] != npc.WhoAmIToTargettingIndex) {
					broadcasterHoldingThisNPC = null;
					return;
				}
				npc.Top = broadcaster.CarryPosition;
				npc.velocity = broadcasterHoldingThisNPC.velocity;
			}
		}
		public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy) {
				defiledEnemy.DrainMana(target);
			}
		}
		public override void OnKill(NPC npc) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy) {
				defiledEnemy.SpawnWisp(npc);
			}
		}
		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy && defiledEnemy.ForceSyncMana) {
				binaryWriter.Write(defiledEnemy.Mana);
			}
		}
		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy && defiledEnemy.ForceSyncMana) {
				defiledEnemy.Mana = binaryReader.ReadSingle();
			}
		}
	}
	public interface IDefiledEnemy {
		int MaxManaDrain => 0;
		int MaxMana => 0;
		float ZapWeakness => 1;
		AssimilationAmount? Assimilation => null;
		float Mana { get; set; }
		bool ForceSyncMana => true;
		void Regenerate(out int lifeRegen) { lifeRegen = 0; }
		void SpawnWisp(NPC npc) {
			if (Main.expertMode) NPC.NewNPC(npc.GetSource_Death(), (int)npc.position.X, (int)npc.position.Y, ModContent.NPCType<Defiled_Wisp>());
		}
		public void DrainMana(Player target) {
			int maxDrain = (int)Math.Min(MaxMana - Mana, MaxManaDrain);
			int manaDrain = Math.Min(maxDrain, target.statMana);
			target.statMana -= manaDrain;
			Mana += manaDrain;
			if (target.manaRegenDelay < 10) target.manaRegenDelay = 10;
		}
		public (Rectangle startArea, Predicate<Vector2> customShape)? GetCustomChrysalisShape(NPC chrysalisNPC) => null;
		public void OnChrysalisSpawn() { }
		public static float GetZapWeakness(NPC npc) => npc.ModNPC is IDefiledEnemy zapee ? zapee.ZapWeakness : 0;
	}
}
