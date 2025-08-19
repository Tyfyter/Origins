using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.ColoredDamageTypes {
	[Autoload(false)]
	public class SentryExplosive : DamageClass { }
	public class ColoredDamageTypes : ModSystem {
		private static DamageClass explosiveSentry;
		public static DamageClass ExplosiveSentry => explosiveSentry ??= ModContent.GetInstance<SentryExplosive>();
		public static Mod ColoredTypesMod;
		public static Color explColor = new(234, 56, 103);
		public static Color explCritColor = new(235, 0, 59);
		public static Dictionary<DamageClass, (Color dmgColor, Color critColor)> colors = [];

		public override bool IsLoadingEnabled(Mod mod) {
			return ModLoader.HasMod("ColoredDamageTypes");
		}
		public override void Load() {
			Mod.AddContent(explosiveSentry = new SentryExplosive());
			if (ModLoader.TryGetMod("ColoredDamageTypes", out ColoredTypesMod)) {
				static bool PushesDamageClass(ILContext il, Instruction instruction) {
					if (instruction.MatchLdarg(out int arg)) return il.Method.Parameters[arg].ParameterType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdloc(out int loc)) return il.Body.Variables[loc].VariableType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchCallOrCallvirt(out MethodReference method)) return method.ReturnType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdfld(out FieldReference field) || instruction.MatchLdsfld(out field)) return field.FieldType.FullName == il.Import(typeof(DamageClass)).FullName;
					return false;
				}
				static void FixMethods(ILContext il) {
					ILCursor c = new(il);
					while (c.TryGotoNext(MoveType.Before,
						i => i.MatchCallOrCallvirt<object>(nameof(ToString)) && PushesDamageClass(il, i.Previous)
					)) {
						c.Remove();
						c.EmitCallvirt(typeof(ModType).GetMethod("get_" + nameof(ModType.FullName)));
					}
				}
				foreach (MethodInfo method in ColoredTypesMod.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
					MonoModHooks.Modify(method, FixMethods);
				}
				foreach (MethodInfo method in ColoredTypesMod.Code.GetType("ColoredDamageTypes.DamageTypes")?.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) ?? []) {
					MonoModHooks.Modify(method, FixMethods);
				}
				//MonoModHooks.Modify(coloredDamageTypes.GetType().GetMethod("LoadModdedDamageTypes", BindingFlags.Public | BindingFlags.Static), FixMethods);
			}
		}
		public override void PostSetupContent() {
			Type DamageConfig = ColoredTypesMod.Code.GetType("ColoredDamageTypes.DamageConfig");
			FieldInfo VanillaDmg = DamageConfig.GetField("VanillaDmg");
			object VanillaInstance = VanillaDmg.GetValue(DamageConfig.GetField("Instance").GetValue(null));
			FieldInfo VanillaMeleeDmg = VanillaDmg.FieldType.GetField("MeleeDmg");
			FieldInfo VanillaRangeDmg = VanillaDmg.FieldType.GetField("RangedDmg");
			FieldInfo VanillaMagicDmg = VanillaDmg.FieldType.GetField("MagicDmg");
			FieldInfo VanillaThrowDmg = VanillaDmg.FieldType.GetField("ThrowingDmg");
			FieldInfo VanillaSummonDmg = VanillaDmg.FieldType.GetField("SummonDmg");
			FieldInfo VanillaSentryDmg = VanillaDmg.FieldType.GetField("SentryDmg");
			Color MeleeColor = (Color)VanillaMeleeDmg.FieldType.GetField("MeleeDamage").GetValue(VanillaMeleeDmg.GetValue(VanillaInstance));
			Color MeleeCritColor = (Color)VanillaMeleeDmg.FieldType.GetField("MeleeDamageCrit").GetValue(VanillaMeleeDmg.GetValue(VanillaInstance));
			Color RangeColor = (Color)VanillaRangeDmg.FieldType.GetField("RangedDamage").GetValue(VanillaRangeDmg.GetValue(VanillaInstance));
			Color RangeCritColor = (Color)VanillaRangeDmg.FieldType.GetField("RangedDamageCrit").GetValue(VanillaRangeDmg.GetValue(VanillaInstance));
			Color MagicColor = (Color)VanillaMagicDmg.FieldType.GetField("MagicDamage").GetValue(VanillaMagicDmg.GetValue(VanillaInstance));
			Color MagicCritColor = (Color)VanillaMagicDmg.FieldType.GetField("MagicDamageCrit").GetValue(VanillaMagicDmg.GetValue(VanillaInstance));
			Color ThrowColor = (Color)VanillaThrowDmg.FieldType.GetField("ThrowingDamage").GetValue(VanillaThrowDmg.GetValue(VanillaInstance));
			Color ThrowCritColor = (Color)VanillaThrowDmg.FieldType.GetField("ThrowingDamageCrit").GetValue(VanillaThrowDmg.GetValue(VanillaInstance));
			Color SummonColor = (Color)VanillaSummonDmg.FieldType.GetField("SummonDamage").GetValue(VanillaSummonDmg.GetValue(VanillaInstance));
			Color SummonCritColor = (Color)VanillaSummonDmg.FieldType.GetField("SummonDamageCrit").GetValue(VanillaSummonDmg.GetValue(VanillaInstance));
			Color SentryColor = (Color)VanillaSentryDmg.FieldType.GetField("SentryDamage").GetValue(VanillaSentryDmg.GetValue(VanillaInstance));
			Color SentryCritColor = (Color)VanillaSentryDmg.FieldType.GetField("SentryDamageCrit").GetValue(VanillaSentryDmg.GetValue(VanillaInstance));

			DamageClass sentryClass = ColoredTypesMod.Find<DamageClass>("SentryClass");
			colors.Add(DamageClass.Melee, (MeleeColor, MeleeCritColor));
			colors.Add(DamageClass.MeleeNoSpeed, (MeleeColor, MeleeCritColor));
			colors.Add(DamageClass.Ranged, (RangeColor, RangeCritColor));
			colors.Add(DamageClass.Magic, (MagicColor, MagicCritColor));
			colors.Add(DamageClass.Throwing, (ThrowColor, ThrowCritColor));
			colors.Add(DamageClass.Summon, (SummonColor, SummonCritColor));
			colors.Add(DamageClass.SummonMeleeSpeed, (SummonColor, SummonCritColor));
			colors.Add(sentryClass, (SentryColor, SentryCritColor));

			if (DamageClasses.ExplosiveVersion.ContainsKey(sentryClass)) DamageClasses.ExplosiveVersion[sentryClass] = ExplosiveSentry;
			else DamageClasses.ExplosiveVersion.Add(sentryClass, ExplosiveSentry);

			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.Explosive,
				explColor,
				explColor,
				explCritColor
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.Incantation,
				colors[DamageClass.Summon].dmgColor,
				colors[DamageClass.Summon].dmgColor,
				colors[DamageClass.Summon].critColor
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.MeleeMagic,
				colors[DamageClass.Melee].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Melee].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Melee].critColor.MultiplyRGB(colors[DamageClass.Magic].critColor)
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.RangedMagic,
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Ranged].critColor.MultiplyRGB(colors[DamageClass.Magic].critColor)
			);
			ColoredTypesMod.Call("AddDamageType",
				ModContent.GetInstance<Chambersite_Mine_Launcher_Damage>(),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(explColor),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(explColor),
				colors[DamageClass.Ranged].critColor.MultiplyRGB(explCritColor)
			);

			if (OriginsModIntegrations.Thorium is not null) {
				if (OriginsModIntegrations.Thorium.TryFind("BardClass", out DamageClass bard)) colors.Add(bard, (new(), new()));
			}

			foreach (KeyValuePair<DamageClass, (Color dmgColor, Color critColor)> item in colors) {
				ColoredTypesMod.Call("AddDamageType",
					DamageClasses.ExplosiveVersion[item.Key],
					item.Value.dmgColor.MultiplyRGB(explColor),
					item.Value.dmgColor.MultiplyRGB(explColor),
					item.Value.critColor.MultiplyRGB(explCritColor)
				);
			}
		}
	}
}
