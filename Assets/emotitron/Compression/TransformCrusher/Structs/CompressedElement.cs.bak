//Copyright 2018, Davin Carten, All rights reserved

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace emotitron.Compression
{

	/// <summary>
	/// The compressed result of crushing. Contains the individual float/quat crusher results involved.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public class CompressedElement : IEquatable<CompressedElement>
	{

		[FieldOffset(0)]
		public CompressedFloat cx;
		[FieldOffset(16)]
		public CompressedFloat cy;
		[FieldOffset(32)]
		public CompressedFloat cz;

		[FieldOffset(0)]
		public CompressedFloat cUniform;

		[FieldOffset(0)]
		public CompressedQuat cQuat;

		[FieldOffset(48)]
		public ElementCrusher crusher;

		public void Clear()
		{
			this.crusher = null;
			this.cx = new CompressedFloat(null, 0);
			this.cy = new CompressedFloat(null, 0);
			this.cz = new CompressedFloat(null, 0);
		}
		[Obsolete("Compressed Element is now a class and no longer a struct. Where this used to be used, now compressedElement.Clear() should be used instead.")]
		public static readonly CompressedElement Empty = new CompressedElement();

		public CompressedElement()
		{
			//UnityEngine.Debug.LogWarning("CE Construct NULL");
		}
		// Constructor
		public CompressedElement(ElementCrusher crusher, CompressedFloat cx, CompressedFloat cy, CompressedFloat cz)/* : this()*/
		{
			//UnityEngine.Debug.LogWarning("CE Construct");
			this.crusher = crusher;
			this.cx = cx;
			this.cy = cy;
			this.cz = cz;

		}

		// Constructor
		public CompressedElement(ElementCrusher crusher, uint cx, uint cy, uint cz) /*: this()*/
		{
			UnityEngine.Debug.LogWarning("CE Construct");
			this.crusher = crusher;
			this.cx = new CompressedFloat(crusher.xcrusher, cx);
			this.cy = new CompressedFloat(crusher.ycrusher, cy);
			this.cz = new CompressedFloat(crusher.zcrusher, cz);
		}

		// Constructor for uniform scale
		/// <summary>
		/// A uint argument indicates compressed uniform scale. A ulong argument indicates a compressed quaternion.
		/// Be sure to cast ulongs down to uint, or your scale we be treated as quaternion values for this constructor.
		/// </summary>
		/// <param name="crusher"></param>
		/// <param name="cUniform"></param>
		/// <param name="ubits"></param>
		public CompressedElement(ElementCrusher crusher, uint cUniform) /*: this()*/
		{
			UnityEngine.Debug.LogWarning("CE Construct");
			this.crusher = crusher;
			this.cUniform = new CompressedFloat(crusher.ucrusher, cUniform);
		}

		// Constructor for Quaternion rotation
		/// <summary>
		/// A uint argument indicates compressed uniform scale. A ulong argument indicates a compressed quaternion.
		/// </summary>
		/// <param name="crusher"></param>
		/// <param name="cQuat"></param>
		/// <param name="qbits"></param>
		public CompressedElement(ElementCrusher crusher, ulong cQuat)/* : this()*/
		{
			UnityEngine.Debug.LogWarning("CE Construct");
			this.crusher = crusher;
			this.cQuat = new CompressedQuat(crusher.qcrusher, cQuat);
		}


		//// Constructor
		//public CompressedElement(ElementCrusher crusher, uint cx, uint cy, uint cz) : this()
		//{
		//	this.crusher = crusher;
		//	this.cx = cx;
		//	this.cy = cy;
		//	this.cz = cz;
		//}

		//// Constructor for half-float
		//public CompressedElement(ElementCrusher ec, ushort cx, ushort cy, ushort cz) : this()
		//{
		//	this.crusher = ec;
		//	this.cx = cx;
		//	this.cy = cy;
		//	this.cz = cz;
		//}

		//// Constructor
		//public CompressedElement(ElementCrusher ec, float x, float y, float z) : this()
		//{
		//	this.crusher = ec;
		//	crusher.xcrusher = x;
		//	floaty = y;
		//	floatz = z;
		//	//floatx = x;
		//	//floaty = y;
		//	//floatz = z;
		//	//this.bitstream = new Bitstream(cx, 32, cy, 32, cz, 32);
		//}


		public void Set(ElementCrusher crusher, CompressedFloat cx, CompressedFloat cy, CompressedFloat cz)
		{
			this.crusher = crusher;
			this.cx = cx;
			this.cy = cy;
			this.cz = cz;
		}
		public void Set(ElementCrusher crusher, uint cx, uint cy, uint cz)
		{
			this.crusher = crusher;
			this.cx = new CompressedFloat(crusher.xcrusher, cx);
			this.cy = new CompressedFloat(crusher.ycrusher, cy);
			this.cz = new CompressedFloat(crusher.zcrusher, cz);
		}
		public void Set(ElementCrusher crusher, uint cUniform) /*: this()*/
		{
			this.crusher = crusher;
			this.cUniform = new CompressedFloat(crusher.ucrusher, cUniform);
		}
		public void Set(ElementCrusher crusher, ulong cQuat)/* : this()*/
		{
			this.crusher = crusher;
			this.cQuat = new CompressedQuat(crusher.qcrusher, cQuat);
		}
		public void CopyTo(CompressedElement copyTarget)
		{
			copyTarget.crusher = this.crusher;
			copyTarget.cx = this.cx;
			copyTarget.cy = this.cy;
			copyTarget.cz = this.cz;
		}
		public void CopyFrom(CompressedElement copySource)
		{
			this.crusher = copySource.crusher;
			this.cx = copySource.cx;
			this.cy = copySource.cy;
			this.cz = copySource.cz;
		}

		// Indexer
		//TODO make these switches
		public uint this[int axis]
		{
			get
			{
				return (axis == 0) ? cx : (axis == 1) ? cy : cz;
			}
		}

		/// <summary>
		/// Serialize all of the contained compressed elements into a new Bitstream struct.
		/// </summary>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public Bitstream ExtractBitstream(BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Bitstream bitstream = new Bitstream();
			crusher.Write(this, ref bitstream, BitCullingLevel.NoCulling);
			return bitstream;
		}

		/// <summary>
		/// Serialize all of the contained compressed elements into a new Bitstream struct.
		/// </summary>
		/// <param name="bcl"></param>
		/// <returns></returns>
		[System.Obsolete("The crusher no longer needs to be supplied, since the crusher that created this CompressedElement should be stored with it.")]
		public Bitstream ExtractBitstream(ElementCrusher crusher, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Bitstream bitstream = new Bitstream();
			crusher.Write(this, ref bitstream, BitCullingLevel.NoCulling);
			return bitstream;
		}

		/// <summary>
		/// Serialize all of the contained compressed elements into an existing Bitstream struct. 
		/// NOTE: This will write starting at the current WritePtr position of the target bitstream.
		/// </summary>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public void ExtractBitstream(ElementCrusher crusher, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			crusher.Write(this, ref bitstream, BitCullingLevel.NoCulling);
		}

		public uint GetUInt(int axis)
		{
			return (axis == 0) ? cx : (axis == 1) ? cy : cz;
		}

		public Element Decompress()
		{
			return crusher.Decompress(this);
		}

		public void Serialize(byte[] buffer, ref int bitposition, IncludedAxes ia, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			crusher.Write(this, buffer, ref bitposition, ia, bcl);
		}

		//public static implicit operator ulong(CompressedElement val)
		//{
		//	return val.bitstream[0];
		//}

		//public static implicit operator CompressedElement(ulong val)
		//{
		//	return new CompressedElement(null, val);
		//}

		/// <summary>
		/// Basic compare of the X, Y, Z, and W values. True if they all match.
		/// </summary>
		[System.Obsolete("Use a.Compare(b) now instead.")]
		public static bool Compare(CompressedElement a, CompressedElement b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// Get the bit count of the highest bit that is different between two compressed positions. This is the min number of bits that must be sent.
		/// </summary>
		/// <returns></returns>
		public static int HighestDifferentBit(uint a, uint b)
		{
			int highestDiffBit = 0;

			// TODO: This needs testing
			for (int i = 0; i < 32; i++)
			{
				uint mask = (uint)1 << i;

				if ((a & mask) == (b & mask))
				{
					highestDiffBit = i;
				}
			}
			return highestDiffBit;
		}

		//public static CompressedElement operator +(CompressedElement a, CompressedElement b)
		//{
		//	return new CompressedElement(a.crusher, (uint)((long)a.cx + b.cx), (uint)((long)a.cy + b.cy), (uint)((long)a.cz + b.cz));
		//}
		//public static CompressedElement operator -(CompressedElement a, CompressedElement b)
		//{
		//	return new CompressedElement(a.crusher, (uint)((long)a.cx - b.cx), (uint)((long)a.cy - b.cy), (uint)((long)a.cz - b.cz));
		//}
		//public static CompressedElement operator *(CompressedElement a, float b)
		//{
		//	return new CompressedElement(a.crusher, (uint)(a.cx * b), (uint)(a.cy * b), (uint)(a.cz * b));
		//}

		///<summary>
		/// It may preferable to use the overload that takes and int divisor value than a float, to avoid all float math to possibly reduce jitter.
		/// </summary>
		public static void Extrapolate(ElementCrusher crusher, CompressedElement target, CompressedElement curr, CompressedElement prev, int divisor = 2)
		{
			target.Set
				(
				crusher,
				(uint)(curr.cx + (((long)curr.cx - prev.cx)) / divisor),
				(uint)(curr.cy + (((long)curr.cy - prev.cy)) / divisor),
				(uint)(curr.cz + (((long)curr.cz - prev.cz)) / divisor)
				);
		}
		[System.Obsolete]
		public static CompressedElement Extrapolate(ElementCrusher crusher, CompressedElement curr, CompressedElement prev, int divisor = 2)
		{
			return new CompressedElement
				(
				crusher,
				(uint)(curr.cx + (((long)curr.cx - prev.cx)) / divisor),
				(uint)(curr.cy + (((long)curr.cy - prev.cy)) / divisor),
				(uint)(curr.cz + (((long)curr.cz - prev.cz)) / divisor)
				);
		}
		///<summary>
		/// It may preferable to use the overload that takes and int divisor value than a float, to avoid all float math to possibly reduce jitter.
		/// Uses curr.crusher.
		/// </summary>
		public static void Extrapolate(CompressedElement target, CompressedElement curr, CompressedElement prev, int divisor = 2)
		{
			target.Set
				(
				curr.crusher,
				(uint)(curr.cx + (((long)curr.cx - prev.cx)) / divisor),
				(uint)(curr.cy + (((long)curr.cy - prev.cy)) / divisor),
				(uint)(curr.cz + (((long)curr.cz - prev.cz)) / divisor)
				);
		}
		[System.Obsolete]
		public static CompressedElement Extrapolate(CompressedElement curr, CompressedElement prev, int divisor = 2)
		{
			return new CompressedElement
				(
				curr.crusher,
				(uint)(curr.cx + (((long)curr.cx - prev.cx)) / divisor),
				(uint)(curr.cy + (((long)curr.cy - prev.cy)) / divisor),
				(uint)(curr.cz + (((long)curr.cz - prev.cz)) / divisor)
				);
		}
		/// <summary>
		/// It may preferable to use the overload that takes and int divisor value than a float, to avoid all float math to possibly reduce jitter.
		/// </summary>
		[System.Obsolete()]
		public static void Extrapolate(ElementCrusher crusher, CompressedElement target, CompressedElement curr, CompressedElement prev, float amount = .5f)
		{
			//int divisor = (int)(1f / amount);
			//return Extrapolate(curr, prev, divisor);
			target.Set
				(
				crusher,
				(uint)(curr.cx + ((long)curr.cx - prev.cx) * amount),
				(uint)(curr.cy + ((long)curr.cy - prev.cy) * amount),
				(uint)(curr.cz + ((long)curr.cz - prev.cz) * amount)
				);
		}
		[System.Obsolete]
		public static CompressedElement Extrapolate(ElementCrusher crusher, CompressedElement curr, CompressedElement prev, float amount = .5f)
		{
			//int divisor = (int)(1f / amount);
			//return Extrapolate(curr, prev, divisor);
			return new CompressedElement
				(
				crusher,
				(uint)(curr.cx + ((long)curr.cx - prev.cx) * amount),
				(uint)(curr.cy + ((long)curr.cy - prev.cy) * amount),
				(uint)(curr.cz + ((long)curr.cz - prev.cz) * amount)
				);
		}
		/// <summary>
		/// It may preferable to use the overload that takes and int divisor value than a float, to avoid all float math to possibly reduce jitter.
		/// </summary>
		public static void Extrapolate(CompressedElement target, CompressedElement curr, CompressedElement prev, float amount = .5f)
		{
			//int divisor = (int)(1f / amount);
			//return Extrapolate(curr, prev, divisor);
			target.Set
				(
				curr.crusher,
				(uint)(curr.cx + ((long)curr.cx - prev.cx) * amount),
				(uint)(curr.cy + ((long)curr.cy - prev.cy) * amount),
				(uint)(curr.cz + ((long)curr.cz - prev.cz) * amount)
				);
		}
		[System.Obsolete()]
		public static CompressedElement Extrapolate(CompressedElement curr, CompressedElement prev, float amount = .5f)
		{
			//int divisor = (int)(1f / amount);
			//return Extrapolate(curr, prev, divisor);
			return new CompressedElement
				(
				curr.crusher,
				(uint)(curr.cx + ((long)curr.cx - prev.cx) * amount),
				(uint)(curr.cy + ((long)curr.cy - prev.cy) * amount),
				(uint)(curr.cz + ((long)curr.cz - prev.cz) * amount)
				);
		}

		private static CompressedElement uppers = new CompressedElement();
		private static CompressedElement lowers = new CompressedElement();
		/// <summary>
		/// Test changes between two compressed Vector3 elements and returns the ideal BitCullingLevel for that change, with the assumption
		/// that it will be recreated using best guess (process where the upperbits are incremented and deincremented until the closest position
		/// to compare positon is returned.
		/// NOT FULLY TESTED
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="maxCullLvl"></param>
		/// <returns></returns>
		public static BitCullingLevel GetGuessableBitCullLevel(CompressedElement a, CompressedElement b, BitCullingLevel maxCullLvl)
		{
			for (BitCullingLevel lvl = maxCullLvl; lvl > 0; lvl--)
			{
				a.ZeroLowerBits(uppers, lvl);
				b.ZeroUpperBits(lowers, lvl);

				if ((uppers.cx | lowers.cx) == b.cx &&
					(uppers.cy | lowers.cy) == b.cy &&
					(uppers.cz | lowers.cz) == b.cz)
					return lvl;
			}
			return BitCullingLevel.NoCulling;
		}
		[System.Obsolete()]
		public static BitCullingLevel GetGuessableBitCullLevel(CompressedElement oldComp, CompressedElement newComp, ElementCrusher ec, BitCullingLevel maxCullLvl)
		{
			for (BitCullingLevel lvl = maxCullLvl; lvl > 0; lvl--)
			{
				oldComp.ZeroLowerBits(uppers, lvl);
				newComp.ZeroUpperBits(lowers, lvl);

				if ((uppers.cx | lowers.cx) == newComp.cx &&
					(uppers.cy | lowers.cy) == newComp.cy &&
					(uppers.cz | lowers.cz) == newComp.cz)
					return lvl;
			}
			return BitCullingLevel.NoCulling;
		}

		///// <summary>
		///// Test changes between two compressed Vector3 elements and return the ideal BitCullingLevel for that change.
		///// </summary>
		//public static BitCullingLevel GetGuessableBitCullLevel(CompressedElement oldComp, CompressedElement newComp, FloatCrusher[] ec, BitCullingLevel maxCullLvl)
		//{
		//	for (BitCullingLevel lvl = maxCullLvl; lvl > 0; lvl--)
		//	{
		//		CompressedElement uppers = oldComp.ZeroLowerBits(ec, lvl);
		//		CompressedElement lowers = newComp.ZeroUpperBits(ec, lvl);
		//		if ((uppers.cx | lowers.cx) == newComp.cx &&				
		//			(uppers.cy | lowers.cy) == newComp.cy &&
		//			(uppers.cz | lowers.cz) == newComp.cz)
		//			return lvl;
		//	}
		//	return BitCullingLevel.NoCulling;
		//}

		/// <summary>
		/// Return the smallest bit culling level that will be able to communicate the changes between two compressed elements.
		/// </summary>
		public static BitCullingLevel FindBestBitCullLevel(CompressedElement a, CompressedElement b, BitCullingLevel maxCulling)
		{
			ElementCrusher ec = a.crusher;
			if (ec == null)
			{
				UnityEngine.Debug.Log("NUL CE CRUSHER FindBestBitCullLevel");
				return BitCullingLevel.NoCulling;
			}

			/// Quats can't cull upper bits, so its an all or nothing. Either the bits match or they don't
			if (ec.TRSType == TRSType.Quaternion)
			{
				if (a.cQuat == b.cQuat)
					return BitCullingLevel.DropAll;
				else
					return BitCullingLevel.NoCulling;
			}

			if (maxCulling == BitCullingLevel.NoCulling || !TestMatchingUpper(a, b, BitCullingLevel.DropThird))
				return BitCullingLevel.NoCulling;

			if (maxCulling == BitCullingLevel.DropThird || !TestMatchingUpper(a, b, BitCullingLevel.DropHalf))
				return BitCullingLevel.DropThird;

			if (maxCulling == BitCullingLevel.DropHalf || !TestMatchingUpper(a, b, BitCullingLevel.DropAll))
				return BitCullingLevel.DropHalf;

			// both values are the same
			return BitCullingLevel.DropAll;
		}
		[System.Obsolete()]
		public static BitCullingLevel FindBestBitCullLevel(CompressedElement a, CompressedElement b, ElementCrusher ec, BitCullingLevel maxCulling)
		{
			/// Quats can't cull upper bits, so its an all or nothing. Either the bits match or they don't
			if (ec.TRSType == TRSType.Quaternion)
			{
				if (a.cQuat == b.cQuat)
					return BitCullingLevel.DropAll;
				else
					return BitCullingLevel.NoCulling;
			}

			if (maxCulling == BitCullingLevel.NoCulling || !TestMatchingUpper(a, b, ec, BitCullingLevel.DropThird))
				return BitCullingLevel.NoCulling;

			if (maxCulling == BitCullingLevel.DropThird || !TestMatchingUpper(a, b, ec, BitCullingLevel.DropHalf))
				return BitCullingLevel.DropThird;

			if (maxCulling == BitCullingLevel.DropHalf || !TestMatchingUpper(a, b, ec, BitCullingLevel.DropAll))
				return BitCullingLevel.DropHalf;

			// both values are the same
			return BitCullingLevel.DropAll;
		}
		[System.Obsolete()]
		public static BitCullingLevel FindBestBitCullLevel(CompressedElement a, CompressedElement b, FloatCrusher[] ec, BitCullingLevel maxCulling)
		{
			if (maxCulling == BitCullingLevel.NoCulling || !TestMatchingUpper(a, b, ec, BitCullingLevel.DropThird))
				return BitCullingLevel.NoCulling;

			if (maxCulling == BitCullingLevel.DropThird || !TestMatchingUpper(a, b, ec, BitCullingLevel.DropHalf))
				return BitCullingLevel.DropThird;

			if (maxCulling == BitCullingLevel.DropHalf || !TestMatchingUpper(a, b, ec, BitCullingLevel.DropAll))
				return BitCullingLevel.DropHalf;

			// both values are the same
			return BitCullingLevel.DropAll;
		}

		private static bool TestMatchingUpper(uint a, uint b, int lowerbits)
		{
			return (((a >> lowerbits) << lowerbits) == ((b >> lowerbits) << lowerbits));
		}
		public static bool TestMatchingUpper(CompressedElement a, CompressedElement b, BitCullingLevel bcl)
		{
			ElementCrusher ec = a.crusher;
			return
				(
				TestMatchingUpper(a.cx, b.cx, ec.xcrusher.GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cy, b.cy, ec.ycrusher.GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cz, b.cz, ec.zcrusher.GetBitsAtCullLevel(bcl))
				);
		}
		[System.Obsolete()]
		public static bool TestMatchingUpper(CompressedElement a, CompressedElement b, ElementCrusher ec, BitCullingLevel bcl)
		{
			return
				(
				TestMatchingUpper(a.cx, b.cx, ec[0].GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cy, b.cy, ec[1].GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cz, b.cz, ec[2].GetBitsAtCullLevel(bcl))
				);
		}
		[System.Obsolete()]
		public static bool TestMatchingUpper(CompressedElement a, CompressedElement b, FloatCrusher[] ec, BitCullingLevel bcl)
		{
			return
				(
				TestMatchingUpper(a.cx, b.cx, ec[0].GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cy, b.cy, ec[1].GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cz, b.cz, ec[2].GetBitsAtCullLevel(bcl))
				);
		}


		//public static bool operator ==(CompressedElement a, CompressedElement b)
		//{
		//	return a.Equals(b);
		//}
		//public static bool operator !=(CompressedElement a, CompressedElement b)
		//{
		//	return !(a.Equals(b));
		//}
		public override string ToString()
		{
			if (crusher == null)
				return "[Empty CompElement]";

			if (crusher.TRSType == TRSType.Quaternion)
				return crusher.TRSType + " [" + cQuat.cvalue + "]";

			if (crusher.TRSType == TRSType.Scale && crusher.uniformAxes != ElementCrusher.UniformAxes.NonUniform)
				return crusher.TRSType + " [" + crusher.uniformAxes + " : " + cUniform.cvalue + "]";

			return crusher.TRSType + " [x:" + cx.cvalue + " y:" + cy.cvalue + " z:" + cz.cvalue + "]";
		}


		public static bool operator ==(CompressedElement a, CompressedElement b)
		{
			if (ReferenceEquals(a, null))
				return false;

			return a.Equals(b);
		}
		public static bool operator !=(CompressedElement a, CompressedElement b)
		{
			if (ReferenceEquals(a, null))
				return true;

			return !a.Equals(b);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as CompressedElement);
		}

		public bool Equals(CompressedElement other)
		{
			//if (crusher != other.crusher)
			//	return false;

			return
				!ReferenceEquals(other, null) &&
				cx.cvalue == other.cx.cvalue &&
				cy.cvalue == other.cy.cvalue &&
				cz.cvalue == other.cz.cvalue &&
				cUniform.cvalue == other.cUniform.cvalue &&
				cQuat.cvalue == other.cQuat.cvalue;
		}

		public override int GetHashCode()
		{
			var hashCode = -1337834834;
			hashCode = hashCode * -1521134295 + cx.GetHashCode();
			hashCode = hashCode * -1521134295 + cy.GetHashCode();
			hashCode = hashCode * -1521134295 + cz.GetHashCode();
			hashCode = hashCode * -1521134295 + cUniform.GetHashCode();
			hashCode = hashCode * -1521134295 + cQuat.GetHashCode();
			return hashCode;
		}
	}

	public static class CompressedElementExt
	{
		public static System.UInt32[] reusableInts = new System.UInt32[3];

		public static void GetChangeAmount(uint[] results, CompressedElement a, CompressedElement b)
		{
			for (int i = 0; i < 3; i++)
				results[i] = (System.UInt32)System.Math.Abs(a[i] - b[0]);
		}
		[System.Obsolete()]
		public static uint[] GetChangeAmount(CompressedElement a, CompressedElement b)
		{
			for (int i = 0; i < 3; i++)
				reusableInts[i] = (System.UInt32)System.Math.Abs(a[i] - b[0]);

			return reusableInts;
		}

		/// <summary>
		/// Alternative to OverwriteUpperBits that attempts to guess the upperbits by seeing if each axis of the new position would be
		/// closer to the old one if the upper bit is incremented by one, two, three etc. Stops trying when it fails to get a better result.
		/// </summary>
		/// <param name="oldcpos">Last best position test against.</param>
		/// <returns>Returns a corrected CompressPos</returns>
		public static void GuessUpperBits(this CompressedElement newcpos, ElementCrusher ec, CompressedElement oldcpos, BitCullingLevel bcl)
		{
			//var crusher = oldcpos.crusher;
			newcpos.Set(
				ec,
				ec.xcrusher.GuessUpperBits(newcpos[0], oldcpos[0], bcl),
				ec.ycrusher.GuessUpperBits(newcpos[1], oldcpos[1], bcl),
				ec.zcrusher.GuessUpperBits(newcpos[2], oldcpos[2], bcl)
				);
		}
		[System.Obsolete()]
		public static CompressedElement GuessUpperBits(this CompressedElement newcpos, CompressedElement oldcpos, ElementCrusher ec, BitCullingLevel bcl)
		{
			//var crusher = oldcpos.crusher;
			return new CompressedElement(
				ec,
				ec.xcrusher.GuessUpperBits(newcpos[0], oldcpos[0], bcl),
				ec.ycrusher.GuessUpperBits(newcpos[1], oldcpos[1], bcl),
				ec.zcrusher.GuessUpperBits(newcpos[2], oldcpos[2], bcl)
				);
		}
		//public static CompressedElement GuessUpperBits(this CompressedElement newcpos, CompressedElement oldcpos, FloatCrusher[] ec, BitCullingLevel bcl)
		//{
		//	var crusher = oldcpos.crusher;
		//	return new CompressedElement(
		//		crusher,
		//		new CompressedValue(crusher.xcrusher, ec[0].GuessUpperBits(newcpos.cx, oldcpos.cx, bcl), crusher.xcrusher.GetBits(bcl)),
		//		new CompressedValue(crusher.ycrusher, ec[1].GuessUpperBits(newcpos.cx, oldcpos.cx, bcl), crusher.xcrusher.GetBits(bcl)),
		//		new CompressedValue(crusher.zcrusher, ec[2].GuessUpperBits(newcpos.cx, oldcpos.cx, bcl), crusher.xcrusher.GetBits(bcl))
		//		);
		//}

		/// <summary>
		/// Replace the upperbits of the first compressed element with the upper bits of the second, using BitCullingLevel as the separation point.
		/// </summary>
		public static void OverwriteUpperBits(this CompressedElement low, CompressedElement uppers, BitCullingLevel bcl)
		{
			ElementCrusher ec = low.crusher;
			low.Set(
				ec,
				ec.xcrusher.OverwriteUpperBits(low.cx, uppers.cx, bcl),
				ec.ycrusher.OverwriteUpperBits(low.cy, uppers.cy, bcl),
				ec.zcrusher.OverwriteUpperBits(low.cz, uppers.cz, bcl)
				);
		}
		[System.Obsolete()]
		public static CompressedElement OverwriteUpperBits(this CompressedElement low, CompressedElement up, ElementCrusher ec, BitCullingLevel bcl)
		{
			return new CompressedElement(
				ec,
				ec[0].OverwriteUpperBits(low.cx, up.cx, bcl),
				ec[1].OverwriteUpperBits(low.cy, up.cy, bcl),
				ec[2].OverwriteUpperBits(low.cz, up.cz, bcl)
				);
		}
		//public static CompressedElement OverwriteUpperBits(this CompressedElement low, CompressedElement up, FloatCrusher[] ec, BitCullingLevel bcl)
		//{
		//	return new CompressedElement(
		//		//ec,
		//		ec[0].OverwriteUpperBits(low.cx, up.cx, bcl),
		//		ec[1].OverwriteUpperBits(low.cy, up.cy, bcl),
		//		ec[2].OverwriteUpperBits(low.cz, up.cz, bcl)
		//		);
		//}

		public static void ZeroLowerBits(this CompressedElement fullpos, CompressedElement target, BitCullingLevel bcl)
		{
			ElementCrusher ec = fullpos.crusher;
			target.Set(
				ec, //fullpos.crusher,
				ec.xcrusher.ZeroLowerBits(fullpos.cx, bcl),
				ec.ycrusher.ZeroLowerBits(fullpos.cy, bcl),
				ec.zcrusher.ZeroLowerBits(fullpos.cz, bcl)
				);
		}
		[System.Obsolete()]
		public static CompressedElement ZeroLowerBits(this CompressedElement fullpos, ElementCrusher ec, BitCullingLevel bcl)
		{
			return new CompressedElement(
				ec, //fullpos.crusher,
				ec[0].ZeroLowerBits(fullpos.cx, bcl),
				ec[1].ZeroLowerBits(fullpos.cy, bcl),
				ec[2].ZeroLowerBits(fullpos.cz, bcl)
				);
		}
		//public static CompressedElement ZeroLowerBits(this CompressedElement fullpos, FloatCrusher[] ec, BitCullingLevel bcl)
		//{
		//	return new CompressedElement(
		//		fullpos.crusher,
		//		ec[0].ZeroLowerBits(fullpos.cx, bcl),
		//		ec[1].ZeroLowerBits(fullpos.cy, bcl),
		//		ec[2].ZeroLowerBits(fullpos.cz, bcl)
		//		);
		//}
		public static void ZeroUpperBits(this CompressedElement fullpos, CompressedElement target, BitCullingLevel bcl)
		{
			ElementCrusher ec = fullpos.crusher;
			target.Set(
				ec, /*fullpos.crusher,*/
				ec.xcrusher.ZeroUpperBits(fullpos.cx, bcl),
				ec.ycrusher.ZeroUpperBits(fullpos.cy, bcl),
				ec.zcrusher.ZeroUpperBits(fullpos.cz, bcl)
				);
		}
		[System.Obsolete()]
		public static CompressedElement ZeroUpperBits(this CompressedElement fullpos, ElementCrusher ec, BitCullingLevel bcl)
		{
			return new CompressedElement(
				ec, /*fullpos.crusher,*/
				ec[0].ZeroUpperBits(fullpos.cx, bcl),
				ec[1].ZeroUpperBits(fullpos.cy, bcl),
				ec[2].ZeroUpperBits(fullpos.cz, bcl)
				);
		}
		//public static CompressedElement ZeroUpperBits(this CompressedElement fullpos, FloatCrusher[] ec, BitCullingLevel bcl)
		//{
		//	return new CompressedElement(
		//		ec, /*fullpos.crusher,*/
		//		ec[0].ZeroUpperBits(fullpos.cx, bcl),
		//		ec[1].ZeroUpperBits(fullpos.cy, bcl),
		//		ec[2].ZeroUpperBits(fullpos.cz, bcl)
		//		);
		//}

	}
}
