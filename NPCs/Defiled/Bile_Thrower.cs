using Microsoft.Xna.Framework;
using Origins.Projectiles.Enemies;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.NPCs.Defiled {
	public class Bile_Thrower : Glowing_Mod_NPC, IDefiledEnemy {
		public int MaxMana => 40;
		public int MaxManaDrain => 5;
		public float Mana {
			get => NPC.localAI[2];
			set => NPC.localAI[2] = value;
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.CorruptPenguin);
			AnimationType = NPCID.CorruptPenguin;
			AIType = NPCID.CorruptPenguin;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Defiled_Wastelands>().Type
			};
        }
		public override void AI() {
			NPCAimedTarget target = NPC.GetTargetData(false);
			if (!target.Invalid && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, target.Position, target.Width, target.Height)) {
				Vector2 targetPos = target.Hitbox.Center.ToVector2();
				Vector2 spawnPos = NPC.Top;
				spawnPos.Y += 6;
				Vector2 diff = targetPos - spawnPos;
				float distSQ = diff.LengthSquared();
				if (++NPC.ai[2] > 300 && distSQ > 0 && distSQ < 480 * 480) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						diff = Main.rand.NextVector2FromRectangle(target.Hitbox) - spawnPos;
						const float speed = 6;
						if (GeometryUtils.AngleToTarget(diff, speed, grav: 0.08f, false) is float angle) {
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								spawnPos,
								new Vector2(speed, 0).RotatedBy(angle),
								ModContent.ProjectileType<Defiled_Goop>(),
								12,
								3,
								Main.maxPlayers
							);
						}
					}
					NPC.ai[2] = 0;
				} else if (NPC.ai[2] > 300 - 36) {
					NPC.velocity.X *= 0.93f;
				}
			} else if (NPC.ai[2] > 0) {
				NPC.ai[2]--;
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                this.GetBestiaryFlavorText(),
            });
		}
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
            for (int i = Main.rand.Next(3); i-- > 0;) Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4)));
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
            int halfWidth = NPC.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for (int i = Main.rand.Next(3); i-- > 0;) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4)));
        }
        public override void HitEffect(NPC.HitInfo hit) {
            if (NPC.life < 0) {
                for (int i = 0; i < 6; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for (int i = 0; i < 10; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4)));
            }
        }
    }
}
