using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
	public class Debug_Shoot_NPC : ModNPC {
		public override string Texture => "Terraria/Images/NPC_" + NPCID.TargetDummy;
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void SetDefaults() {
			NPC.width = 18;
			NPC.height = 40;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.knockBackResist = 0.5f;
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.HitSound = SoundID.NPCHit26;
			NPC.DeathSound = SoundID.NPCDeath29;
		}
		public override void AI() {
			if (!NetmodeActive.MultiplayerClient && ++NPC.ai[0] > 240) {
				Player nearest = null;
				float dist = float.PositiveInfinity;
				foreach (Player realPlayer in Main.ActivePlayers) {
					float d = NPC.DistanceSQ(realPlayer.Center);
					if (dist > d) {
						dist = d;
						nearest = realPlayer;
					}
				}
				if (nearest?.HeldItem?.IsAir ?? true) return;
				NPC.ai[0] = 0;
				Player player = Main.player[Main.maxPlayers];
				(int mouseX, int mouseY) = (Main.mouseX, Main.mouseY);
				try {
					(Main.mouseX, Main.mouseY) = (nearest.Center - Main.screenPosition).ToPoint();
					if (CombinedHooks.CanShoot(player, nearest.HeldItem)) {
						Vector2 position = NPC.Center;
						EntitySource_ItemUse_WithAmmo projectileSource = new(player, nearest.HeldItem, ItemID.None);
						Vector2 direction = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - position;
						if (player.gravDir == -1f) direction.Y = (Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - position.Y;

						Vector2 velocity = Vector2.Normalize(direction);
						if (velocity.HasNaNs()) velocity = -Vector2.UnitY;
						velocity *= nearest.HeldItem.shootSpeed;
						int projToShoot = nearest.HeldItem.shoot;
						int damage = nearest.HeldItem.damage;
						float knockBack = nearest.HeldItem.knockBack;
						CombinedHooks.ModifyShootStats(player, nearest.HeldItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockBack);
						if (CombinedHooks.Shoot(player, nearest.HeldItem, projectileSource, position, velocity, projToShoot, damage, knockBack)) {
							Projectile.NewProjectile(projectileSource, position, velocity, projToShoot, damage, knockBack);
						}
					}
				} finally {
					(Main.mouseX, Main.mouseY) = (mouseX, mouseY);
				}
			}
		}
	}
}
