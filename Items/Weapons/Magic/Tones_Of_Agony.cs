using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Tones_Of_Agony : ModItem, IElementalItem, ICustomWikiStat {
        public string[] Categories => [
            "SpellBook"
        ];
        public ushort Element => Elements.Earth;
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Bleeding];
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
			Item.mana = 15;
			Item.shoot = ModContent.ProjectileType<Agony_Shard>();
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Green;
		}
    }
	public class Agony_Shard : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 5;
			Projectile.extraUpdates = 2;
			Projectile.aiStyle = 0;
			Projectile.width = Projectile.height = 10;
			Projectile.hide = true;
			Projectile.light = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 2;
			Dust.NewDust(Projectile.Center, 0, 0, DustID.Dirt, Scale: 0.4f);
		}
		public override void OnKill(int timeLeft) {
			Dust.NewDust(Projectile.position, 10, 10, DustID.Dirt, Scale: 0.6f);
			Dust.NewDust(Projectile.position, 10, 10, DustID.Dirt, Scale: 0.6f);
			Dust.NewDust(Projectile.position, 10, 10, DustID.Dirt, Scale: 0.6f);
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			if (Projectile.hide) behindNPCsAndTiles.Add(index);
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.X += (int)Projectile.velocity.X;
			hitbox.Y += (int)Projectile.velocity.Y;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Bleeding, 300);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, (Projectile.Center + Projectile.velocity) - Main.screenPosition, new Rectangle(0, 0, 10, 14), lightColor, Projectile.rotation, new Vector2(5, 7), 1f, SpriteEffects.None, 0);
			return true;
		}
	}
}
