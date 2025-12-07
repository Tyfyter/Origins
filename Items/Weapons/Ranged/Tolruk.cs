using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using ReLogic.Utilities;
using Origins.Items.Weapons.Magic;
using Origins.Buffs;
using Origins.Items.Tools;
using Origins.CrossMod;

namespace Origins.Items.Weapons.Ranged {
	public class Tolruk : ModItem {
		public static short[] glowmasks;
        public override void SetStaticDefaults() { //still needs reshaping chee
			glowmasks = [
				-1,
				Origins.AddGlowMask(Texture + "_Glow_1"),
				Origins.AddGlowMask(Texture + "_Glow_2"),
				Origins.AddGlowMask(Texture + "_Glow_3"),
				Origins.AddGlowMask(Texture + "_Glow_4"),
				Origins.AddGlowMask(Texture + "_Glow_5"),
				Origins.AddGlowMask(Texture + "_Glow_6"),
				Origins.AddGlowMask(Texture + "_Glow_7"),
				Origins.AddGlowMask(Texture + "_Glow_8"),
				Origins.AddGlowMask(Texture + "_Glow_9"),
				Origins.AddGlowMask(Texture + "_Glow_10")
			];
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Electrified_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.damage = 37;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.width = 18;
			Item.height = 36;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 1;
			Item.shootSpeed = 14;
			Item.autoReuse = true;
			Item.useAmmo = AmmoID.Bullet;
			Item.shoot = ProjectileID.Bullet;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = SoundID.Item11;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Uzi)
			.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		SlotId soundSlot;
		public override void HoldItem(Player player) {
			int charge = player.OriginPlayer().tolrukCharge;
			Item.glowMask = glowmasks[Math.Min(charge / 4, 10)];
			if (charge > 0) {
				if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) {
					float chargeFactor = charge / 40f;
					sound.Volume = chargeFactor * 1f;
					sound.Pitch = chargeFactor * 0.2f;
					sound.Position = player.Center;
				} else {
					soundSlot = SoundEngine.PlaySound(Origins.Sounds.LightningCharging, player.Center);
				}
			} else if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) {
				sound.Stop();
			}
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-12, -2);
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(Origins.Sounds.Lightning.WithPitch(1f).WithVolume(0.01f), player.itemLocation);
			boltShot = false;
			ref int charge = ref player.OriginPlayer().tolrukCharge;
			if (charge >= 40) {
				charge = 0;
				SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolume(2), player.itemLocation);
				SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds), player.itemLocation);
				boltShot = true;
			} else charge += 4;
			return null;
		}
		bool boltShot = false;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 offset = velocity.SafeNormalize(Vector2.Zero);
			position -= offset.RotatedBy(MathHelper.PiOver2) * player.direction * 2;
			damage += player.OriginPlayer().tolrukCharge;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (boltShot) Projectile.NewProjectileDirect(source, position, velocity.SafeNormalize(velocity / 16) * 8, ModContent.ProjectileType<Tolruk_Bolt>(), damage * 3, knockback, player.whoAmI);
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Item.glowMask = glowmasks[Math.Min(Main.LocalPlayer.OriginPlayer().tolrukCharge / 4, 10)];
			if (Item.glowMask > -1) spriteBatch.Draw(TextureAssets.GlowMask[Item.glowMask].Value, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
		}
	}
	public class Tolruk_Bolt : Magnus_P {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			const int max_length = 1200 * 2;
			ProjectileID.Sets.TrailCacheLength[Type] = max_length / tick_motion;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = max_length + 16;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			startupDelay = 8;
			randomArcing = 0.1f;
			Projectile.DamageType = DamageClass.Ranged;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Electrified_Debuff.ID, Main.rand.Next(180, 301));
		}
	}
}
