using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class C6_Jackhammer : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"ToolWeapon"
		];
		public string EntryName => "Origins/" + typeof(C6_Jackhammer_Entry).Name;
		public class C6_Jackhammer_Entry : JournalEntry {
			public override string TextKey => "C6_Jackhammer";
			public override JournalSortIndex SortIndex => new("Mechanicus_Sovereignty", 5);
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ChlorophyteJackhammer);
			Item.damage = 24;
			Item.width = 28;
			Item.height = 26;
			Item.hammer = 54;
			//Item.useTime = Item.useAnimation = 60;
			Item.knockBack = 6f;
			Item.shootSpeed = 4f;
			Item.shoot = ModContent.ProjectileType<C6_Jackhammer_P>();
			Item.value = Item.sellPrice(silver: 48);
			Item.rare = ItemRarityID.Blue;
			Item.tileBoost = 0;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.controlUseTile) {
				player.controlUseItem = true;
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
            .AddIngredient(ModContent.ItemType<NE8>(), 6)
            .AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override float UseSpeedMultiplier(Player player) {
			return player.altFunctionUse == 2 ? 0.3f : 1;
		}
		public override bool MeleePrefix() => true;
        public override Vector2? HoldoutOffset() => new Vector2(-8, 4);
    }
	public class C6_Jackhammer_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ChlorophyteJackhammer);
			Projectile.width = 38;
			Projectile.height = 38;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
		}
		public override bool PreAI() {
			Player owner = Main.player[Projectile.owner];
			if (owner.controlUseTile) {
				owner.channel = true;
			}
			return true;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			if (Main.player[Projectile.owner].altFunctionUse == 2) {
				hitbox.Inflate((int)(hitbox.Width * (Projectile.scale - 1)), (int)(hitbox.Height * (Projectile.scale - 1)));
			}
			hitbox.Offset((Projectile.velocity * 2).ToPoint());
		}
		public override void AI() {
			Projectile.rotation -= MathHelper.PiOver2;
			Projectile.Center = Main.player[Projectile.owner].MountedCenter + Projectile.velocity.SafeNormalize(default) * 42 * Projectile.scale;
			Projectile.friendly = false;
			if (++Projectile.frameCounter > (Main.player[Projectile.owner].altFunctionUse == 2 ? 6 : 4)) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) {
					Projectile.frame = 0;
					Projectile.friendly = true;
					Vector2 slamPos = Projectile.Center + Projectile.velocity * 2f;
					SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolumeScale(0.15f).WithPitch(1), slamPos);
					for (int i = (Main.player[Projectile.owner].altFunctionUse == 2 ? 8 : 4); i --> 0;) {
						Dust.NewDustPerfect(
							slamPos,
							DustID.Torch,
							new Vector2(0, 1).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi))
						).velocity *= 2;
					}
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Main.player[Projectile.owner].altFunctionUse == 2) {
				modifiers.SourceDamage *= 1.65f;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				Projectile.rotation,
				new Vector2(50, 6 + (5 - 5 * Projectile.direction * player.gravDir)),
				Projectile.scale,
				Projectile.direction * player.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically
			);
			return false;
		}
	}
}
