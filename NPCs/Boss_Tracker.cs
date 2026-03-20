using Origins.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.NPCs {
	[Obsolete("Superseded by ProgressFlags", true)]
	public class Boss_Tracker : ModSystem {
		public static Boss_Tracker Instance => ModContent.GetInstance<Boss_Tracker>();
		public bool downedFiberglassWeaver;
		public bool downedLostDiver;
		public bool downedShimmerConstruct;

		public bool downedChambersiteSentinel;

		public bool downedDefiledMimic;
		public bool downedRivenMimic;
		public bool downedTrashCompactorMimic;
		
		public override void Load() {
			{
				DynamicMethod _saveData = new("saveData", typeof(void), [typeof(TagCompound), typeof(Boss_Tracker)], true);
				DynamicMethod _loadData = new("loadData", typeof(void), [typeof(TagCompound), typeof(Boss_Tracker)], true);
				ILGenerator _saveDataGen = _saveData.GetILGenerator();
				ILGenerator _loadDataGen = _loadData.GetILGenerator();
				MethodInfo tagCompound_Set = typeof(TagCompound).GetMethod(nameof(TagCompound.Set));
				MethodInfo tagCompound_TryGet = typeof(TagCompound).GetMethod(nameof(TagCompound.TryGet)).MakeGenericMethod(typeof(bool));
				foreach (FieldInfo field in GetType().GetFields()) {
					if (field.IsStatic) continue;
					if (field.FieldType == typeof(bool)) {
						_saveDataGen.Emit(OpCodes.Ldarg_0);
						_saveDataGen.Emit(OpCodes.Ldstr, field.Name);
						_saveDataGen.Emit(OpCodes.Ldarg_1);
						_saveDataGen.Emit(OpCodes.Ldfld, field);
						_saveDataGen.Emit(OpCodes.Box, field.FieldType);
						_saveDataGen.Emit(OpCodes.Ldc_I4_0);
						_saveDataGen.Emit(OpCodes.Callvirt, tagCompound_Set);

						_loadDataGen.Emit(OpCodes.Ldarg_0);
						_loadDataGen.Emit(OpCodes.Ldstr, field.Name);
						_loadDataGen.Emit(OpCodes.Ldarg_1);
						_loadDataGen.Emit(OpCodes.Ldflda, field);
						_loadDataGen.Emit(OpCodes.Callvirt, tagCompound_TryGet);
						_loadDataGen.Emit(OpCodes.Pop);


					}
				}
				_saveDataGen.Emit(OpCodes.Ret);
				_loadDataGen.Emit(OpCodes.Ret);
				saveData = _saveData.CreateDelegate<Action<TagCompound, Boss_Tracker>>();
				loadData = _loadData.CreateDelegate<Action<TagCompound, Boss_Tracker>>();
			}
			{
				DynamicMethod _clearWorld = new("clearWorld", typeof(void), [typeof(Boss_Tracker)], true);
				ILGenerator _clearWorldGen = _clearWorld.GetILGenerator();
				foreach (FieldInfo field in GetType().GetFields()) {
					if (field.IsStatic) continue;
					if (field.FieldType == typeof(bool)) {
						_clearWorldGen.Emit(OpCodes.Ldarg_0);
						_clearWorldGen.Emit(OpCodes.Ldc_I4_0);
						_clearWorldGen.Emit(OpCodes.Stfld, field);
					}
				}
				_clearWorldGen.Emit(OpCodes.Ret);
				clearWorld = _clearWorld.CreateDelegate<Action<Boss_Tracker>>();
			}
		}
		public override void Unload() {
			foreach (FieldInfo field in GetType().GetFields()) {
				if (field.IsStatic && field.FieldType.IsClass) field.SetValue(null, null);
			}
		}
		public override void OnModLoad() {
			foreach (PropertyInfo property in typeof(ProgressFlags).GetProperties()) {
				if (property.PropertyType == typeof(ProgressFlag)) {
					Conditions.Add(property.Name[..1].ToLower() + property.Name[1..], (ProgressFlag)property.GetValue(null));
				}
			}
		}
		public static Dictionary<string, Condition> Conditions { get; private set; } = [];
		static Action<TagCompound, Boss_Tracker> saveData;
		static Action<TagCompound, Boss_Tracker> loadData;
		static Action<Boss_Tracker> clearWorld;
		public override void SaveWorldData(TagCompound tag) { }
		public override void LoadWorldData(TagCompound tag) {
			loadData?.Invoke(tag, this);
			if (downedFiberglassWeaver) ProgressFlags.DownedFiberglassWeaver.Set(true);
			if (downedLostDiver) ProgressFlags.DownedLostDiver.Set(true);
			if (downedShimmerConstruct) ProgressFlags.DownedShimmerConstruct.Set(true);
			if (downedChambersiteSentinel) ProgressFlags.DownedChambersiteSentinel.Set(true);
			if (downedDefiledMimic) ProgressFlags.DownedDefiledMimic.Set(true);
			if (downedRivenMimic) ProgressFlags.DownedRivenMimic.Set(true);
			if (downedTrashCompactorMimic) ProgressFlags.DownedTrashCompactorMimic.Set(true);
		}
		public override void ClearWorld() => clearWorld(this);
	}
}
