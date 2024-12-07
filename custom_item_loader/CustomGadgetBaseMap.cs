using System;
using System.Collections.Generic;

namespace custom_item_mod
{
	public static class CustomGadgetBaseMap
	{
		private static Dictionary<Type, (Type, ApplySpecificCustomization)> mappedCustomTypes = new();
		public static void RegisterGadgetImplementation(Type sourceType, Type targetType, ApplySpecificCustomization apply)
		{
			if (!typeof(custom_item_components.GadgetBase).IsAssignableFrom(sourceType))
			{
				Main.Error($"Attempted to create mapping for {sourceType.Name} which does not inherit from {nameof(custom_item_components.GadgetBase)}");
				return;
			}
			if (!typeof(DV.Customization.Gadgets.GadgetBase).IsAssignableFrom(targetType))
			{
				Main.Error($"Attempted to create mapping to {targetType.Name} which does not inherit from {nameof(DV.Customization.Gadgets.GadgetBase)}");
				return;
			}
			mappedCustomTypes[sourceType] = (targetType, apply);
		}
		public static void UnregisterGadgetImplementation(Type sourceType)
		{
			if (mappedCustomTypes.ContainsKey(sourceType))
			{
				mappedCustomTypes.Remove(sourceType);
			}
		}
		internal static DV.Customization.Gadgets.GadgetBase CreateGadgetBase(custom_item_components.GadgetBase sourceGadgetBase)
		{
			Type targetType = default;
			if (mappedCustomTypes.ContainsKey(sourceGadgetBase.GetType())) {
				targetType = mappedCustomTypes[sourceGadgetBase.GetType()].Item1;
			} else
			{
				targetType = typeof(DV.Customization.Gadgets.GadgetBase);
			}
			return sourceGadgetBase.gameObject.AddComponent(targetType) as DV.Customization.Gadgets.GadgetBase;
		}
		internal static void ApplyCustomizations(custom_item_components.GadgetBase sourceGadgetBase, ref DV.Customization.Gadgets.GadgetBase targetGadgetBase)
		{
			try
			{
				if (mappedCustomTypes.ContainsKey(sourceGadgetBase.GetType()))
				{
					mappedCustomTypes[sourceGadgetBase.GetType()].Item2(sourceGadgetBase, ref targetGadgetBase);
				}
			} catch(Exception ex)
			{
				Main.Error($"Error applying specific customizations to {sourceGadgetBase.GetType()}");
				Main.Error(ex.Message);
			}
		}
		public delegate void ApplySpecificCustomization(custom_item_components.GadgetBase source, ref DV.Customization.Gadgets.GadgetBase target);
	}
}
