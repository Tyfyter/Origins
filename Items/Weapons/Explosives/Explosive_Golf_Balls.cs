using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Explosives {
	public class Impact_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_"+ItemID.GolfBallDyedBlack;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBallDyedBlack);
			Item.damage = 80;
			Item.DamageType = DamageClasses.Explosive;
			Item.shoot = ModContent.ProjectileType<Impact_Golf_Ball_P>();
		}
	}
	public class Impact_Golf_Ball_P : Golf_Ball_Projectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBlack;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Impact Golf Ball");
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if(CanDamage()??true)Projectile.Kill();
			return true;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Projectile.Kill();
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIV;
			return true;
		}
	}
	public class Explosive_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GolfBallDyedBlack;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBallDyedBlack);
			Item.damage = 80;
			Item.DamageType = DamageClasses.Explosive;
			Item.shoot = ModContent.ProjectileType<Explosive_Golf_Ball_P>();
		}
	}
	public class Explosive_Golf_Ball_P : Golf_Ball_Projectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBlack;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Explosive Golf Ball");
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 1;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Projectile.Kill();
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIII;
			return true;
		}
	}
}
