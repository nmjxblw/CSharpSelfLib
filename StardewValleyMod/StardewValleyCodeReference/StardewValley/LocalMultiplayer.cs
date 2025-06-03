using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace StardewValley;

public class LocalMultiplayer
{
	public delegate void StaticInstanceMethod(object staticVarsHolder);

	internal static List<FieldInfo> staticFields;

	internal static List<object> staticDefaults;

	public static Type StaticVarHolderType;

	private static DynamicMethod staticDefaultMethod;

	private static DynamicMethod staticSaveMethod;

	private static DynamicMethod staticLoadMethod;

	public static StaticInstanceMethod StaticSetDefault;

	public static StaticInstanceMethod StaticSave;

	public static StaticInstanceMethod StaticLoad;

	public static bool IsLocalMultiplayer(bool is_local_only = false)
	{
		if (is_local_only)
		{
			return Game1.hasLocalClientsOnly;
		}
		return GameRunner.instance.gameInstances.Count > 1;
	}

	public static void Initialize()
	{
		LocalMultiplayer.GetStaticFieldsAndDefaults();
		LocalMultiplayer.GenerateDynamicMethodsForStatics();
	}

	private static void GetStaticFieldsAndDefaults()
	{
		LocalMultiplayer.staticFields = new List<FieldInfo>();
		LocalMultiplayer.staticDefaults = new List<object>();
		HashSet<string> ignored_assembly_roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Microsoft", "MonoGame", "mscorlib", "NetCode", "System", "xTile" };
		List<Type> types = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (!ignored_assembly_roots.Contains(assembly.GetName().Name.Split('.')[0]))
			{
				Type[] types2 = assembly.GetTypes();
				foreach (Type type in types2)
				{
					types.Add(type);
				}
			}
		}
		foreach (Type type2 in types)
		{
			if (type2.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
			{
				continue;
			}
			bool include_by_default = type2.GetCustomAttribute<InstanceStatics>() != null;
			FieldInfo[] fields = type2.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields)
			{
				if (!field.IsInitOnly && field.IsStatic && !field.IsLiteral && (include_by_default || field.GetCustomAttribute<InstancedStatic>() != null) && field.GetCustomAttribute<NonInstancedStatic>() == null)
				{
					RuntimeHelpers.RunClassConstructor(field.DeclaringType.TypeHandle);
					LocalMultiplayer.staticFields.Add(field);
					LocalMultiplayer.staticDefaults.Add(field.GetValue(null));
				}
			}
		}
	}

	private static void GenerateDynamicMethodsForStatics()
	{
		TypeBuilder typeBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("StardewValley.StaticInstanceVars"), AssemblyBuilderAccess.RunAndCollect).DefineDynamicModule("MainModule").DefineType("StardewValley.StaticInstanceVars", TypeAttributes.Public | TypeAttributes.AutoClass);
		foreach (FieldInfo field in LocalMultiplayer.staticFields)
		{
			typeBuilder.DefineField(field.DeclaringType.Name + "_" + field.Name, field.FieldType, FieldAttributes.Public);
		}
		LocalMultiplayer.StaticVarHolderType = typeBuilder.CreateType();
		LocalMultiplayer.staticDefaultMethod = new DynamicMethod("SetStaticVarsToDefault", null, new Type[1] { typeof(object) }, typeof(Game1).Module, skipVisibility: true);
		ILGenerator il = LocalMultiplayer.staticDefaultMethod.GetILGenerator();
		LocalBuilder local = il.DeclareLocal(LocalMultiplayer.StaticVarHolderType);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Castclass, LocalMultiplayer.StaticVarHolderType);
		il.Emit(OpCodes.Stloc_0);
		FieldInfo defaultsField = typeof(LocalMultiplayer).GetField("staticDefaults", BindingFlags.Static | BindingFlags.NonPublic);
		MethodInfo listIndexOperator = typeof(List<object>).GetMethod("get_Item");
		for (int i = 0; i < LocalMultiplayer.staticFields.Count; i++)
		{
			FieldInfo field2 = LocalMultiplayer.staticFields[i];
			il.Emit(OpCodes.Ldloc, local.LocalIndex);
			il.Emit(OpCodes.Ldsfld, defaultsField);
			il.Emit(OpCodes.Ldc_I4, i);
			il.Emit(OpCodes.Callvirt, listIndexOperator);
			if (field2.FieldType.IsValueType)
			{
				il.Emit(OpCodes.Unbox_Any, field2.FieldType);
			}
			else
			{
				il.Emit(OpCodes.Castclass, field2.FieldType);
			}
			il.Emit(OpCodes.Stfld, LocalMultiplayer.StaticVarHolderType.GetField(field2.DeclaringType.Name + "_" + field2.Name));
		}
		il.Emit(OpCodes.Ret);
		LocalMultiplayer.StaticSetDefault = (StaticInstanceMethod)LocalMultiplayer.staticDefaultMethod.CreateDelegate(typeof(StaticInstanceMethod));
		LocalMultiplayer.staticSaveMethod = new DynamicMethod("SaveStaticVars", null, new Type[1] { typeof(object) }, typeof(Game1).Module, skipVisibility: true);
		il = LocalMultiplayer.staticSaveMethod.GetILGenerator();
		local = il.DeclareLocal(LocalMultiplayer.StaticVarHolderType);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Castclass, LocalMultiplayer.StaticVarHolderType);
		il.Emit(OpCodes.Stloc_0);
		foreach (FieldInfo field3 in LocalMultiplayer.staticFields)
		{
			il.Emit(OpCodes.Ldloc, local.LocalIndex);
			il.Emit(OpCodes.Ldsfld, field3);
			il.Emit(OpCodes.Stfld, LocalMultiplayer.StaticVarHolderType.GetField(field3.DeclaringType.Name + "_" + field3.Name));
		}
		il.Emit(OpCodes.Ret);
		LocalMultiplayer.StaticSave = (StaticInstanceMethod)LocalMultiplayer.staticSaveMethod.CreateDelegate(typeof(StaticInstanceMethod));
		LocalMultiplayer.staticLoadMethod = new DynamicMethod("LoadStaticVars", null, new Type[1] { typeof(object) }, typeof(Game1).Module, skipVisibility: true);
		il = LocalMultiplayer.staticLoadMethod.GetILGenerator();
		local = il.DeclareLocal(LocalMultiplayer.StaticVarHolderType);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Castclass, LocalMultiplayer.StaticVarHolderType);
		il.Emit(OpCodes.Stloc_0);
		foreach (FieldInfo field4 in LocalMultiplayer.staticFields)
		{
			il.Emit(OpCodes.Ldloc, local.LocalIndex);
			il.Emit(OpCodes.Ldfld, LocalMultiplayer.StaticVarHolderType.GetField(field4.DeclaringType.Name + "_" + field4.Name));
			il.Emit(OpCodes.Stsfld, field4);
		}
		il.Emit(OpCodes.Ret);
		LocalMultiplayer.StaticLoad = (StaticInstanceMethod)LocalMultiplayer.staticLoadMethod.CreateDelegate(typeof(StaticInstanceMethod));
	}

	public static void SaveOptions()
	{
		if (Game1.player != null && Game1.player.isCustomized.Value)
		{
			Game1.splitscreenOptions[Game1.player.UniqueMultiplayerID] = Game1.options;
		}
	}
}
