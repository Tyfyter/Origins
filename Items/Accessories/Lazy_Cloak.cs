using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Summoner;
using Origins.Journal;
using Origins.Layers;
using Origins.NPCs;
using Origins.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front, EquipType.Back)]
	public class Lazy_Cloak : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Lazy_Cloak_Entry).Name;
		public class Lazy_Cloak_Entry : JournalEntry {
			public override string TextKey => "Lazy_Cloak";
			public override JournalSortIndex SortIndex => new("Arabel", 4);
		}
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Back_Glow_Layer>(Item.backSlot, Texture + "_Back_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 36);
			Item.damage = 18;
			Item.DamageType = DamageClass.Summon;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.shoot = Lazy_Cloak_P.ID;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.hasVanityEffects = true;
			Item.buffType = Lazy_Cloak_Buff.ID;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (player.ownedProjectileCounts[Item.shoot] < 1) {
				player.SpawnMinionOnCursor(player.GetSource_Accessory(Item), player.whoAmI, Item.shoot, Item.damage, Item.knockBack, player.MountedCenter - Main.MouseWorld);
			}
			player.AddBuff(Item.buffType, 5);
			player.OriginPlayer().lazyCloakHidden = hideVisual;
		}
		public override void UpdateVanity(Player player) => player.OriginPlayer().lazyCloakHidden = false;
		public override void EquipFrameEffects(Player player, EquipType type) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (type == EquipType.Front && !originPlayer.lazyCloakHidden && player.front == Item.frontSlot && originPlayer.lazyCloaksOffPlayer[Item.frontSlot] > 0) {
				player.front = -1;
				player.back = -1;
			}
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			return OriginExtensions.AccessoryOrSpecialPrefix(Item, rand, PrefixCategory.AnyWeapon, PrefixCategory.Magic);
		}
	}
	public class Lazy_Cloak_P : SpeedModifierMinion, IShadedProjectile {
		public const int frameSpeed = 5;
		public static int ID { get; private set; }
		public virtual int BuffID => Lazy_Cloak_Buff.ID;
		public int Shader => Main.player[Projectile.owner].cFront;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 2;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			if (GetType().GetProperty(nameof(ID)).GetSetMethod(true) is MethodInfo setID) setID.Invoke(null, [Type]);
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

		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) Projectile.ai[1] = itemUse.Item.frontSlot;
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
				player.ClearBuff(BuffID);
			}
			if (player.HasBuff(BuffID)) {
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

				player.OriginPlayer().lazyCloaksOffPlayer[(int)Projectile.ai[1]] = 2;
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
					vectorToIdlePosition *= speed * SpeedModifier;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				} else {
					Projectile.ai[0] = 0;
				}
				if (Projectile.ai[0] == 0) {
					Projectile.hide = true;
					Projectile.position = idlePosition;
				} else {
					player.OriginPlayer().lazyCloaksOffPlayer[(int)Projectile.ai[1]] = 3;
				}
			}

			for (int i = Main.rand.RandomRound(SpeedModifier - 1); i > 0; i--) {
				for (int j = 0; j < Projectile.localNPCImmunity.Length; j++) {
					Projectile.localNPCImmunity[j].Cooldown();
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
		bool requestedGlow;
		Asset<Texture2D> glowTexture;
		public override bool PreDraw(ref Color lightColor) {
			if (!requestedGlow) {
				requestedGlow = true;
				ModContent.RequestIfExists(GlowTexture, out glowTexture, AssetRequestMode.ImmediateLoad);
			}
			SpriteEffects dir = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			int offsetX = 0;
			int offsetY = 0;
			float originX = 0;
			ProjectileLoader.DrawOffset(Projectile, ref offsetX, ref offsetY, ref originX);
			int frameHeight = TextureAssets.Projectile[Type].Height() / Main.projFrames[Projectile.type];
			Vector2 origin = new(originX, Projectile.height / 2 + offsetY);
			Rectangle sourceRectangle = new(0, frameHeight * Projectile.frame, TextureAssets.Projectile[Projectile.type].Width(), frameHeight - 1);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.position + new Vector2(originX + offsetX, (Projectile.height / 2) + Projectile.gfxOffY) - Main.screenPosition,
				sourceRectangle,
				lightColor,
				Projectile.rotation,
				origin,
				Projectile.scale,
				dir
			);
			if (glowTexture is not null) {
				Main.EntitySpriteDraw(
					glowTexture.Value,
					Projectile.position + new Vector2(originX + offsetX, (Projectile.height / 2) + Projectile.gfxOffY) - Main.screenPosition,
					sourceRectangle,
					new Color(250, 250, 250, Projectile.alpha),
					Projectile.rotation,
					origin,
					Projectile.scale,
					dir
				);
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.player[Projectile.owner].IsWithinRectangular(target, new Vector2(16 * 4, 16 * 2))) {
				target.AddBuff(ModContent.BuffType<Lazy_Cloak_Buff>(), 10);
				target.DoCustomKnockback(Vector2.UnitY * Main.player[Projectile.owner].GetTotalKnockback(DamageClass.Summon).ApplyTo(4));
			}
		}
	}
}
namespace Origins.Buffs {
	public class Lazy_Cloak_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override void Load() {
			if (GetType() != typeof(Lazy_Cloak_Buff)) return;
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
			if (GetType().GetProperty(nameof(ID)).GetSetMethod(true) is MethodInfo setID) setID.Invoke(null, [Type]);
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