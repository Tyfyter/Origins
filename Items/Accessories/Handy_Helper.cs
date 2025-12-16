using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace Origins.Items.Accessories {
	public class Handy_Helper : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
		bool bothGloves = false;
		bool noGloves = false;

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PowerGlove);
			if (!bothGloves) Item.handOffSlot = -1;
			if (noGloves) Item.handOnSlot = -1;
			Item.knockBack = 8;
			Item.rare = ItemRarityID.Pink;
			Item.master = true;
			Item.hasVanityEffects = true;
			Item.value = Item.sellPrice(gold: 8);
			Item.glowMask = glowmask;
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			player.kbGlove = true;
			player.autoReuseGlove = true;
			player.meleeScaleGlove = true;
			player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.amebicVialCooldown > 0) {
				originPlayer.amebicVialVisible = false;
				return;
			}
			originPlayer.amebicVialVisible = !isHidden;
			const float maxDist = 71 * 71;
			Vector2 target = default;
			float bestWeight = 0;
			Projectile projectile;
			Vector2 currentPos;
			Vector2 diff;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				projectile = Main.projectile[i];
				if (projectile.active && (projectile.hostile || (Main.player[projectile.owner].hostile && Main.player[projectile.owner].team != player.team)) && Amebic_Vial.CanBeDeflected[projectile.type]) {
					currentPos = projectile.Hitbox.ClosestPointInRect(player.MountedCenter);
					diff = player.Hitbox.ClosestPointInRect(projectile.Center) - currentPos;
					float dist = diff.LengthSquared();
					if (dist > maxDist) continue;
					float currentWeight = Vector2.Dot(projectile.velocity, diff.SafeNormalize(default)) * dist;
					if (currentWeight > bestWeight) {
						bestWeight = currentWeight;
						target = currentPos;
					}
				}
			}
			bool npcTarget = false;
			NPC npc;
			for (int i = 0; i < Main.maxNPCs; i++) {
				npc = Main.npc[i];
				if (npc.active && npc.damage > 0 && !npc.friendly) {
					currentPos = npc.Hitbox.ClosestPointInRect(player.MountedCenter);
					diff = player.Hitbox.ClosestPointInRect(npc.Center) - currentPos;
					float dist = diff.LengthSquared();
					if (dist > maxDist) continue;
					float currentWeight = Vector2.Dot(npc.velocity, diff.SafeNormalize(default)) * dist;
					if (currentWeight > bestWeight) {
						npcTarget = npc.aiStyle != NPCAIStyleID.Spell;
						bestWeight = currentWeight;
						target = currentPos;
					}
				}
			}
			if (bestWeight > 0) {
				float dir = (target.Y > player.MountedCenter.Y == target.X > player.MountedCenter.X) ? -1 : 1;
				Projectile.NewProjectile(
					player.GetSource_Accessory(Item),
					player.MountedCenter,
					(target - player.MountedCenter).SafeNormalize(default).RotatedBy(dir * -1f) * 3.2f,
					npcTarget ? Handy_Helper_NPC_Tentacle.ID : Handy_Helper_Tentacle.ID,
					1,
					player.GetWeaponKnockback(Item),
					player.whoAmI,
					ai1: dir
				);
				originPlayer.amebicVialCooldown = 100;
			}
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<OriginPlayer>().amebicVialVisible = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Amebic_Vial>())
			.AddIngredient(ItemID.PowerGlove)
			.AddTile(TileID.TinkerersWorkbench)
			.AddCondition(LocalizedText.Empty, () => !OriginsModIntegrations.CheckAprilFools())
			.ApplyConditionsAsDecraftConditions()
			.Register();

			static void HalfPrefix(Item item) {
				switch (item.prefix) {
					case PrefixID.Warding:
					item.prefix = PrefixID.Guarding;
					break;
					case PrefixID.Armored:
					case PrefixID.Guarding:
					item.prefix = PrefixID.Hard;
					break;

					case PrefixID.Lucky:
					item.prefix = PrefixID.Precise;
					break;

					case PrefixID.Menacing:
					item.prefix = PrefixID.Spiked;
					break;
					case PrefixID.Angry:
					case PrefixID.Spiked:
					item.prefix = PrefixID.Jagged;
					break;

					case PrefixID.Quick:
					item.prefix = PrefixID.Fleeting;
					break;
					case PrefixID.Hasty:
					case PrefixID.Fleeting:
					item.prefix = PrefixID.Brisk;
					break;

					case PrefixID.Violent:
					item.prefix = PrefixID.Rash;
					break;
					case PrefixID.Intrepid:
					case PrefixID.Rash:
					item.prefix = PrefixID.Wild;
					break;

					case PrefixID.Hard:
					case PrefixID.Precise:
					case PrefixID.Jagged:
					case PrefixID.Brisk:
					case PrefixID.Wild:
					case PrefixID.Arcane:
					item.prefix = 0;
					break;
				}
			}

			CreateRecipe()
			.AddIngredient(ItemID.PowerGlove, 2)
			.AddIngredient(ModContent.ItemType<Amebic_Vial>())
			.AddTile(TileID.TinkerersWorkbench)
			.AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.AprilFools"), () => OriginsModIntegrations.CheckAprilFools())
			.AddOnCraftCallback((_, result, consumed, _) => {
				if (result.ModItem is Handy_Helper helper) {
					helper.bothGloves = true;
					helper.SetDefaults();
				}

				Item dropped = consumed[Main.rand.Next(1, 3)];
				HalfPrefix(dropped);
				Main.LocalPlayer.QuickSpawnItem(Entity.GetSource_DropAsItem(), dropped);
			})
			.ApplyConditionsAsDecraftConditions()
			.Register();

			Recipe.Create(ItemID.PowerGlove)
			.AddIngredient(Type)
			.AddTile(TileID.TinkerersWorkbench)
			.AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.AprilFoolsHandyHelper"), () => OriginsModIntegrations.CheckAprilFools())
			.AddOnCraftCallback((_, result, consumed, _) => {
				Item dropped = consumed[0];
				if (dropped.ModItem is Handy_Helper helper) {
					if (helper.bothGloves) {
						helper.bothGloves = false;
					} else if (!helper.noGloves) {
						helper.noGloves = true;
					} else {
						dropped.SetDefaults(ModContent.ItemType<Amebic_Vial>());
					}
					int droppedPrefix = dropped.prefix;
					helper.SetDefaults();
				}
				HalfPrefix(dropped);
				Main.LocalPlayer.QuickSpawnItem(Entity.GetSource_DropAsItem(), dropped);
			})
			.ApplyConditionsAsDecraftConditions()
			.Register();
		}
		public override void SaveData(TagCompound tag) {
			tag.Add("bothGloves", bothGloves);
			tag.Add("noGloves", noGloves);
		}
		public override void LoadData(TagCompound tag) {
			tag.TryGet("bothGloves", out bothGloves);
			tag.TryGet("noGloves", out noGloves);
			Item.handOnSlot = (sbyte)(noGloves ? -1 : ArmorIDs.HandOn.PowerGlove);
			Item.handOffSlot = (sbyte)(bothGloves ? ArmorIDs.HandOn.PowerGlove : -1);
		}
	}
	//extends Amebic_Vial_Tentacle so it inherits all of the changes that might be made to it that it can
	public class Handy_Helper_Tentacle : Amebic_Vial_Tentacle {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);

			Projectile.Center = ownerMountedCenter;

			if (movementFactor == 0f) {
				movementFactor = 5.1f;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft < 20) {
				movementFactor -= 1.1f;
			} else {
				movementFactor += 1.1f;
			}

			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1] * 0.05f);
			Projectile.position += Projectile.velocity * movementFactor;

			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
			Projectile other;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				other = Main.projectile[i];
				if (other.active && other.hostile && Amebic_Vial.CanBeDeflected[other.type] && (Colliding(Projectile.Hitbox, other.Hitbox) ?? false)) {
					other.velocity = Vector2.Lerp(other.velocity, Projectile.velocity, 0.6f);
				}
			}
		}
	}
	public class Handy_Helper_NPC_Tentacle : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Summoner/Flagellash_P";
		public override string GlowTexture => Texture;
		public static AutoCastingAsset<Texture2D> GloveTexture { get; private set; }
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			if (ModContent.RequestIfExists<Texture2D>("Origins/Items/Accessories/Handy_Helper_P", out ReLogic.Content.Asset<Texture2D> gloveTexture))
				GloveTexture = gloveTexture;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 2;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 40;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
			Projectile.alpha = 150;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);

			Projectile.Center = ownerMountedCenter;

			if (movementFactor == 0f) {
				movementFactor = 5.1f;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft < 20) {
				movementFactor -= 1.1f;
			} else {
				movementFactor += 1.1f;
			}

			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1] * 0.05f);
			Projectile.position += Projectile.velocity * movementFactor;

			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (target.knockBackResist > 0) {
				target.oldVelocity = target.velocity;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.knockBackResist > 0) {
				target.velocity = Vector2.Lerp(target.oldVelocity, Projectile.velocity * hit.Knockback / 3.2f, (float)Math.Sqrt(target.knockBackResist));
			}
			if (Projectile.timeLeft > 20) {
				Projectile.timeLeft = 20;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (projHitbox.Intersects(targetHitbox) || Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Main.player[Projectile.owner].MountedCenter + Projectile.velocity * 2, Projectile.Center)) {
				return true;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Vector2 diff = Projectile.Center - ownerMountedCenter;
			int dist = (int)Math.Min(diff.Length(), 180);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				ownerMountedCenter - Main.screenPosition,
				new Rectangle(0, 180 - dist, 6, dist),
				Color.White,
				Projectile.rotation,
				new Vector2(3, 0),
				Projectile.scale,
				SpriteEffects.None,
			0);
			if (GloveTexture.IsLoaded) {
				bool swapDir = Projectile.velocity.X < 0;
				Main.EntitySpriteDraw(
					GloveTexture,
					Projectile.Center - Main.screenPosition,
					new Rectangle(0, 0, 18, 16),
					lightColor,
					Projectile.rotation + (swapDir ? -MathHelper.PiOver2 : MathHelper.PiOver2),
					new Vector2(3, 9),
					Projectile.scale,
					swapDir ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			}
			return false;
		}
	}
	/*public class Single_Power_Glove : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.PowerGlove;
		override set
	}*/
}
