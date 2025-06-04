using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Aetherite;
using Origins.Items.Pets;
using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Aetherite_Crystal : ModItem /*, ICustomWikiStat, ICustomPetFrames, IJournalEntrySource<Blockus_Tube_Entry> */ {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		public string[] Categories => [
			"Pet"
		];
		public override string Texture => $"Terraria/Images/Item_{ItemID.CrystalShard}";
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Master;
			Item.master = true;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(Item.buffType, 3600);
			}
		}
		/*public IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites => [
			("", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Juvenile_Amalgamation).GetDefaultTMLName(), AssetRequestMode.ImmediateLoad).Value, Main.projFrames[projectileID], 5)),
		];*/
	}
	public class Aetherium_Construct : ModProjectile {
		public override string Texture => $"Terraria/Images/NPC_{NPCID.ServantofCthulhu}";
		public override void SetStaticDefaults() {
			Aetherite_Crystal.projectileID = Projectile.type;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 2;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.timeLeft = 5;
			Projectile.width = 20;
			Projectile.height = 30;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		public override void AI() {
			if (Main.rand.NextBool(650) && Projectile.ai[1] == 0) Projectile.ai[1] = 1;
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Aetherite_Crystal.buffID);
			}
			if (player.HasBuff(Aetherite_Crystal.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}


			#endregion
			#region Movement

			float speed;
			float inertia;
			if (distanceToIdlePosition > 600f) {
				speed = 16f;
				inertia = 36f;
			} else {
				speed = 6f;
				inertia = 48f;
			}
			if (Projectile.ai[0] > 0) {
				speed /= 2;
				inertia /= 2;
			}
			if (distanceToIdlePosition > 12f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			} else if (Projectile.velocity != Vector2.Zero) {
				Projectile.velocity *= 0.95f;
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
			int velDir = Math.Sign(Projectile.velocity.X);
			if (velDir == 0) {
				Projectile.spriteDirection = player.direction;
			} else {
				Projectile.spriteDirection = velDir;
			}

			// Some visuals here
			if (Projectile.ai[1] == 1 && Projectile.ai[0] == 0) {
				if (MathUtils.LinearSmoothing(ref Projectile.ai[2], 1f, 1 / 60f)) Projectile.ai[0] = 1;
			} else if (Projectile.ai[1] == 1 && ++Projectile.ai[0] > 600) {
				if (MathUtils.LinearSmoothing(ref Projectile.ai[2], 0, 1 / 60f)) {
					Projectile.ai[1] = Projectile.ai[0] = 0;
				}
			}
			if (!Main.gamePaused) {
				Texture2D circle = TextureAssets.Projectile[ModContent.ProjectileType<Aetherite_Aura_P>()].Value;
				SC_Phase_Three_Overlay.drawDatas.Add(new(
					circle,
					Projectile.position - Main.screenPosition,
					null,
					Color.White
				) {
					origin = circle.Size() * 0.5f,
					scale = Vector2.One * Projectile.ai[2]
				});
			}
			#endregion
		}
	}/*
	public class Aetherite_Crystal_Entry : JournalEntry {
		public override JournalSortIndex SortIndex => new("The_Defiled", 18);
	}*/
}

namespace Origins.Buffs {
	public class Aetherite_Crystal_Buff : ModBuff {
		public override string Texture => $"Terraria/Images/Item_{ItemID.CrystalShard}";
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			Aetherite_Crystal.buffID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;

			int projType = Aetherite_Crystal.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				var entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
	}
}