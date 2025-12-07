using MonoMod.Cil;
using Origins.Dev;
using Origins.Misc;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Abyssal_Anchor : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ManaShielding,
			WikiCategories.MagicBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Yellow;
			Item.expert = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Binding_Book>())
			.AddIngredient(ModContent.ItemType<Celestial_Stone_Mask>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			player.manaMagnet = true;
			player.magicCuffs = true;
			player.statManaMax2 += 20;
			originPlayer.manaShielding += 0.25f;
			originPlayer.abyssalAnchor = true;
			originPlayer.refactoringPieces = true;

			player.GetDamage(DamageClass.Generic) += 0.1f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
			player.GetCritChance(DamageClass.Generic) += 4f;
			player.lifeRegen += 4;
			player.statDefense += 8;
			player.pickSpeed -= 0.25f;
			player.endurance += (1 - player.endurance) * 0.1f;
			player.GetKnockback(DamageClass.Summon).Base += 1;

			originPlayer.moveSpeedMult *= 0.75f;
			player.jumpSpeedBoost -= 2.2f;
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.abyssalAnchorVisual = true;
			Vector2 halfSize = new Vector2(12);
			ref Physics.Chain chain = ref originPlayer.abyssalAnchorChain;
			if (chain is null || chain.links[0].position.HasNaNs() || chain.links[0].position.DistanceSQ(player.position) > 512 * 512) {
				Physics.EntityAnchorPoint anchor = new() {
					entity = player,
					offset = Vector2.Zero
				};
				const float spring = 0.5f;
				Physics.Gravity[] gravs = [
					new Physics.ConstantGravity(Vector2.UnitY * 0.08f)
				];
				chain = new Physics.Chain() {
					anchor = anchor,
					links = [
						new(anchor.WorldPosition, default, 8, gravs, drag: 0.93f, spring: spring),
						new(anchor.WorldPosition, default, 8, gravs, drag: 0.93f, spring: spring),
						new(anchor.WorldPosition, default, 8, gravs, drag: 0.93f, spring: spring),
						new(anchor.WorldPosition, default, 8, gravs, drag: 0.93f, spring: spring),
						new(anchor.WorldPosition, default, 8, gravs, drag: 0.93f, spring: spring),
						new(anchor.WorldPosition, default, 8, gravs, drag: 0.93f, spring: spring),
						new(anchor.WorldPosition, default, 8, gravs, drag: 0.93f, spring: spring),
						new(anchor.WorldPosition, default, 12, [new Physics.EntityDirectionGravity(new Vector2(-0.18f, 0.08f), player), ..gravs], drag: 0.93f, spring: spring)
					]
				};
				originPlayer.abyssalAnchorPosition = chain.links[^1].position - halfSize;
				originPlayer.abyssalAnchorVelocity = default;
			}
			Vector2 anchorWorldPosition = chain.anchor.WorldPosition;
			void DoCollision(ref Vector2 position, ref Vector2 velocity) {
				if (!position.IsWithin(anchorWorldPosition, 68 * 3)) return;
				bool fall = !position.IsWithin(anchorWorldPosition, 68);
				Vector4 slopeCollision = Collision.SlopeCollision(position, velocity, 24, 24, fall: fall);
				position = slopeCollision.XY();
				velocity = slopeCollision.ZW();
				velocity = Collision.TileCollision(position, velocity, 24, 24, fallThrough: fall);
			}
			DoCollision(ref originPlayer.abyssalAnchorPosition, ref originPlayer.abyssalAnchorVelocity);
			/*Vector4 slopeCollision = Collision.SlopeCollision(originPlayer.abyssalAnchorPosition, originPlayer.abyssalAnchorVelocity, 24, 24);
			originPlayer.abyssalAnchorPosition = slopeCollision.XY();
			originPlayer.abyssalAnchorVelocity = slopeCollision.ZW();
			originPlayer.abyssalAnchorVelocity = Collision.TileCollision(originPlayer.abyssalAnchorPosition, originPlayer.abyssalAnchorVelocity, 24, 24);*/
			int kMax = 2;
			for (int k = 0; k < kMax; k++) {
				chain.links[^1].position = originPlayer.abyssalAnchorPosition + halfSize;
				chain.links[^1].velocity = originPlayer.abyssalAnchorVelocity;
				Vector2[] deltas = chain.Update();
				Vector2 difference = (chain.links[^1].position - halfSize) - originPlayer.abyssalAnchorPosition;
				DoCollision(ref originPlayer.abyssalAnchorPosition, ref difference);
				originPlayer.abyssalAnchorPosition += difference;
				originPlayer.abyssalAnchorVelocity = chain.links[^1].velocity;
				if (OriginsModIntegrations.CheckAprilFools()) {
					float oldVelY = player.velocity.Y;
					float sum = 0;
					for (int j = 0; j < deltas.Length; j++) {
						player.velocity -= deltas[j] * 0.004f;
						sum += deltas[j].Y * 0.004f;
					}
					if (float.Abs(sum) < 0.001f) {
						player.velocity.Y = oldVelY;
					}
				}
				if (kMax < 20 && (chain.links[0].position - chain.anchor.WorldPosition).LengthSquared() > 128 * 128) {
					kMax++;
				}
			}
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) => player.OriginPlayer().abyssalAnchorDye = dye;
		internal static void IL_Player_WaterCollision(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdfld<Entity>(nameof(Entity.velocity)),
				i => i.MatchLdcR4(0.5f),
				i => i.MatchCall<Vector2>("op_Multiply")
			);
			c.EmitLdarg0();
			c.EmitDelegate((Vector2 velocity, Player player) => {
				if (player.OriginPlayer().abyssalAnchor && player.velocity.Y > 0 && player.wet) velocity.Y *= 2.3f;
				return velocity;
			});
		}
	}
}
