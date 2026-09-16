using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Pets;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Dysfunctional_Table_Saw : ModItem {
		static AutoLoadingAsset<Texture2D> useTexture = typeof(Dysfunctional_Table_Saw).GetDefaultTMLName("_Use");
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToVanitypet(ModContent.ProjectileType<Sprockey>(), ModContent.BuffType<Sprockey_Buff>());
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 7, silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		static bool DuringOrAfterSlam(Player player, int offset = 0) => (player.itemAnimation + offset) / (float)player.itemAnimationMax <= 0.1f;
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.ItemTimeIsZero) player.ApplyItemTime(Item, 1f, false);
			else if (DuringOrAfterSlam(player) && !DuringOrAfterSlam(player, 1)) {
				Vector2 slamPos = player.MountedCenter + new Vector2(player.direction * 40, player.gravDir * 6);
				SoundEngine.PlaySound(Item.UseSound, slamPos);
				if (!player.IsLocallyOwned()) return;
				if (player.ownedProjectileCounts[Item.shoot] > 0) {
					foreach (Projectile proj in Main.ActiveProjectiles) {
						if (proj.IsLocallyOwned() && proj.type == Item.shoot) proj.Kill();
					}
				}
				Projectile.NewProjectile(
					player.GetSource_ItemUse(Item),
					slamPos,
					new Vector2(player.direction * 8, player.gravDir * -8) + Main.rand.NextVector2Circular(1, 1),
					Item.shoot,
					0,
					0
				);
				player.AddBuff(Item.buffType, 3600);
			}
		}

		public override bool ModifyItemDraw(ref PlayerDrawSet drawInfo, ref DrawData drawData, ref DrawData? coloredDrawData, ref DrawData? glowMaskDrawData) {
			drawData.texture = useTexture;
			if (drawData.origin.X != 0) drawData.origin.X = drawData.texture.Width;
			drawData.origin.Y = drawData.texture.Height / 2;
			drawData.sourceRect = null;
			drawData.rotation -= MathHelper.PiOver4 * (drawInfo.drawPlayer.direction * drawInfo.drawPlayer.gravDir);
			drawData.effect ^= SpriteEffects.FlipVertically;
			return base.ModifyItemDraw(ref drawInfo, ref drawData, ref coloredDrawData, ref glowMaskDrawData);
		}
	}
	public class Sprockey : ModProjectile {
		public override void SetStaticDefaults() {
			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.LightPet[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Main.projFrames[Projectile.type] = 1;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() => false;

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			int buffType = ModContent.BuffType<Sprockey_Buff>();
			if (player.dead || !player.active) {
				player.ClearBuff(buffType);
			} else if (player.HasBuff(buffType)) {
				Projectile.timeLeft = 2;
			}
			#endregion
			#region General behavior
			Vector2 idlePosition = player.MountedCenter;
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			if (Main.myPlayer == player.whoAmI && !vectorToIdlePosition.WithinRange(Vector2.Zero, 2000f)) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}


			#endregion

			#region Movement
			switch ((int)Projectile.ai[0]) {
				case 1: {
					if (vectorToIdlePosition.WithinRange(Vector2.Zero, 16) && !Projectile.Hitbox.OverlapsAnyTiles()) {
						Rectangle floorbox = Projectile.Hitbox;
						floorbox.Offset(0, Projectile.height);
						floorbox.Height = 16 * 4;
						if (floorbox.OverlapsAnyTiles(false)) {
							Projectile.ai[0] = 0;
							goto default;
						}
					}
					Projectile.tileCollide = false;

					float speed;
					float inertia;
					if (!vectorToIdlePosition.WithinRange(Vector2.Zero, 600)) {
						speed = 16f;
						inertia = 32f;
					} else {
						speed = 6f;
						inertia = 24f;
					}
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition.Normalized(out _) * speed) / inertia;

					Projectile.rotation += (MathHelper.Pi / Projectile.width) * Projectile.direction * Projectile.velocity.Length();
					break;
				}

				default: {
					if (vectorToIdlePosition.Y.Abs(out int yDir) >= 16 * 20 && (yDir == -1 || Projectile.velocity.X.Abs(out _) < 1)) {
						Projectile.ai[0] = 1;
						goto case 1;
					}
					Projectile.tileCollide = true;

					float dist = vectorToIdlePosition.X.Abs(out int dir);
					float lerpValue = Utils.GetLerpValue(16, 64, dist, true);
					if (dist > 16) {
						Projectile.velocity.X += dir * lerpValue * 0.1f;
					}
					Projectile.velocity.X *= float.Lerp(0.93f, 0.99f, lerpValue);
					Projectile.velocity.Y += 0.3f;
					Projectile.rotation += (MathHelper.Pi / Projectile.width) * Projectile.velocity.X;

					if (Projectile.velocity.Y >= 0f) {
						Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
						Collision.StepDown(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
					}
					break;
				}
			}
			#endregion
		}
	}
}

namespace Origins.Buffs {
	public class Sprockey_Buff : ModBuff {
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.lightPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;

			int projType = ModContent.ProjectileType<Sprockey>();

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				Terraria.DataStructures.IEntitySource entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
	}
}