using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using Origins.NPCs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Melee {
	public class Vorpal_Sword_Cursed : ModItem, IJournalEntryItem {
		static short glowmask;
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
		public string EntryName => "Origins/" + typeof(Vorpal_Sword_Entry).Name;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Vorpal Sword");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 17;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 42;
			Item.height = 50;
			Item.useTime = 17;
			Item.useAnimation = 34;
			Item.shoot = ModContent.ProjectileType<Cursed_Vorpal_Sword_Slash>();
			Item.shootSpeed = 12;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 0f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = CursedRarity.ID;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 9999;
			Item.UseSound = null;
			Item.glowMask = glowmask;
		}
		static int textIndex = -1;
		static int delayTime = 0;
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			string text = "";
			var color = Main.MouseTextColorReal;
			switch (++textIndex) {
				case 0:
				text = "Twas brillig, and the slithy toves";
				break;
				case 1:
				text = "  Did gyre and gimble in the wabe;";
				break;
				case 2:
				text = "All mimsy were the borogoves,";
				break;
				case 3:
				text = "  And the mome raths outgrabe.";
				break;
				case 4:
				text = "\"Beware the Jabberwock, my son!";
				break;
				case 5:
				text = "  The jaws that bite, the claws that catch!";
				break;
				case 6:
				text = "Beware the Jubjub bird, and shun";
				break;
				case 7:
				text = "  The frumious Bandersnatch!\"";
				break;
				case 8:
				text = "He took his vorpal sword in hand:";
				break;
				case 9:
				text = "  Long time the manxome foe he soughtÑ";
				break;
				case 10:
				text = "So rested he by the Tumtum tree,";
				break;
				case 11:
				text = "  And stood awhile in thought.";
				break;
				case 12:
				text = "And as in uffish thought he stood,";
				break;
				case 13:
				text = "  The Jabberwock, with eyes of flame,";
				break;
				case 14:
				text = "Came whiffling through the tulgey wood,";
				break;
				case 15:
				text = "  And burbled as it came!";
				break;
				case 16:
				text = "One, two! One, two! And through and through";
				break;
				case 17:
				text = $"   The vorpal blade went snicker-snack!";
				color = color.MultiplyRGBA(new(1f, 0f, 0f, delayTime / 13f));
				if (delayTime == 0) delayTime = 13;
				break;
				case 18:
				text = "He left it dead, and with its head";
				break;
				case 19:
				text = "  He went galumphing back.";
				break;
				case 20:
				text = "\"And hast thou slain the Jabberwock?";
				break;
				case 21:
				text = "  Come to my arms, my beamish boy!";
				break;
				case 22:
				text = $"O frabjous day! Callooh! Callay!\"";
				break;
				case 23:
				text = "  He chortled in his joy.";
				break;
				case 24:
				text = "'Twas brillig, and the slithy toves";
				break;
				case 25:
				text = "  Did gyre and gimble in the wabe;";
				break;
				case 26:
				text = "All mimsy were the borogoves,";
				break;
				case 27:
				text = "  And the mome raths outgrabe. ";
				break;
				default:
				textIndex = 0;
				goto case 0;
			}
			if (delayTime == 0) delayTime = 2;
			if (delayTime > 0) {
				delayTime--;
				if (delayTime > 0) {
					textIndex--;
				}
			}
			tooltips.Add(new TooltipLine(Mod, "SnickerSnack", text) {
				OverrideColor = color
			});
		}
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			if (line.Name == "SnickerSnack") {
				line.X += Main.rand.Next(-2, 3);
				line.Y += Main.rand.Next(-2, 3);
			}
			return true;
		}
		int times = 0;
		float dir = 0;
		int switchbackSlot = -1;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			SoundEngine.PlaySound(SoundID.Item1, position);
			if (times > 0) {
				velocity = OriginExtensions.Vec2FromPolar(dir, velocity.Length());
				times--;
				player.direction = Math.Sign(velocity.X);
				if (times == 0 && switchbackSlot > -1) {
					player.selectedItem = switchbackSlot;
					switchbackSlot = -1;
				}
			}
			bool firstSwing = player.ItemUsesThisAnimation == 1;
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: firstSwing ? 0 : 1, ai1: firstSwing ? 1 : -1);
			return false;
		}
		public override void UpdateInventory(Player player) {
			if (times == 0 && Main.rand.NextBool(60)) {
				NPC npc;
				for (int n = 0; n < Main.maxNPCs; n++) {
					npc = Main.npc[n];
					if (npc.DistanceSQ(player.MountedCenter) < 128 * 128 && npc.CanBeChasedBy()) {
						for (int i = 0; i < Main.InventoryItemSlotsCount; i++) {
							if (player.inventory[i].ModItem == this) {
								switchbackSlot = player.selectedItem;
								player.selectedItem = i;
								player.controlUseItem = true;
								dir = (npc.Center - player.MountedCenter).ToRotation();
								times = 2;
								return;
							}
						}
					}
				}
			}
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			Item.autoReuse = false;
			if (times > 0) {
				player.controlUseItem = true;
				Item.autoReuse = true;
			}
		}
	}
	public class Cursed_Vorpal_Sword_Slash : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Vorpal_Sword_Cursed";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] *= player.direction;
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			Projectile.rotation = MathHelper.Lerp(-2.75f, 2f, swingFactor) * Projectile.ai[1];
			switch ((int)Projectile.ai[0]) {
				case 0:
				player.velocity = Vector2.Lerp(
					Vector2.Lerp(player.velocity, Vector2.Zero, swingFactor),
					Projectile.velocity * 3f,
					MathHelper.Lerp(swingFactor, 0, swingFactor)
				);
				break;
				case 1:
				player.velocity = Vector2.Lerp(
					player.velocity,
					Vector2.Lerp(player.velocity, Vector2.Zero, swingFactor * swingFactor),
					MathHelper.Lerp(swingFactor * swingFactor, 0, swingFactor)
				);
				break;
			}
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.MountedCenter - Projectile.velocity + (Vector2)new PolarVec2(32, realRotation);
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 1; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 60, targetSeverity: 0.65f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]),
				new Vector2(14, 25 + 11 * Projectile.ai[1]),
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Vorpal_Sword_Entry : JournalEntry {
		public override string TextKey => "Vorpal_Sword";
		public override ArmorShaderData TextShader => null;
	}
}
