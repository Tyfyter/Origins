using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Explosives {
	public class Peatball : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Peat Ball");
			Tooltip.SetDefault("");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Snowball);
            Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
            //item.maxStack = 999;
            Item.damage*=3;
			Item.value+=20;
			Item.useTime = (int)(Item.useTime*0.75);
			Item.useAnimation = (int)(Item.useAnimation*0.75);
            Item.shoot = ModContent.ProjectileType<Peatball_P>();
			Item.shootSpeed*=1.35f;
            Item.knockBack*=2;
			Item.rare = ItemRarityID.Blue;
		}
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type, 4);
            recipe.AddIngredient(ModContent.ItemType<Peat_Moss>());
            recipe.Register();
        }
    }
    public class Peatball_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Peatball";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
            Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
            Projectile.penetrate = 1;
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 0.85f;
        }
        /*public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.Kill();
            return false;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
            return true;
        }*/
        public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
            SoundEngine.PlaySound(SoundID.Item14.WithVolume(0.66f), Projectile.Center);
            Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
            Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
            //Main.gore[Gore.NewGore(NPC.GetSource_Death(), new Vector2(projectile.Center.X, projectile.Center.Y), default, Main.rand.Next(61, 64))].velocity += Vector2.One;
        }
    }
}
