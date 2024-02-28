using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Area_Denial : ModItem {
        public string[] Categories => new string[] {
            "MagicStaff"
        };
        public override void SetStaticDefaults() {
            Item.staff[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.damage = 20;
            Item.noMelee = true;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Area_Denial_P>();
			Item.shootSpeed = 8f;
			Item.mana = 13;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item67;
			Item.autoReuse = false;
        }
    }
	public class Area_Denial_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.timeLeft = 5 * 60 * 60;
			Projectile.ignoreWater = true;
		}
        public override void AI() {
			if (Projectile.timeLeft % 60 == 0) {

			}
        }
	}
	public class Area_Denial_Explosion : ModProjectile {
		public override void SetDefaults() {
			Projectile.timeLeft = 5 * 60 * 60;
			Projectile.ignoreWater = true;
		}
	}
}
