using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Projectiles;
using System.IO;
using Terraria.DataStructures;

namespace Origins.Items.Weapons.Demolitionist {
	//very 7y, following the theme of the whole ashen countdown theme
    public class Internal_Combustionfish : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 7;
		}
		public override void SetDefaults() {
			Item.DefaultToThrownWeapon(ModContent.ProjectileType<Internal_Combustionfish_P>(), 30, 6f);
			Item.DamageType = DamageClasses.ThrownExplosive;
			Item.damage = 35;
			Item.knockBack = 10f;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 7;
            Item.ArmorPenetration += 7;
			Item.AllowReforgeForStackableItem = true;
		}
		public override bool WeaponPrefix() => true;
		public override bool CanShoot(Player player) {
			consumed = false;
			return true;
		}
		public override void OnConsumeItem(Player player) {
			consumed = true;
		}
		internal static bool consumed = false;
	}
	public class Internal_Combustionfish_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Internal_Combustionfish";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
		}
		Vector2 stickPos = default;
		float stickRot = 0;
		bool consumed = false;
		int prefix = 0;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 60 * 20;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
		}
		public override void OnSpawn(IEntitySource source) {
			consumed = Internal_Combustionfish.consumed;
			Internal_Combustionfish.consumed = false;
			if (source is EntitySource_ItemUse itemUse) {
				prefix = itemUse.Item.prefix;
			}
		}
		public override void AI() {
			if (Projectile.ai[2] == 0) {
				Rectangle hitbox = Projectile.Hitbox;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.friendly && hitbox.Intersects(npc.Hitbox)) {
						Projectile.ai[2] = npc.whoAmI + 1;
						stickPos = (Projectile.Center - npc.Center).RotatedBy(-npc.rotation);
						stickRot = Projectile.rotation - npc.rotation;
						Projectile.netUpdate = true;
						break;
					}
				}
			} else {
				if (Projectile.ai[2] == -1) {
					Projectile.Center = stickPos;
					Projectile.rotation = stickRot;
					Projectile.velocity.X = 0;
					Projectile.velocity.Y = 0;
				} else {
					NPC npc = Main.npc[(int)Projectile.ai[2] - 1];
					if (npc.active) {
						Projectile.Center = npc.Center + stickPos.RotatedBy(npc.rotation);
						Projectile.rotation = stickRot + npc.rotation;
						Projectile.velocity.X = 0;
						Projectile.velocity.Y = 0;
					} else {
						Projectile.ai[2] = 0;
					}
				}
				if (Projectile.timeLeft <= 3) return;
				Rectangle hitbox = Projectile.Hitbox;
				hitbox.Inflate(64, 64);
				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.friendly && npc.whoAmI != (int)Projectile.ai[2] - 1 && npc.CanBeChasedBy(Projectile) && hitbox.Intersects(npc.Hitbox)) {
						Projectile.timeLeft = 3;
					}
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.timeLeft == 0 && !Projectile.IsNPCIndexImmuneToProjectileType(Type, target.whoAmI)) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.perIDStaticNPCImmunity[Type][target.whoAmI] = Main.GameUpdateCount + 1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[2] = -1;
			stickPos = Projectile.Center;
			stickRot = Projectile.rotation;
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(stickPos.X);
			writer.Write(stickPos.Y);
			writer.Write(stickRot);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			stickPos.X = reader.ReadSingle();
			stickPos.Y = reader.ReadSingle();
			stickRot = reader.ReadSingle();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 192, sound: SoundID.Item62);
			if (consumed && Projectile.owner == Main.myPlayer) {
				int item = Item.NewItem(
					Projectile.GetSource_Death(),
					Projectile.Center,
					ModContent.ItemType<Internal_Combustionfish>(),
					prefixGiven: prefix
				);
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
				}
			}
		}
	}
}
