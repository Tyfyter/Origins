using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.NPCs;
using Origins.Projectiles;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Vorpal_Sword : Uncursed_Cursed_Item<Vorpal_Sword_Cursed>, IJournalEntrySource, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.4f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
            "Sword"
        ];
		public string EntryName => "Origins/" + typeof(Vorpal_Sword_Entry).Name;
		public override bool HasOwnTexture => true;
		public override void SetStaticDefaults() {
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 28;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 42;
			Item.height = 50;
			Item.useTime = 17;
			Item.useAnimation = 34;
			Item.shoot = ModContent.ProjectileType<Vorpal_Sword_Slash>();
			Item.shootSpeed = 12;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = float.Epsilon;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 150;
			Item.UseSound = null;
		}
		public override void AddRecipes() {
			base.AddRecipes();
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
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
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item1, player.itemLocation);
			return null;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (!player.controlUseItem) {
				player.itemAnimation = 0;
				player.itemTime = 0;
				return false;
			}
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback * 0.25f, player.whoAmI, ai1: player.ItemUsesThisAnimation == 1 ? 1 : -1);
			return false;
		}
		public override bool MeleePrefix() => true;
		public bool? Hardmode => false;
	}
	public class Vorpal_Sword_Slash : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Vorpal_Sword";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
			Projectile.noEnchantmentVisuals = true;
			Projectile.hide = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			Projectile.rotation = MathHelper.Lerp(-2.75f, 2f, swingFactor) * Projectile.ai[1] * player.gravDir;
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation * player.gravDir - MathHelper.PiOver2);
			Projectile.Center = player.GetCompositeArmPosition(false) + (Vector2)new PolarVec2(16, realRotation);

			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 1; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
			}
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
			OriginGlobalNPC.InflictTorn(target, 20, targetSeverity: Vorpal_Sword.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			float gravDir = Main.player[Projectile.owner].gravDir;
			SpriteEffects effects = Projectile.ai[1] * gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1] * gravDir),
				new Vector2(14, 18).Apply(effects ^ SpriteEffects.FlipVertically, TextureAssets.Projectile[Type].Size()),
				Projectile.scale,
				effects,
				0
			);
			return false;
		}
	}
}
