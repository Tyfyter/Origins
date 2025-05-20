using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Summoner;
using Origins.Journal;
using Origins.NPCs;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using ThoriumMod.Items.Donate;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front)]
	public class Lazy_Cloak : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Lazy_Cloak_Entry).Name;
		public class Lazy_Cloak_Entry : JournalEntry {
			public override string TextKey => "Lazy_Cloak";
			public override JournalSortIndex SortIndex => new("Arabel", 4);
		}
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 36);
			Item.damage = 10;
			Item.DamageType = DamageClass.Summon;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.shoot = ModContent.ProjectileType<Lazy_Cloak_P>();
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Orange;
			Item.backSlot = 5;
			Item.hasVanityEffects = true;
			Item.buffType = Lazy_Cloak_Buff.ID;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (player.ownedProjectileCounts[Item.shoot] < 1) {
				player.SpawnMinionOnCursor(player.GetSource_Accessory(Item), player.whoAmI, Item.shoot, Item.damage, Item.knockBack, player.MountedCenter - Main.MouseWorld);
			}
			player.AddBuff(Item.buffType, 5);
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (type == EquipType.Front && originPlayer.lazyCloakOffPlayer > 0) {
				player.front = -1;
				player.back = -1;
			}
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			return OriginExtensions.AccessoryOrSpecialPrefix(Item, rand, PrefixCategory.AnyWeapon, PrefixCategory.Magic);
		}
	}
	public class Lazy_Cloak_P : ModProjectile, IShadedProjectile {
		public const int frameSpeed = 5;
		public static int ID { get; private set; }
		public int Shader => Main.player[Projectile.owner].cFront;
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 2;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 40;
			Projectile.height = 28;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			Projectile.netImportant = true;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}


		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Lazy_Cloak_Buff.ID);
			}
			if (player.HasBuff(Lazy_Cloak_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.MountedCenter;

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

			#region Find target
			// Starting search distance
			float distanceFromTarget = 2000f * 2000f;
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget) return;
				float between = Vector2.DistanceSquared(npc.Center, Projectile.Center);
				if (between < distanceFromTarget) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
					target = npc.whoAmI;
					foundTarget = true;
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			/*if (!foundTarget) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 100f;
						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.height / (float)npc.width > 1 ? npc.Top + new Vector2(0, 8) : npc.Center;
							target = npc.whoAmI;
							foundTarget = true;
						}
					}
				}
			}*/

			Projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 12f;
			float inertia = 16f;

			if (foundTarget) {
				Projectile.hide = false;
				Projectile.ai[0] = 1;

				Vector2 direction = targetCenter - Projectile.Center;
				direction.Normalize();
				direction *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;

				player.OriginPlayer().lazyCloakOffPlayer = 2;
			} else {
				if (distanceToIdlePosition > 600f) {
					speed = 24f;
					inertia = 12f;
				} else {
					speed = 12f;
					inertia = 12f;
				}
				if (distanceToIdlePosition > 12f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				} else {
					Projectile.ai[0] = 0;
				}
				if (Projectile.ai[0] == 0) {
					Projectile.hide = true;
					Projectile.position = idlePosition;
				} else {
					player.OriginPlayer().lazyCloakOffPlayer = 2;
				}
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;
			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);

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
			#endregion
		}
		public override void PostDraw(Color lightColor) {
			if (ModContent.RequestIfExists(GlowTexture, out Asset<Texture2D> glowTexture, AssetRequestMode.ImmediateLoad)) {
				SpriteEffects dir = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				int num137 = 0;
				int num136 = 0;
				float num138 = 0;
				ProjectileLoader.DrawOffset(Projectile, ref num137, ref num136, ref num138);
				int num408 = glowTexture.Height() / Main.projFrames[Projectile.type];
				int y27 = num408 * Projectile.frame;
				Main.EntitySpriteDraw(glowTexture.Value,
					Projectile.position + new Vector2(num138 + (float)num137, (Projectile.height / 2) + Projectile.gfxOffY) - Main.screenPosition,
					new Rectangle(0, y27, TextureAssets.Projectile[Projectile.type].Width(), num408 - 1),
					new Color(250, 250, 250, Projectile.alpha),
					Projectile.rotation,
					new Vector2(num138, Projectile.height / 2 + num136),
					Projectile.scale,
					dir
				);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.player[Projectile.owner].IsWithinRectangular(target, new Vector2(16 * 4, 16 * 2))) {
				target.AddBuff(Lazy_Cloak_Buff.ID, 10);
				target.DoCustomKnockback(Vector2.UnitY * Main.player[Projectile.owner].GetTotalKnockback(DamageClass.Summon).ApplyTo(4));
			}
		}
	}
}
namespace Origins.Buffs {
	public class Lazy_Cloak_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override void Load() {
			try {
				IL_NPC.UpdateNPC_Inner += DoLazyCloakShimmer;
			} catch (Exception ex) {
				if (Origins.LogLoadingILError(nameof(DoLazyCloakShimmer), ex)) throw;
			}
		}
		static void DoLazyCloakShimmer(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt<NPC>("UpdateCollision"));

			c.GotoPrev(MoveType.After, i => i.MatchLdfld<NPC>(nameof(NPC.noTileCollide)));
			c.EmitLdarg0();
			c.EmitDelegate((bool noTileCollide, NPC npc) => noTileCollide || (npc.TryGetGlobalNPC(out OriginGlobalNPC global) && global.lazyCloakShimmer));

			c.GotoNext(MoveType.After, i => i.MatchLdfld<Entity>(nameof(Entity.velocity)));
			c.EmitLdarg0();
			c.EmitDelegate((Vector2 velocity, NPC npc) => velocity * (npc.TryGetGlobalNPC(out OriginGlobalNPC global) && global.lazyCloakShimmer ? 0.375f : 1));
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			bool foundAny = false;
			foreach (int proj in ProjectileTypes()) {
				if (player.ownedProjectileCounts[proj] > 0) foundAny = true;
			}
			if (!foundAny) {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().lazyCloakShimmer = true;
		}

		public override IEnumerable<int> ProjectileTypes() => [
			Lazy_Cloak_P.ID
		];
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) { }
	}
}