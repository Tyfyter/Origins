using System;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace Origins.CrossMod.Avalon {
	public class Stamina : ILoadable {
		public static Action<Player, int> Restore { get; private set; } = (_, _) => { };
		public void Load(Mod mod) {
			if (!ModLoader.TryGetMod("Avalon", out Mod Avalon)) return;
			Type AvalonStaminaPlayer = Avalon.Code.GetType("Avalon.Common.Players.AvalonStaminaPlayer");
			FieldInfo StatStam = AvalonStaminaPlayer.GetField(nameof(StatStam));
			FieldInfo StatStamMax2 = AvalonStaminaPlayer.GetField(nameof(StatStamMax2));
			try {
				DynamicMethod method = new(nameof(Restore), typeof(void), [typeof(Player), typeof(int)], true);
				ILGenerator gen = method.GetILGenerator();

				LocalBuilder staminaPlayer = gen.DeclareLocal(AvalonStaminaPlayer);

				gen.Emit(OpCodes.Ldarg_0); // AvalonStaminaPlayer staminaPlayer = player.GetModPlayer<AvalonStaminaPlayer>();
				gen.Emit(OpCodes.Callvirt, typeof(Player).GetMethod(nameof(Player.GetModPlayer), []).MakeGenericMethod(AvalonStaminaPlayer));
				gen.Emit(OpCodes.Stloc, staminaPlayer);

				gen.Emit(OpCodes.Ldloc, staminaPlayer); // staminaPlayer.StatStam += amount;
				gen.Emit(OpCodes.Dup); // staminaPlayer.StatStam += amount;
				gen.Emit(OpCodes.Ldfld, StatStam);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Add);
				gen.Emit(OpCodes.Stfld, StatStam);

				gen.Emit(OpCodes.Ldloc, staminaPlayer); // staminaPlayer.StaminaHealEffect(amount, true);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Ldc_I4_1);
				gen.Emit(OpCodes.Callvirt, AvalonStaminaPlayer.GetMethod("StaminaHealEffect"));

				gen.Emit(OpCodes.Ldarg_0); // player.AddBuff(ModContent.BuffType<StaminaDrain>(), 540, true, false);
				gen.Emit(OpCodes.Call, typeof(ModContent).GetMethod(nameof(ModContent.BuffType)).MakeGenericMethod(Avalon.Code.GetType("Avalon.Buffs.Debuffs.StaminaDrain")));
				gen.Emit(OpCodes.Ldc_I4, 540);
				gen.Emit(OpCodes.Ldc_I4_1);
				gen.Emit(OpCodes.Ldc_I4_0);
				gen.Emit(OpCodes.Callvirt, typeof(Player).GetMethod(nameof(Player.AddBuff)));

				gen.Emit(OpCodes.Ldloc, staminaPlayer); // Min(ref staminaPlayer.StatStam, staminaPlayer.StatStamMax2);
				gen.Emit(OpCodes.Ldflda, StatStam);
				gen.Emit(OpCodes.Ldloc, staminaPlayer);
				gen.Emit(OpCodes.Ldfld, StatStamMax2);
				gen.Emit(OpCodes.Call, GetType().GetMethod(nameof(Min)).MakeGenericMethod(typeof(int)));

				gen.Emit(OpCodes.Ret);

				Restore = method.CreateDelegate<Action<Player, int>>();
			} catch (Exception ex) {
				if (Origins.LogLoadingILError($"{nameof(Stamina)}.{nameof(Restore)}", ex)) throw;
			}
		}


		public static void Min<T>(ref T current, T @new) where T : IComparisonOperators<T, T, bool> {
			if (current > @new) current = @new;
		}
		public void Unload() {}
	}
}
