using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Pets;
using Origins.Items.Vanity.Dev;
using Origins.Items.Vanity.Dev.cher;
using Origins.Items.Weapons.Melee;
using Origins.Tiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Items.Pets.Chee_Toy_Message_Types;

namespace Origins.Items.Pets {
	public class Chee_Set : DevSet<Chew_Toy> {
		public override IEnumerable<ItemTypeDropRuleWrapper> GetDrops() {
			yield return ModContent.ItemType<First_Dream>();
			yield return ModContent.ItemType<Chew_Toy>();
			yield return new(ItemDropRule.ByCondition(DropConditions.HardmodeBossBag, ModContent.ItemType<The_Bird>()));
		}
	}
	public class Chew_Toy : ModItem, ICustomWikiStat, ICustomPetFrames {
		public bool? Hardmode => false;
		public string[] Categories => [
			WikiCategories.DeveloperItem
		];
		internal static int projectileID = 0;
		internal static int buffID = 0;
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Cyan;// dev items are cyan rarity, despite being expert exclusive
			Item.buffType = buffID;
			Item.shoot = projectileID;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2); // The item applies the buff, the buff spawns the projectile
			return false;
		}
		public IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites => [
			("", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Chee_Toy).GetDefaultTMLName(), AssetRequestMode.ImmediateLoad).Value, Main.projFrames[projectileID], 4)),
			("_Flying", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Chee_Toy).GetDefaultTMLName() + "_Flying", AssetRequestMode.ImmediateLoad).Value, 4, 4)),
		];
	}
	public class Chee_Toy : ModProjectile {
		public AutoLoadingAsset<Texture2D> flyingTexture = typeof(Chee_Toy).GetDefaultTMLName() + "_Flying";
		public bool OnGround {
			get {
				return Projectile.localAI[1] > 0;
			}
			set {
				Projectile.localAI[1] = value ? 2 : 0;
			}
		}
		public sbyte CollidingX {
			get {
				return (sbyte)Projectile.localAI[0];
			}
			set {
				Projectile.localAI[0] = value;
			}
		}
		public static HashSet<int> dogs = [
			NPCID.TownDog,
			NPCID.Wolf,
			NPCID.Werewolf,
		];
		public override void Unload() => dogs = null;
		public override void SetStaticDefaults() {
			Chew_Toy.projectileID = Type;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Type] = 4;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 48;
			Projectile.height = 32;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
			Projectile.ignoreWater = false;
			DrawOriginOffsetY = -36;
			DrawOffsetX = -24;
			//Projectile.scale = 1.5f;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Chew_Toy.buffID);
			}
			if (player.HasBuff(Chew_Toy.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Bottom;
			idlePosition.X -= 80f * player.direction;
			idlePosition.Y -= 16f * Projectile.scale;

			const float dog_range = 320;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (dogs.Contains(npc.type) && npc.DistanceSQ(idlePosition) < dog_range * dog_range) {
					Chee_Toy_Messages.Instance.PlayRandomMessage(Dog, Projectile.Top);
					Vector2 diff = npc.Center - idlePosition;
					float dist = diff.Length();
					if (dist != 0 && dist < dog_range) {
						idlePosition -= diff.SafeNormalize(default) * (dog_range - dist);
					}
				}
			}

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Projectile.soundDelay == 1) {
				SoundEngine.PlaySound(SoundID.Item29.WithPitch(1f), Projectile.Center);
				Projectile.soundDelay = 0;
			}
			if (Main.myPlayer == player.whoAmI) {
				if (distanceToIdlePosition > 1200f) {
					// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
					// and then set netUpdate to true
					Projectile.position = idlePosition;
					Projectile.velocity *= 0.1f;
					Projectile.netUpdate = true;
					Projectile.soundDelay = 2;
				} else if (distanceToIdlePosition > 300) {
					Projectile.ai[2] = 1;
					Projectile.netUpdate = true;
				}
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

			float speed = 6f;
			float inertia = 12f;
			if (distanceToIdlePosition > 250f) {
				speed = 12f;
			}
			int direction = Math.Sign(vectorToIdlePosition.X);
			Projectile.spriteDirection = direction;
			if (vectorToIdlePosition.Y < 160 && CollidingX == direction && OnGround) {
				float jumpStrength = 6;
				if (Collision.TileCollision(Projectile.position - new Vector2(0, 18), new Vector2(4 * direction, 0), Projectile.width, Projectile.height, false, false).X == 0) {
					jumpStrength += 2;
					if (Collision.TileCollision(Projectile.position - new Vector2(0, 36), new Vector2(4 * direction, 0), Projectile.width, Projectile.height, false, false).X == 0) {
						jumpStrength += 2;
					}
				}
				Projectile.velocity.Y = -jumpStrength;
			}
			if (distanceToIdlePosition > 16f) {
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Vector2 dir = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				Projectile.velocity.X = dir.X;
				if (Projectile.ai[2] == 1) {
					Projectile.velocity.Y = dir.Y;
				}
			} else {
				inertia = 6f;
				Vector2 dir = (Projectile.velocity * (inertia - 1)) / inertia;
				Projectile.velocity.X = dir.X;
				if (Projectile.ai[2] == 1) {
					Projectile.velocity.Y = dir.Y;
					Projectile.ai[2] = 0;
				}
			}

			Projectile.tileCollide = Projectile.ai[2] != 1;
			#endregion

			#region Animation and visuals
			if (Projectile.velocity.LengthSquared() < 1) Projectile.spriteDirection = Projectile.direction = player.direction;
			else Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);
			if (Projectile.ai[2] != 1) {
				Projectile.velocity.Y += 0.4f;
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, 0, 0.35f);
			} else {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver2 * Projectile.direction;
			}

			if (OnGround) {
				Projectile.localAI[1]--;
				const int frameSpeed = 4;
				if (Math.Abs(Projectile.velocity.X) < 0.05f) {
					Projectile.velocity.X = 0f;
				}
				if ((Projectile.velocity.X != 0) ^ (Projectile.oldVelocity.X != 0)) {
					Projectile.frameCounter = 0;
					Projectile.frame = 0;
				}
				if (Projectile.velocity.X != 0) {
					Projectile.frameCounter++;
					if (Projectile.frameCounter >= frameSpeed) {
						Projectile.frameCounter = 0;
						Projectile.frame++;
						if (Projectile.frame >= 4) {
							Projectile.frame = 0;
						}
					}
				}
			} else {
				if (Projectile.ai[2] == 1) {
					int frameSpeed = (int)(24 / (Projectile.velocity.Length() + 1));
					if (frameSpeed > 0 && ++Projectile.frameCounter >= frameSpeed) {
						Projectile.frameCounter = 0;
						if (++Projectile.frame >= 4) {
							Projectile.frame = 0;
						}
					}
				} else {
					Projectile.frame = 2;
				}
			}
			#endregion

			Chee_Toy_Messages.Instance.UpdateMoonlordWarningAndIdle(Projectile);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)Projectile.soundDelay);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.soundDelay = reader.ReadByte();
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (oldVelocity.Y > Projectile.velocity.Y) {
				OnGround = true;
			} else {
				if (Collision.SlopeCollision(Projectile.position, new Vector2(0, 4), Projectile.width, Projectile.height).Y != 4) {
					OnGround = true;
				}
			}
			if (oldVelocity.X > Projectile.velocity.X) {
				CollidingX = (sbyte)(1 - Collision.TileCollision(Projectile.position, Vector2.UnitX, Projectile.width, Projectile.height, false, false).X);
			} else if (oldVelocity.X < Projectile.velocity.X) {
				CollidingX = (sbyte)(-1 - Collision.TileCollision(Projectile.position, -Vector2.UnitX, Projectile.width, Projectile.height, false, false).X);
			} else {
				CollidingX = 0;
			}
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[2] == 1) {
				Main.EntitySpriteDraw(
					flyingTexture,
					Projectile.Center - Main.screenPosition,
					flyingTexture.Frame(verticalFrames: 4, frameY: Projectile.frame),
					lightColor,
					Projectile.rotation,
					new Vector2(57 + 45 * Projectile.direction, 18),
					1,
					Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
				);
				return false;
			}
			return true;
		}
	}
	public class Chee_Toy_Messages : Message_Cache<Chee_Toy_Message_Types> {
		public static Chee_Toy_Messages Instance;
		public override string KeyBase => "Mods.Origins.Dialogue.Chee_Toy";
		public override void StartCooldown(Chee_Toy_Message_Types type) {
			switch (type) {
				case Dog:
				cooldowns[type] = Main.rand.Next(200, 481);
				break;
				case Werewolfified:
				cooldowns[type] = 1;
				break;
				case Combat:
				cooldowns[type] = Main.rand.Next(720, 1801);
				break;
				case Near_Cubekon_Girl:
				cooldowns[type] = Main.rand.Next(1800, 3601);
				break;
				case The_Part_Where_He_Kills_You:
				cooldowns[type] = Main.rand.Next(3600, 7201);
				break;
			}
			cooldowns[Idle] = Main.rand.Next(1800, 3601);
		}
		public void UpdateMoonlordWarningAndIdle(Projectile projectile, bool disableIdle = false) {
			if (cooldowns[The_Part_Where_He_Kills_You] <= 0) {
				int index = NPC.FindFirstNPC(NPCID.MoonLordHead);
				if (index != -1 && Main.npc[index].ai[0] == 1f) { //charging up deathray
					PlayRandomMessage(
						The_Part_Where_He_Kills_You,
						projectile.Top
					);
				}
			}
			if (!disableIdle) {
				PlayRandomMessage(Idle, projectile.Top);
			}
			if (Main.player[projectile.owner].wereWolf) {
				PlayRandomMessage(Werewolfified, projectile.Top);
				cooldowns[Werewolfified]++;
			}
		}
	}
	public enum Chee_Toy_Message_Types {
		Summoned,
		Idle,
		Dog,
		Werewolfified,
		Death,
		Combat,
		Near_Cubekon_Girl, // todo: implement when she's implemented
		The_Part_Where_He_Kills_You
	}
}
namespace Origins.Buffs {
	public class Chee_Toy_Buff : ModBuff {
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			Chew_Toy.buffID = Type;
		}

		public override void Update(Player player, ref int buffIndex) { // This method gets called every frame your buff is active on your player.
			player.buffTime[buffIndex] = 18000;

			int projType = Chew_Toy.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				IEntitySource entitySource = player.GetSource_Buff(buffIndex);

				Projectile proj = Projectile.NewProjectileDirect(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
				Chee_Toy_Messages.Instance.PlayRandomMessage(Summoned, proj.Top);
			}
		}
	}
}
