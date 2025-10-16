using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Flashbang : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.ThrownExplosive,
			WikiCategories.IsGrenade,
			WikiCategories.ExpendableWeapon
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Confused, BuffID.Slow, BuffID.Darkness];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 32;
			Item.crit += 6;
			Item.shootSpeed *= 1.75f;
			Item.shoot = ModContent.ProjectileType<Flashbang_P>();
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 15);
			Item.ArmorPenetration += 4;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 25)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ItemID.Grenade, 25)
			.Register();
		}
	}
	public class Flashbang_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flashbang";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = -1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Flash_P>(), 0, 6, Projectile.owner, ai1: -0.5f).scale = 1f;
			Projectile.Damage();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.TryGetGlobalNPC(out Blind_Debuff_Global blindGlobal) && blindGlobal.blindable) {
				target.AddBuff(Blind_Debuff.ID, 120);
			} else {
				target.AddBuff(BuffID.Confused, 220);
			}
			target.AddBuff(BuffID.Slow, 300);
			target.AddBuff(BuffID.Darkness, 120);
		}
	}
	public class Flash_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flash";
		public override void SetDefaults() {
			Projectile.timeLeft = 25;
			Projectile.tileCollide = false;
			Projectile.alpha = 100;
			Projectile.hide = true;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(1, 1, 1));
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overWiresUI.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			const float scale = 2f;
			Main.spriteBatch.Restart(SpriteSortMode.Immediate);
			DrawData data = new(
				Mod.Assets.Request<Texture2D>("Projectiles/Pixel").Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, 0, 1, 1),
				new Color(0, 0, 0, 255),
				0, new Vector2(0.5f, 0.5f),
				new Vector2(160, 160) * scale,
				SpriteEffects.None,
			0);
			float percent = Projectile.timeLeft / 10f;
			Origins.blackHoleShade.UseOpacity(0.985f);
			Origins.blackHoleShade.UseSaturation(0f + percent);
			Origins.blackHoleShade.UseColor(1, 1, 1);
			Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.5f);
			Origins.blackHoleShade.Apply(data);
			Main.EntitySpriteDraw(data);
			Main.spriteBatch.Restart();
			return false;
		}
	}
}
