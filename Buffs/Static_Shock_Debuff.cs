using Origins.NPCs;
using Origins.NPCs.Defiled;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Static_Shock_Debuff : ModBuff {
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().staticShock = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().staticShock = true;
		}
		public static void ProcessShocking(Entity entity) {
			int damageDebuffID = ModContent.BuffType<Static_Shock_Damage_Debuff>();
			Rectangle entityHitbox;
			bool entityWet;
			bool ShouldInflict(Rectangle hitbox, bool wet) {
				Vector2 pointA = hitbox.Center().Clamp(entityHitbox);
				Vector2 pointB = entityHitbox.Center().Clamp(hitbox);
				if (pointA.IsWithin(pointB, 16 * ((entityWet || wet) ? 10 : 5))) {
					if (!Main.dedServ) Dust.NewDustPerfect(pointA, ModContent.DustType<Static_Shock_Arc_Dust>(), pointB);
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
						if (other == player) continue;
						if (ShouldInflict(other.Hitbox, other.wet)) other.AddBuff(damageDebuffID, 20);
					}
					foreach (NPC other in Main.ActiveNPCs) {
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
							if (other == npc) continue;
							if ((other.type == NPCID.TargetDummy || !other.friendly) && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
								other.AddBuff(damageDebuffID, 20);
							}
						}
					} else {
						foreach (Player other in Main.ActivePlayers) {
							if (ShouldInflict(other.Hitbox, other.wet)) other.AddBuff(damageDebuffID, 20);
						}
						foreach (NPC other in Main.ActiveNPCs) {
							if (other == npc) continue;
							if (!(other.type == NPCID.TargetDummy || other.friendly) && ShouldInflict(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
								other.AddBuff(damageDebuffID, 20);
							}
						}
					}
				}
			}
		}
	}
	public class Static_Shock_Damage_Debuff : ModBuff {
		public override string Texture => typeof(Static_Shock_Debuff).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().staticShockDamage = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().staticShockDamage = true;
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
