using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Other {
	public class Tones_Of_Agony : ModItem, IElementalItem {
        public ushort Element => Elements.Earth;

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tones Of Agony");
			Tooltip.SetDefault("A tome full of ancient spells used for harming another with malicious intent.");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 40;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.mana = 18;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Agony_Shard>();
			Item.rare = ItemRarityID.Green;
		}
    }
    public class Agony_Shard : ModProjectile {
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Bullet);
            Projectile.penetrate = 5;//when projectile.penetrate reaches 0 the projectile is destroyed
            Projectile.extraUpdates = 2;
            Projectile.aiStyle = 0;
            Projectile.width = Projectile.height = 10;
            Projectile.hide = true;
            Projectile.light = 0;
        }
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.Pi/2;
            Dust.NewDust(Projectile.Center, 0, 0, DustID.Dirt, Scale:0.4f);
        }
        public override void Kill(int timeLeft) {
            Dust.NewDust(Projectile.position, 10, 10, DustID.Dirt, Scale:0.6f);
            Dust.NewDust(Projectile.position, 10, 10, DustID.Dirt, Scale:0.6f);
            Dust.NewDust(Projectile.position, 10, 10, DustID.Dirt, Scale:0.6f);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
		    if(Projectile.hide) behindNPCsAndTiles.Add(index);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            hitbox.X+=(int)Projectile.velocity.X;
            hitbox.Y+=(int)Projectile.velocity.Y;
        }
        public override bool PreDraw(ref Color lightColor) {
            Main.EntitySpriteDraw(Mod.Assets.Request<Texture2D>("Items/Weapons/Other/Agony_Shard").Value, (Projectile.Center+Projectile.velocity) - Main.screenPosition, new Rectangle(0,0,10,14), lightColor, Projectile.rotation, new Vector2(5, 7), 1f, SpriteEffects.None, 0);
            return true;
        }
    }
}
