using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Magic {
	public class Laser_Tag_Gun : AnimatedModItem, IElementalItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "HardmodeMagicGun"
        ];
        public ushort Element => Elements.Earth;
		static DrawAnimationManual animation;
		public override DrawAnimation Animation => animation;
		public override Color? GetGlowmaskTint(Player player) => Main.teamColor[player.team];
		public override void SetStaticDefaults() {
			animation = new DrawAnimationManual(1);
			Main.RegisterItemAnimation(Item.type, animation);
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SpaceGun);
			Item.damage = 1;
			Item.DamageType = DamageClasses.RangedMagic;
			Item.noMelee = true;
			Item.crit = 46;
			Item.width = 26;
			Item.height = 18;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.mana = 10;
			Item.shoot = ModContent.ProjectileType<Laser_Tag_Laser>();
			Item.scale = 1f;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 15);
			recipe.AddIngredient(ModContent.ItemType<Fiberglass_Item>(), 18);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 12);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
		public override void UpdateInventory(Player player) {
		}

		static int GetCritMod(Player player) {
			OriginPlayer modPlayer = player.GetModPlayer<OriginPlayer>();
			int critMod = 0;
			if ((modPlayer.oldBonuses & 1) != 0 || modPlayer.fiberglassSet || modPlayer.fiberglassDagger) {
				critMod = -50;
			}
			if ((modPlayer.oldBonuses & 2) != 0 || modPlayer.felnumSet) {
				critMod = -64;
			}
			return critMod;
		}
		public override void ModifyWeaponCrit(Player player, ref float crit) {
			if (player.HeldItem.type != Item.type) crit += GetCritMod(player);
		}
		/*public override Vector2? HoldoutOffset() {
            return new Vector2(3-(11*Main.player[Item.playerIndexTheItemIsReservedFor].direction),0);
        }*/
		public override void HoldItem(Player player) {
			if (player.itemAnimation != 0) {
				player.GetModPlayer<OriginPlayer>().itemLayerWrench = true;
			}
			int critMod = GetCritMod(player);
			player.GetCritChance(DamageClass.Ranged) += critMod;
			player.GetCritChance(DamageClass.Magic) += critMod;
		}
	}
	public class Laser_Tag_Laser : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GreenLaser);
			Projectile.light = 0;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates++;
			Projectile.DamageType = DamageClasses.RangedMagic;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			try {
				Color color = Main.teamColor[Main.player[Projectile.owner].team];
				Lighting.AddLight(Projectile.Center, Vector3.Normalize(color.ToVector3()) * 3);
			} catch (Exception) { }
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.CritDamage *= 123 * 0.5f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.Cursed, 600);
		}
		public override bool PreDraw(ref Color lightColor) {
			Color color = Main.teamColor[Main.player[Projectile.owner].team];
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, new Vector2(42, 1), Projectile.scale, SpriteEffects.None, 1);
			return false;
		}
	}
}
