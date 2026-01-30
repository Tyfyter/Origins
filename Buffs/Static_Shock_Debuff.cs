using Origins.NPCs;
using Origins.NPCs.Defiled;
using PegasusLib;
using PegasusLib.UI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Static_Shock_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.Electrified
			];
			Buff_Hint_Handler.ModifyTip(Type, this is Mini_Static_Shock_Debuff ? 0 : 4, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().staticShock = true;
			DoDust(player);
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().staticShock = true;
			DoDust(npc);
		}
		public static void DoDust(Entity entity) {
			float zappiness = 1 + entity.wet.ToInt();
			if (entity is NPC npc) zappiness += npc.HasBuff<Static_Shock_Damage_Debuff>().ToInt() + IDefiledEnemy.GetZapWeakness(npc);
			else if (entity is Player player) zappiness += player.HasBuff<Static_Shock_Damage_Debuff>().ToInt();
			if (Main.rand.Next(4) < zappiness) {
				Vector2 offset = Main.rand.NextVector2FromRectangle(entity.Hitbox) - entity.Center;
				Dust dust = Dust.NewDustPerfect(
					entity.Center - offset,
					DustID.Electric,
					Vector2.Zero,
					Scale: 0.5f
				);
				dust.velocity += entity.velocity;
				dust.noGravity = true;
			}
		}
		public static void Inflict(Entity victim, int time) {
			bool isDead = false;
			if (victim is NPC npc) {
				npc.AddBuff(ID, time);
				isDead = npc.life <= 0;
			} else if (victim is Player player) {
				player.AddBuff(ID, time);
				isDead = player.dead || player.statLife < 0;
			}
			if (isDead) ProcessShocking(victim);
		}
		public static void ProcessShocking(Entity entity, float baseRange = 5) {
			int damageDebuffID = ModContent.BuffType<Static_Shock_Damage_Debuff>();
			Rectangle entityHitbox;
			bool entityWet;
			bool ShouldInflict(Rectangle hitbox, bool wet) {
				Vector2 pointA = hitbox.Center().Clamp(entityHitbox);
				Vector2 pointB = entityHitbox.Center().Clamp(hitbox);
				if (pointA.IsWithin(pointB, 16 * baseRange * ((entityWet || wet) ? 2 : 1))) {
					if (!Main.dedServ && Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), pointA, pointB)) {
						Dust.NewDustPerfect(pointA, ModContent.DustType<Static_Shock_Arc_Dust>(), pointB);
						SoundEngine.PlaySound(Origins.Sounds.LittleZap, (pointA + pointB) * 0.5f);
					}
					return Main.netMode != NetmodeID.MultiplayerClient;
				}
				return false;
			}
			if (entity is Player player) {
				ref int staticShockTime = ref player.OriginPlayer().staticShockTime;
				if (staticShockTime > 0) staticShockTime--;
				if (staticShockTime <= 0) {
					staticShockTime = 20;
					entityWet = player.wet;
					entityHitbox = player.Hitbox;
					foreach (Player other in Main.ActivePlayers) {
						if (other == player || other.buffImmune[damageDebuffID]) continue;
						if (ShouldInflict(other.Hitbox, other.wet)) other.AddBuff(damageDebuffID, 20);
					}
					foreach (NPC other in Main.ActiveNPCs) {
						if (other.buffImmune[damageDebuffID] || (other.dontTakeDamage && !other.ShowNameOnHover)) continue;
						if (other.friendly && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
							other.AddBuff(damageDebuffID, 20);
						}
					}
				}
			} else if (entity is NPC npc) {
				ref int staticShockTime = ref npc.GetGlobalNPC<OriginGlobalNPC>().staticShockTime;
				if (staticShockTime > 0) staticShockTime--;
				if (staticShockTime > 0 && (npc.wet || npc.ModNPC is IDefiledEnemy)) staticShockTime--;
				if (staticShockTime <= 0) {
					staticShockTime = 20;
					entityWet = npc.wet || npc.ModNPC is IDefiledEnemy;
					entityHitbox = npc.Hitbox;
					if (npc.type == NPCID.TargetDummy || !npc.friendly) {
						foreach (NPC other in Main.ActiveNPCs) {
							if (other == npc || other.buffImmune[damageDebuffID] || (other.dontTakeDamage && !other.ShowNameOnHover)) continue;
							if ((other.type == NPCID.TargetDummy || !other.friendly) && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
								other.AddBuff(damageDebuffID, 20);
							}
						}
					} else {
						foreach (Player other in Main.ActivePlayers) {
							if (other.buffImmune[damageDebuffID]) continue;
							if (ShouldInflict(other.Hitbox, other.wet)) other.AddBuff(damageDebuffID, 20);
						}
						foreach (NPC other in Main.ActiveNPCs) {
							if (other == npc || other.buffImmune[damageDebuffID] || (other.dontTakeDamage && !other.ShowNameOnHover)) continue;
							if (!(other.type == NPCID.TargetDummy || other.friendly) && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
								other.AddBuff(damageDebuffID, 20);
							}
						}
					}
				}
			}
		}
		public static IEnumerable<Entity> GetValidChainTargets(Entity entity, float baseRange = 5, bool doArc = true) {
			Rectangle entityHitbox;
			bool entityWet;
			bool ShouldInflict(Rectangle hitbox, bool wet) {
				Vector2 pointA = hitbox.Center().Clamp(entityHitbox);
				Vector2 pointB = entityHitbox.Center().Clamp(hitbox);
				if (pointA.IsWithin(pointB, 16 * baseRange * ((entityWet || wet) ? 2 : 1))) {
					if (!Main.dedServ && doArc && Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), pointA, pointB)) {
						Dust.NewDustPerfect(pointA, ModContent.DustType<Static_Shock_Arc_Dust>(), pointB);
						SoundEngine.PlaySound(Origins.Sounds.LittleZap, (pointA + pointB) * 0.5f);
					}
					return true;
				}
				return false;
			}
			if (entity is Player player) {
				entityWet = player.wet;
				entityHitbox = player.Hitbox;
				for (int i = 0; i < Main.player.Length; i++) {
					Player other = Main.player[i];
					if (!other.active) continue;
					if (other == player) continue;
					if (ShouldInflict(other.Hitbox, other.wet)) yield return other;
				}
				for (int i = 0; i < Main.npc.Length; i++) {
					NPC other = Main.npc[i];
					if (other.dontTakeDamage && !other.ShowNameOnHover) continue;
					if (other.friendly && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) yield return other;
				}
			} else if (entity is NPC npc) {
				entityWet = npc.wet || npc.ModNPC is IDefiledEnemy;
				entityHitbox = npc.Hitbox;
				if (npc.type == NPCID.TargetDummy || !npc.friendly) {
					for (int i = 0; i < Main.npc.Length; i++) {
						NPC other = Main.npc[i];
						if (other == npc || (other.dontTakeDamage && !other.ShowNameOnHover)) continue;
						if ((other.type == NPCID.TargetDummy || !other.friendly) && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) yield return other;
					}
				} else {
					for (int i = 0; i < Main.player.Length; i++) {
						Player other = Main.player[i];
						if (ShouldInflict(other.Hitbox, other.wet)) yield return other;
					}
					for (int i = 0; i < Main.npc.Length; i++) {
						NPC other = Main.npc[i];
						if (other == npc || (other.dontTakeDamage && !other.ShowNameOnHover)) continue;
						if (!(other.type == NPCID.TargetDummy || other.friendly) && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) yield return other;
					}
				}
			}
		}
	}
	public class Static_Shock_Damage_Debuff : ModBuff {
		public override string Texture => typeof(Static_Shock_Debuff).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.GrantImmunityWith[Type] = [
				Static_Shock_Debuff.ID
			];
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().staticShockDamage = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().staticShockDamage = true;
		}
	}
	public class Mini_Static_Shock_Debuff : Static_Shock_Debuff {
		public override string Texture => typeof(Static_Shock_Debuff).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			BuffID.Sets.GrantImmunityWith[Type] = [
				Static_Shock_Debuff.ID
			];
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().miniStaticShock = true;
			if (player.HasBuff<Static_Shock_Debuff>()) player.buffTime[buffIndex] = 2;
			else DoDust(player);
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().miniStaticShock = true;
			if (npc.HasBuff<Static_Shock_Debuff>()) npc.buffTime[buffIndex] = 2;
			else DoDust(npc);
		}
	}
	public class Static_Shock_Arc_Dust : ModDust {
		public override string Texture => "Terraria/Images/Item_1";
		public override void OnSpawn(Dust dust) {
			dust.alpha = 0;
		}
		public override bool Update(Dust dust) {
			dust.alpha++;
			if (dust.alpha > 7) dust.active = false;
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return false;
		}
		public override bool PreDraw(Dust dust) {
			Main.spriteBatch.DrawLightningArcBetween(dust.position - Main.screenPosition, dust.velocity - Main.screenPosition, Main.rand.NextFloat(-4, 4));
			return false;
		}
	}
}
