using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using PegasusLib;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;

namespace Origins.Items.Weapons.Ranged {
	public class Harpoon_Gun : ModItem {
		protected override bool CloneNewInstances => true;
		public AutoLoadingAsset<Texture2D> ChainTexture { get; private set; }
		public int ChainFrames { get; protected set; } = 1;
		public override void AutoStaticDefaults() {
			base.AutoStaticDefaults();
			ChainTexture = Texture + "_Chain";
			if (GetType() != typeof(Harpoon_Gun)) return;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Harpoon;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Harpoon] = Type;
		}
		/// <summary>
		/// Item.DamageType = DamageClass.Ranged; <br/>
		/// Item.useStyle = ItemUseStyleID.Shoot; <br/>
		/// Item.noMelee = true; <br/>
		/// Item.useAmmo = Harpoon.ID; <br/>
		/// Item.shoot = Harpoon_P.ID; <br/>
		/// Item.UseSound = SoundID.Item11; <br/>
		/// Item.autoReuse = true; <br/>
		/// </summary>
		protected void DefaultToHarpoonGun() {
			Item.DamageType = DamageClass.Ranged;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.UseSound = SoundID.Item11;
			Item.autoReuse = true;
		}
		public override void SetDefaults() {
			DefaultToHarpoonGun();
			Item.damage = 24;
			Item.knockBack = 4;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.reuseDelay = 2;
			Item.width = 58;
			Item.height = 22;
			Item.shootSpeed = 14.75f;
			Item.value = Item.sellPrice(silver: 54);
			Item.rare = ItemRarityID.Green;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		protected bool consume = false;
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextBool(7);
		}
		public override bool? CanChooseAmmo(Item ammo, Player player) {
			consume = false;
			return null;
		}
		public override void OnConsumeAmmo(Item ammo, Player player) {
			consume = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (OriginConfig.Instance.NewHarpoonsFromTheFuture) player.StartChanneling(type);
			ModifyShotProjectile(Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: consume ? 1 : 0), source);
			return false;
		}
		public virtual void ModifyShotProjectile(Projectile projectile, EntitySource_ItemUse_WithAmmo source) { }
		public virtual int GetChainFrame(int index, HarpoonGlobalProjectile global, Projectile projectile) => index % ChainFrames;
	}
}
