using MonoMod.Cil;
using Origins.Dev;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	//TODO: add fast conveyor belts with TileID.Sets.ConveyorDirection = 2
	public class Speed_Booster : ModItem, ICustomWikiStat {
		public delegate void ConveyorBeltModifier(ref Vector2 movement, Player player);
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.Movement,
			WikiCategories.RangedBoostAcc,
			WikiCategories.GenericBoostAcc
		];
		public override void SetStaticDefaults() {
			try {
				IL_Collision.StepConveyorBelt += EnableFastConveyors;
			} catch (Exception e) {
				if (Origins.LogLoadingILError($"{nameof(EnableFastConveyors)}", e)) throw;
			}
		}
		static void EnableFastConveyors(ILContext il) {
			ILCursor c = new(il);
			int maxValue = 0;
			c.EmitDelegate<Action>(() => maxValue = 0);
			//IL_03b9: ldloc.s 15
			//IL_03bb: ldloc.s 16
			//IL_03bd: mul
			//IL_03be: ldarg.1
			//IL_03bf: conv.i4
			//IL_03c0: mul
			c.GotoNext(MoveType.After,
				i => i.MatchLdloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchMul(),
				i => i.MatchLdarg1(),
				i => i.MatchConvI4(),
				i => i.MatchMul()
			);
			c.EmitDelegate<Func<int, int>>(value => {
				if (maxValue < Math.Abs(value)) maxValue = Math.Abs(value);
				return value;
			});
			c.GotoNext(MoveType.After,
				i => i.MatchLdcR4(2.5f),
				i => i.MatchCall<Vector2>("op_Multiply")
			);
			c.EmitLdarg0();
			c.EmitDelegate<Func<Vector2, Entity, Vector2>>((value, entity) => {
				value *= maxValue;
				if (entity is Player player) player.OriginPlayer().conveyorBeltModifiers?.Invoke(ref value, player);
				return value;
			});
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 24);
			Item.damage = 48;
			Item.knockBack = 3;
			Item.value = Item.sellPrice(gold: 14);
			Item.rare = ItemRarityID.Yellow;
			Item.accessory = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Automated_Returns_Handler>()
			.AddIngredient<Lovers_Leap>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.hasMagiluminescence = true;
			if (player.accRunSpeed < 6f) player.accRunSpeed = 6f;
			if (originPlayer.shineSparkCharge > 0) {
				player.accRunSpeed += 3f;
			}
			player.rocketBoots = 2;
			player.vanityRocketBoots = 2;
			originPlayer.guardedHeart = true;
			originPlayer.loversLeap = true;
			originPlayer.loversLeapItem = Item;
			originPlayer.shineSpark = true;
			originPlayer.shineSparkItem = Item;
			originPlayer.shineSparkVisible = !hideVisual;
			originPlayer.turboReel2 = true;
			originPlayer.automatedReturnsHandler = true;
			originPlayer.conveyorBeltModifiers += (ref Vector2 movement, Player player) => {
				if (Math.Sign(movement.X) == -Math.Sign(player.velocity.X)) {
					movement *= player.OriginPlayer().shineSparkCharge <= 0 ? 1.5f : 0.75f;
				}
			};

			player.blackBelt = true;
			player.dashType = 1;
			player.spikedBoots += 2;

			DelegateMethods.v3_1 = originPlayer.shineSparkCharge > 0 ? new Vector3(0.8f, 0.5f, 0.9f) : new Vector3(0.9f, 0.8f, 0.5f);
			Utils.PlotTileLine(player.Center, player.Center + player.velocity * 6f, 20f, DelegateMethods.CastLightOpen);
			Utils.PlotTileLine(player.Left, player.Right, 20f, DelegateMethods.CastLightOpen);
		}
	}
}
