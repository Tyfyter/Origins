using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Longbone : ModItem, ICustomWikiStat {
		internal static int t = ProjectileID.WoodenArrowFriendly;
        public string[] Categories => new string[] {
            "Bow"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldBow);
			Item.damage = 28;
			Item.knockBack = 5;
			Item.crit = 4;
			Item.useTime = Item.useAnimation = 16;
			Item.shoot = ModContent.ProjectileType<Bone_Bolt>();
			Item.shootSpeed = 9;
			Item.width = 24;
			Item.height = 56;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Green;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type < ProjectileID.Count) {
				t = type;
				type = Item.shoot;
			}
		}
	}
	public class Bone_Bolt : ModProjectile {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Terraria/Images/Projectile_117";
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			// DisplayName.SetDefault("Bone Bolt");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(Longbone.t);
			Projectile.timeLeft = 30;
			Projectile.extraUpdates = 1;
			Projectile.localAI[0] = Longbone.t;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.type = (int)Projectile.localAI[0];
			Projectile.StatusNPC(target.whoAmI);
			Projectile.type = ID;
			if ((int)Projectile.localAI[0] == ProjectileID.HellfireArrow) {
				Projectile.Kill();
			}
		}
		public override bool PreKill(int timeLeft) {
			if ((int)Projectile.localAI[0] == ProjectileID.HellfireArrow) {
				int t = Bone_Shard.ID;
				Longbone.t = (int)Projectile.localAI[0];
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.3f), t, Projectile.damage / 5, 2, Projectile.owner).localNPCImmunity = Projectile.localNPCImmunity;
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.3f), t, Projectile.damage / 5, 2, Projectile.owner).localNPCImmunity = Projectile.localNPCImmunity;
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.3f), t, Projectile.damage / 5, 2, Projectile.owner).localNPCImmunity = Projectile.localNPCImmunity;
				SoundEngine.PlaySound(SoundID.NPCHit2.WithVolume(0.75f).WithPitchRange(0.1f, 0.2f), Projectile.Center);
				Projectile.SetToType(ProjectileID.HellfireArrow);
				return true;
			}
			return true;
		}
		public override void OnKill(int timeLeft) {
			int t = Bone_Shard.ID;
			Longbone.t = (int)Projectile.localAI[0];
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.3f), t, Projectile.damage / 5, 2, Projectile.owner).localNPCImmunity = Projectile.localNPCImmunity;
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.3f), t, Projectile.damage / 5, 2, Projectile.owner).localNPCImmunity = Projectile.localNPCImmunity;
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(0.3f), t, Projectile.damage / 5, 2, Projectile.owner).localNPCImmunity = Projectile.localNPCImmunity;
			SoundEngine.PlaySound(SoundID.NPCHit2.WithVolume(0.75f).WithPitchRange(0.1f, 0.2f), Projectile.Center);
			Projectile.SetToType(ProjectileID.HellfireArrow);
		}
		internal static void Render(Projectile projectile, Color lightColor) {
			int d;
			switch ((int)projectile.localAI[0]) {
				case ProjectileID.FireArrow:
				lightColor.R += (byte)Math.Min(80, 255 - lightColor.R);
				lightColor.G += (byte)Math.Min(60, 255 - lightColor.G);
				lightColor.B += (byte)Math.Min(10, 255 - lightColor.B);
				Dust.NewDust(projectile.Center, 0, 0, DustID.Torch);
				break;
				case ProjectileID.FrostburnArrow:
				lightColor.G += (byte)Math.Min(60, 255 - lightColor.G);
				lightColor.B += (byte)Math.Min(80, 255 - lightColor.B);
				Dust.NewDust(projectile.Center, 0, 0, DustID.IceTorch);
				break;
				case ProjectileID.CursedArrow:
				lightColor.R += (byte)Math.Min(30, 255 - lightColor.R);
				lightColor.G += (byte)Math.Min(80, 255 - lightColor.G);
				d = Dust.NewDust(projectile.Center, 0, 0, DustID.BubbleBurst_Green, Scale: 0.75f);
				Main.dust[d].velocity *= 0.5f;
				break;
				case ProjectileID.IchorArrow:
				lightColor.R += (byte)Math.Min(80, 255 - lightColor.R);
				lightColor.G += (byte)Math.Min(80, 255 - lightColor.G);
				Dust.NewDust(projectile.Center, 0, 0, DustID.GoldFlame);
				break;
				case ProjectileID.VenomArrow:
				d = Dust.NewDust(projectile.Center, 0, 0, DustID.Water_Corruption);
				Main.dust[d].noGravity = true;
				break;
				case ProjectileID.HellfireArrow:
				lightColor.R += (byte)Math.Min(80, 255 - lightColor.R);
				lightColor.G += (byte)Math.Min(60, 255 - lightColor.G);
				lightColor.B += (byte)Math.Min(10, 255 - lightColor.B);
				Dust.NewDust(projectile.Center, 0, 0, DustID.Torch);
				break;
			}
			for (int i = 1; i < 5; i++) {
				float x = projectile.velocity.X * i;
				float y = projectile.velocity.Y * i;
				Color color = projectile.GetAlpha(lightColor);
				float a = 0f;
				switch (i) {
					case 1:
					a = 0.4f;
					break;
					case 2:
					a = 0.3f;
					break;
					case 3:
					a = 0.2f;
					break;
					case 4:
					a = 0.1f;
					break;
				}
				color.R = (byte)(color.R * a);
				color.G = (byte)(color.G * a);
				color.B = (byte)(color.B * a);
				color.A = (byte)(color.A * a);
				Texture2D texture = TextureAssets.Projectile[projectile.type].Value;
				Main.EntitySpriteDraw(texture, new Vector2(projectile.position.X - Main.screenPosition.X - x, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - y), new Rectangle(0, 0, texture.Width, texture.Height), color, projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), projectile.scale, SpriteEffects.None, 0);
			}
		}
		public override void PostDraw(Color lightColor) {
			Render(Projectile, lightColor);
		}
	}
	public class Bone_Shard : ModProjectile {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Origins/Projectiles/Weapons/BoneS_hard";
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			// DisplayName.SetDefault("BoneS hard");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(Longbone.t);
			Projectile.extraUpdates = 1;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localAI[0] = Longbone.t;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.type = (int)Projectile.localAI[0];
			Projectile.StatusNPC(target.whoAmI);
			Projectile.type = ID;
			if ((int)Projectile.localAI[0] == ProjectileID.HellfireArrow) {
				Projectile.Kill();
			}
		}
		public override bool PreKill(int timeLeft) {
			Projectile.SetToType((int)Projectile.localAI[0]);
			SoundEngine.PlaySound(SoundID.NPCHit2.WithVolume(0.75f).WithPitchRange(0.1f, 0.2f), Projectile.Center);
			return true;
		}
		public override void PostDraw(Color lightColor) {
			Bone_Bolt.Render(Projectile, lightColor);
		}
	}
}
