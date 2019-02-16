//Copyright 2018, Davin Carten, All rights reserved

using System.Runtime.InteropServices;
using UnityEngine;

namespace emotitron.Compression
{

	/// <summary>
	/// A struct that allows Quaternion and Vector types to be treated as the same.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct Element
	{
		public enum VectorType { Vector3 = 1, Quaternion = 2 }

		[FieldOffset(0)]
		public VectorType vectorType;

		[FieldOffset(4)]
		public Vector3 v;

		[FieldOffset(4)]
		public Quaternion quat;

		public Element(Vector3 v) : this()
		{
			vectorType = VectorType.Vector3;
			this.v = v;
		}

		public Element(Quaternion quat) : this()
		{
			vectorType = VectorType.Quaternion;
			this.quat = quat;
		}

		public static implicit operator Quaternion(Element e)
		{
			if (e.vectorType == VectorType.Quaternion)
				return e.quat;
			else
				return Quaternion.Euler(e.v);
		}
		public static implicit operator Vector3(Element e)
		{
			if (e.vectorType == VectorType.Vector3)
				return e.v;
			else
				return e.quat.eulerAngles;
		}

		public static Element Slerp(Element a, Element b, float t)
		{
			if (a.vectorType == VectorType.Quaternion)
				return Quaternion.Slerp(a, b, t);
			else
				return Quaternion.Slerp(a, b, t).eulerAngles;
		}

		public static Element SlerpUnclamped(Element a, Element b, float t)
		{
			if (a.vectorType == VectorType.Quaternion)
				return Quaternion.SlerpUnclamped(a, b, t);
			else
				return Quaternion.SlerpUnclamped(a, b, t).eulerAngles;
		}

		public static implicit operator Element(Quaternion q) { return new Element(q); }
		public static implicit operator Element(Vector3 v) { return new Element(v); }

		public override string ToString()
		{
			return vectorType + " " + ((vectorType == VectorType.Quaternion) ? quat.ToString() : v.ToString());
		}
	}
}
