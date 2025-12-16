using Origins.Buffs;
using Origins.Dev;
using Origins.Projectiles;
using Origins.Tiles.Other;
using PegasusLib;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Sapphire {
    [AutoloadEquip(EquipType.Head)]
	public class Sapphire_Hood : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            WikiCategories.ArmorSet,
            WikiCategories.MagicBoostGear
        ];
        public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.statManaMax2 -= 40;
			player.manaRegenBonus += 100;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Sapphire_Vest>() && legs.type == ModContent.ItemType<Sapphire_Tights>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Sapphire");
			player.GetModPlayer<OriginPlayer>().sapphireSet = true;
			int type = ModContent.ProjectileType<Sapphire_Aura>();
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[type] <= 0) {
				Projectile.NewProjectile(
					player.GetSource_Misc("Sapphire_Set"),
					player.MountedCenter,
					default,
					type,
					0,
					0
				);
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Sapphire, 4)
			.AddIngredient(ItemID.SoulofMight, 3)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public string ArmorSetName => "Sapphire_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Sapphire_Vest>();
		public int LegsItemID => ModContent.ItemType<Sapphire_Tights>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Sapphire_Vest : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 10;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Magic) += 0.2f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Sapphire, 12)
			.AddIngredient(ItemID.SoulofMight, 3)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 36)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Sapphire_Tights : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.2f;
			player.manaCost *= 0.85f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Sapphire, 8)
			.AddIngredient(ItemID.SoulofMight, 3)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 24)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Sapphire_Aura : ModProjectile {
		const float range = 256;
		public override string Texture => "Terraria/Images/Extra_194";
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.GetGlobalProjectile<OriginGlobalProj>().weakShimmer = player.OriginPlayer()?.weakShimmer ?? false;

			Projectile.position = player.MountedCenter;
			if (!player.dead && player.GetModPlayer<OriginPlayer>().sapphireSet) {
				Projectile.timeLeft = 5;
			} else {
				Projectile.Kill();
				return;
			}
			float manaFactor = 1 - player.statMana / (float)player.statManaMax2;
			manaFactor = 1 - manaFactor * manaFactor;
			if (manaFactor <= 0) return;
			void Push(Entity other, float factor = 1f) {
				Vector2 diff = other.Center - Projectile.position;
				float dist = diff.LengthSquared();
				if (dist <= range * range) {
					Vector2 normalizedDir = diff.SafeNormalize(-Vector2.UnitY);
					float otherSpeed = other.velocity.Length();
					other.velocity += normalizedDir * (10f / System.MathF.Pow(dist, 0.75f) + 2.5f) * factor;
					//other.velocity -= (0.1f + 0.2f / MathHelper.Max(System.MathF.Pow(other.damage, 0.5f) - 2, 1)) * Vector2.Dot(other.velocity, normalizedDir) * normalizedDir;
					if (other.velocity != Vector2.Zero) other.velocity *= otherSpeed / other.velocity.Length();
					Dust.NewDustDirect(other.position, other.width, other.height,
						DustID.Wet,
						normalizedDir.X * 4, normalizedDir.Y * 4
					).noGravity = true;
				}
			}
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (i == Projectile.whoAmI) continue;
				Projectile other = Main.projectile[i];
				if (other.active && other.hostile && other.damage > 0) {
					Push(other);
				}
			}
			foreach (NPC npc in Main.ActiveNPCs) {
				if (NPCID.Sets.ProjectileNPC[npc.type] || npc.CanBeChasedBy(Projectile)) Push(npc, 0.5f);
			}
			for (int i = 0; i < Main.maxPlayers; i++) {
				if (i == Projectile.owner) continue;
				Player other = Main.player[i];
				if (other.active && ((!other.hostile && !player.hostile) || other.team == player.team) && other.position.IsWithin(Projectile.Center, range)) {
					other.AddBuff(Sapphire_Aura_Buff.ID, 5);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Sapphire_Aura_Drawer drawer = default;
			drawer.Length = range;
			Player player = Main.player[Projectile.owner];
			float manaFactor = 1 - player.statMana / (float)player.statManaMax2;
			drawer.ManaFactor = 1 - manaFactor * manaFactor;
			drawer.Draw(Projectile);
			return false;
		}
	}
	public struct Sapphire_Aura_Drawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new();

		public float Length;

		public float ManaFactor;
		public void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:SapphireAura"];
			int num = 1;//1
			int num2 = 0;//0
			int num3 = 0;//0
			float w = 0f;//0.6f
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailShape]);
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
			miscShaderData.Apply();
			float uTime = (float)Main.timeForVisualEffects / 44;
			const int verts = 128;
			float[] rot = new float[verts + 1];
			Vector2[] pos = new Vector2[verts + 1];
			Vector2 playerVelocity = Main.player[proj.owner].velocity;
			for (int i = 0; i < verts + 1; i++) {
				rot[i] = (i * MathHelper.TwoPi) / verts + uTime;
				pos[i] = proj.position + new Vector2(Length, 0).RotatedBy(rot[i] + MathHelper.PiOver2);
				Lighting.AddLight(pos[i] + playerVelocity, 0, 0.1f, 0.4f);
			}
			_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth, -Main.screenPosition, pos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			return new Color(0, 50, 200, 175) * ManaFactor;
		}

		private float StripWidth(float progressOnStrip) {
			return 64;
		}
	}
}
