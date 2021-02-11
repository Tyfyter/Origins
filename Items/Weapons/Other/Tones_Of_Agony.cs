using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Tones_Of_Agony : ModItem, IElementalItem {
        public ushort Element => Elements.Earth;

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tones Of Agony");
			Tooltip.SetDefault("A tome full of ancient spells used for harming another with malicious intent.");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.RubyStaff);
			item.damage = 40;
			item.magic = true;
			item.noMelee = true;
			item.width = 28;
			item.height = 30;
			item.useTime = 28;
			item.useAnimation = 28;
			item.mana = 18;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Agony_Shard>();
			item.rare = ItemRarityID.Green;
		}
    }
    public class Agony_Shard : ModProjectile {
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.penetrate = 5;//when projectile.penetrate reaches 0 the projectile is destroyed
            projectile.extraUpdates = 2;
            projectile.aiStyle = 0;
            projectile.width = projectile.height = 10;
            projectile.hide = true;
            projectile.light = 0;
        }
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation()+MathHelper.Pi/2;
            Dust.NewDust(projectile.Center, 0, 0, 0, Scale:0.4f);
        }
        public override void Kill(int timeLeft) {
            Dust.NewDust(projectile.position, 10, 10, 0, Scale:0.6f);
            Dust.NewDust(projectile.position, 10, 10, 0, Scale:0.6f);
            Dust.NewDust(projectile.position, 10, 10, 0, Scale:0.6f);
            Main.PlaySound(SoundID.Dig, projectile.position);
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI){
            if(projectile.hide)drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            hitbox.X+=(int)projectile.velocity.X;
            hitbox.Y+=(int)projectile.velocity.Y;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.Draw(mod.GetTexture("Items/Weapons/Other/Agony_Shard"), (projectile.Center+projectile.velocity) - Main.screenPosition, new Rectangle(0,0,10,14), lightColor, projectile.rotation, new Vector2(5, 7), 1f, SpriteEffects.None, 0f);
            return true;
        }
    }
}
