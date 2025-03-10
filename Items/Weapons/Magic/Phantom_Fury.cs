using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Terraria.DataStructures;

namespace Origins.Items.Weapons.Magic {
    public class Phantom_Fury : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "Wand"
        ];
        public override void SetStaticDefaults() {
			Item.staff[Type] = true;
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 40;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 9;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.width = 30;
			Item.height = 36;
			Item.useTime = 46;
			Item.useAnimation = 46;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Phantom_Fury_P>();
			Item.shootSpeed = 16f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f);
			Item.autoReuse = true;
			Item.glowMask = glowmask;
		}
	}
	public class Phantom_Fury_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.width = Projectile.height = 8;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 80;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.hide = true;
			Projectile.friendly = false;
		}
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Center, DustID.AncientLight, default, newColor: Color.White, Scale: 0.5f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f);
			Rectangle hitbox = Projectile.Hitbox;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.Hitbox.Intersects(hitbox)) {
					Projectile.Kill();
					break;
				}
			}
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 3; i++) {
				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Phantom_Fury_P2>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner,
					i
				);
			}
		}
	}
	public class Phantom_Fury_P2 : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Phantom_Fury_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.width = Projectile.height = 8;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
		}
		public override void AI() {
			Projectile.friendly = false;
			Projectile.hide = Projectile.ai[0] > 0;
			if (++Projectile.frameCounter > 7) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= 3) {
					if (Projectile.ai[0] > 0) {
						Projectile.ai[0]--;
						Projectile.frame = 0;
						return;
					}
					if (Projectile.frame > 3) {
						Projectile.Kill();
						return;
					}
					Projectile.friendly = true;
				}
			}
		}
	}
}
