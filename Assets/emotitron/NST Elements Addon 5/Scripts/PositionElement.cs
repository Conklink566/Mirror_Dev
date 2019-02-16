//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Compression;
using emotitron.Utilities.SmartVars;
using emotitron.Utilities.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.NST
{

	[System.Serializable]
	public class PositionElement : TransformElement, IPositionElement
	{
		// Constructor
		public PositionElement()
		{
			crusher = new ElementCrusher(TRSType.Position, false) { local = true, hideFieldName = true };
		}

		public override GenericX Localized
		{
			// Can modify this in the future to handle local/global positions if need be.
			get { return new GenericX(crusher.local ? gameobject.transform.localPosition : gameobject.transform.position); }
			set { Apply(value, gameobject); }
		}
		
		public float ClampAxis(float value, int axisId)
		{
			FloatCrusher ar = crusher[axisId];
			// Only range compression has ranges at all.
			return  ar.Clamp(value);
		}
		
		public override GenericX Extrapolate(GenericX curr, GenericX prev)
		{
			if (curr.type == XType.NULL)
			{
				Debug.Log("Extrap pos element NULL !! Try to eliminate these Davin");
				return Localized;
			}

			return
				(extrapolation == 0) ? curr :
				curr + (curr - prev) * extrapolation;
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(PositionElement))]
	[CanEditMultipleObjects]

	public class PositionElementEditor : TransformElementDrawer
	{
		
	}
#endif
}



