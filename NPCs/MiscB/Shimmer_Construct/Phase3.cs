using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;
using Terraria.ModLoader;
using MonoMod.Cil;
using static Origins.NPCs.Defiled.Boss.DA_Body_Part;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseThreeIdleState : AIState {
		public static List<AIState> aiStates = [];
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => boss.IsInPhase3.Mul(3)));
		}
		public override void SetStaticDefaults() {

		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.TargetClosest();
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > (60 - ContentExtensions.DifficultyDamageMultiplier * 10) && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
		}
		public override void TrackState(int[] previousStates) { }
	}
	public class Weak_Shimmer_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Shimmer;
		public static int ID { get; private set; }
		public override void Load() {
			On_Player.ShimmerCollision += (On_Player.orig_ShimmerCollision orig, Player self, bool fallThrough, bool ignorePlats, bool noCollision) => {
				if (self.OriginPlayer().weakShimmer) {
					self.position += self.velocity * new Vector2(0.75f, 0.5f);
				} else {
					orig(self, fallThrough, ignorePlats, noCollision);
				}
			};
			On_Player.TryLandingOnDetonator += (orig, self) => {
				orig(self);
				if (self.OriginPlayer().weakShimmer) {
					Collision.up = false;
					Collision.down = false;
				}
			};
			try {
				IL_Projectile.HandleMovement += IL_Projectile_HandleMovement;
			} catch (Exception ex) {
				if (Origins.LogLoadingILError(nameof(IL_Projectile_HandleMovement), ex)) throw;
			}
		}
		static void IL_Projectile_HandleMovement(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdfld<Projectile>(nameof(Projectile.tileCollide))
			);
			c.EmitLdarg0();
			c.EmitDelegate((bool tileCollide, Projectile projectile) => tileCollide && (!projectile.friendly || !projectile.TryGetOwner(out Player player) || !player.OriginPlayer().weakShimmer));
		}

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.mount?.Active == true) {
				Mount mount = player.mount;
				player.ClearBuff(mount._data.buff);

				mount._mountSpecificData = null;

				if (mount.Cart) {
					player.ClearBuff(mount._data.extraBuff);
					player.cartFlip = false;
					player.lastBoost = Vector2.Zero;
				}

				player.fullRotation = 0f;
				player.fullRotationOrigin = Vector2.Zero;

				mount.Reset();
				player.position.Y += player.height;
				player.height = 42;
				player.position.Y -= player.height;
				if (player.whoAmI == Main.myPlayer)
					NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
			}
			player.buffImmune[BuffID.Shimmer] = true;
			player.shimmering = true;
			player.OriginPlayer().weakShimmer = true;
			player.fallStart = (int)(player.position.Y / 16f);
			if (player.buffTime[buffIndex] > 2) {
				player.timeShimmering = 0;
			} else {
				bool isBlocked = false;
				for (int i = (int)(player.position.X / 16f); i <= (player.position.X + player.width) / 16; i++) {
					for (int j = (int)(player.position.Y / 16f); j <= (player.position.Y + player.height) / 16; j++) {
						if (WorldGen.SolidTile3(i, j))
							isBlocked = true;
					}
				}

				if (isBlocked) {
					player.buffTime[buffIndex]++;
				} else {
					player.DelBuff(buffIndex--);
				}
			}
		}
	}
}
