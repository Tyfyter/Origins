using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Laser_Tag_Gun : AnimatedModItem, IElementalItem, ICustomWikiStat {
		public static int CritDamage => 123;
		static short glowmask;
		public string[] Categories => [
			WikiCategories.MagicGun
		];
		public ushort Element => Elements.Earth;
		static DrawAnimationManual animation;
		public override DrawAnimation Animation => animation;
		public override Color? GetGlowmaskTint(Player player) => Main.teamColor[player.team];
		public override void SetStaticDefaults() {
			animation = new DrawAnimationManual(1);
			Main.RegisterItemAnimation(Item.type, animation);
			glowmask = Origins.AddGlowMask(this);
			ItemID.Sets.IsSpaceGun[Type] = true;
			OriginsSets.Items.DamageBonusScale[Type] = 0.05f;
		}
		public bool? Hardmode => true;
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
			Item.shoot = Laser_Tag_Laser.ID;
			Item.scale = 1f;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 15)
			.AddIngredient(ModContent.ItemType<Fiberglass_Item>(), 18)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rubber>(), 12)
			//.AddIngredient(ModContent.ItemType<Space_Goo_Item>(), 10)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override void HoldItem(Player player) {
			if (player.itemAnimation != 0) {
				player.GetModPlayer<OriginPlayer>().itemLayerWrench = true;
			}
		}
	}
	public class Laser_Tag_Laser : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
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
			if (!CritType.ModEnabled) modifiers.CritDamage *= Laser_Tag_Gun.CritDamage * 0.5f;
		}
		public static void OnHitPvP(Projectile proj, Player target) {
			target.AddBuff(BuffID.Cursed, 600);
			OriginPlayer originPlayer = target.GetModPlayer<OriginPlayer>();
			if (originPlayer.laserTagVestActive) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.laser_tag_hit);
				packet.Write((byte)proj.owner);
				packet.Write((byte)target.whoAmI);
				packet.Send(-1, Main.myPlayer);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Color color = Main.teamColor[Main.player[Projectile.owner].team];
			color.A = (byte)(color.A * 0.7f);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, new Vector2(42, 1), Projectile.scale, SpriteEffects.None, 1);
			return false;
		}
	}
	public class Laser_Tag_Gun_Crit_Type : CritType<Laser_Tag_Gun> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => Main.rand.NextBool();
		public override float CritMultiplier(Player player, Item item) => Laser_Tag_Gun.CritDamage; // crit damage already modified in ModifyHitNPC
	}
}
