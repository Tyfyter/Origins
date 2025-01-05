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
			player.GetModPlayer<OriginPlayer>().staticShock = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().staticShock = true;
		}
		public static void ProcessShocking(Entity entity) {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (entity is Player player) {
				ref int staticShockTime = ref player.OriginPlayer().staticShockTime;
				if (staticShockTime > 0) staticShockTime--;
				if (staticShockTime > 0 && player.wet) staticShockTime--;
				if (staticShockTime <= 0) {
					staticShockTime = 60;
					Player.HurtInfo hurt = new() {
						DamageSource = PlayerDeathReason.ByOther(10),
						Damage = 3,
						Dodgeable = false,
						CooldownCounter = ImmunityCooldownID.WrongBugNet,
						SoundDisabled = true,
						Knockback = 0
					};
					NPC.HitInfo hit = new() {
						Damage = 3,
						DamageType = DamageClass.Default,
						Knockback = 0
					};
					bool WithinRange(Rectangle hitbox, bool wet) {
						return hitbox.Center().Clamp(player.Hitbox).IsWithin(player.MountedCenter.Clamp(hitbox), 16 * ((player.wet || wet) ? 10 : 5));
					}
					foreach (Player other in Main.ActivePlayers) {
						if (WithinRange(other.Hitbox, other.wet)) other.Hurt(hurt);
					}
					foreach (NPC other in Main.ActiveNPCs) {
						if (!other.CanBeChasedBy(player) && WithinRange(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
							other.StrikeNPC(hit);
							NetMessage.SendStrikeNPC(other, hit);
						}
					}
				}
			} else if (entity is NPC npc) {
				ref int staticShockTime = ref npc.GetGlobalNPC<OriginGlobalNPC>().staticShockTime;
				if (staticShockTime > 0) staticShockTime--;
				if (staticShockTime > 0 && (npc.wet || npc.ModNPC is IDefiledEnemy)) staticShockTime--;
				if (staticShockTime <= 0) {
					staticShockTime = 60;
					bool isWet = npc.wet || npc.ModNPC is IDefiledEnemy;
					bool WithinRange(Rectangle hitbox, bool wet) {
						return hitbox.Center().Clamp(npc.Hitbox).IsWithin(npc.Center.Clamp(hitbox), 16 * ((isWet || wet) ? 10 : 5));
					}
					NPC.HitInfo hit = new() {
						Damage = 3,
						DamageType = DamageClass.Default,
						Knockback = 0
					};
					if (npc.type == NPCID.TargetDummy || npc.CanBeChasedBy()) {
						foreach (NPC other in Main.ActiveNPCs) {
							if ((npc.type == NPCID.TargetDummy || other.CanBeChasedBy()) && WithinRange(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
								other.StrikeNPC(hit);
								NetMessage.SendStrikeNPC(other, hit);
							}
						}
					} else {
						Player.HurtInfo hurt = new() {
							DamageSource = PlayerDeathReason.ByOther(10),
							Damage = 3,
							Dodgeable = false,
							CooldownCounter = ImmunityCooldownID.WrongBugNet,
							SoundDisabled = true,
							Knockback = 0
						};
						foreach (Player other in Main.ActivePlayers) {
							if (WithinRange(other.Hitbox, other.wet)) other.Hurt(hurt);
						}
						foreach (NPC other in Main.ActiveNPCs) {
							if (!(npc.type == NPCID.TargetDummy || other.CanBeChasedBy()) && WithinRange(other.Hitbox, other.wet || other.ModNPC is IDefiledEnemy)) {
								other.StrikeNPC(hit);
								NetMessage.SendStrikeNPC(other, hit);
							}
						}
					}
				}
			}
		}
	}
}
