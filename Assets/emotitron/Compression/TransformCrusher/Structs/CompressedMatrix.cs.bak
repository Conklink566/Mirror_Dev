//Copyright 2018, Davin Carten, All rights reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Compression
{
	/// <summary>
	/// This class contains CompressedElement classes for the compressed Position, Rotation and Scale,
	/// and exposes methods for comparing, copying, serialiing and deserializing the entire collection in one call.
	/// </summary>
	public class CompressedMatrix : IEquatable<CompressedMatrix>
	{
		public CompressedElement cPos = new CompressedElement();
		public CompressedElement cRot = new CompressedElement();
		public CompressedElement cScl = new CompressedElement();

		public TransformCrusher crusher;

		#region

		// Constructor
		public CompressedMatrix()
		{
		}

		public CompressedMatrix(TransformCrusher crusher)
		{
			this.crusher = crusher;
		}
		// Constructor
		public CompressedMatrix(TransformCrusher crusher, CompressedElement cPos, CompressedElement cRot, CompressedElement cScl)
		{
			this.crusher = crusher;
			this.cPos = cPos;
			this.cRot = cRot;
			this.cScl = cScl;
		}

		// Constructor
		public CompressedMatrix(TransformCrusher crusher, ref CompressedElement cPos, ref CompressedElement cRot, ref CompressedElement cScl, int pBits, int rBits, int sBits)
		{
			this.crusher = crusher;
			this.cPos = cPos;
			this.cRot = cRot;
			this.cScl = cScl;
		}

		#endregion

		public void CopyTo(CompressedMatrix copyTarget)
		{
			cPos.CopyTo(copyTarget.cPos);
			cRot.CopyTo(copyTarget.cRot);
			cScl.CopyTo(copyTarget.cScl);
		}
		public void CopyFrom(CompressedMatrix copySource)
		{
			cPos.CopyFrom(copySource.cPos);
			cRot.CopyFrom(copySource.cRot);
			cScl.CopyFrom(copySource.cScl);
		}

		public void Clear()
		{
			crusher = null;
			cPos.Clear();
			cRot.Clear();
			cScl.Clear();
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
		/// Serialize all of the contained compressed elements into an existing Bitstream struct. 
		/// NOTE: This will write starting at the current WritePtr position of the target bitstream.
		/// </summary>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public void ExtractBitstream(ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			crusher.Write(this, ref bitstream, BitCullingLevel.NoCulling);
		}

		public void Read(ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (crusher != null)
			{
				crusher.Read(this, ref bitstream, bcl);
			}
			else
			{
				Debug.LogError("CompressedMatrix does not have a crusher associated with it.");
			}
		}
		public void Read(TransformCrusher crusher, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (crusher != null)
			{
				crusher.Read(this, ref bitstream, bcl);
			}
			else
			{
				Debug.LogError("CompressedMatrix does not have a crusher associated with it.");
			}
		}

		//public ulong this[int i]
		//{
		//	get
		//	{
		//		return bitstream[i];
		//	}
		//}

		/// <summary>
		/// Decompress this CompressedMatrix and put the TRS (Position, Rotation, Scale) values into the supplied Matrix class.
		/// </summary>
		/// <param name="nonalloc">The target for the uncompressed TRS.</param>
		public void Decompress(Matrix nonalloc)
		{
			if (crusher != null)
				crusher.Decompress(nonalloc, this);
			else
				nonalloc.Clear();
		}
		[System.Obsolete("Use the nonalloc overload instead and supply a target CompressedMatrix. CompressedMatrix is now a class rather than a struct")]
		public Matrix Decompress()
		{
			return crusher.Decompress(this);
		}

		[System.Obsolete("Supply the transform to Compress. Default Transform has been deprecated to allow shared TransformCrushers.")]
		public void Apply()
		{
			if (crusher != null)
				crusher.Apply(this);
		}
		public void Apply(Transform t)
		{
			if (crusher != null)
				crusher.Apply(t, this);
		}

		public static bool operator ==(CompressedMatrix a, CompressedMatrix b)
		{
			if (ReferenceEquals(a, null))
				return false;

			return a.Equals(b);
		}
		public static bool operator !=(CompressedMatrix a, CompressedMatrix b)
		{
			if (ReferenceEquals(a, null))
				return true;

			return !a.Equals(b);
		}

		public override string ToString()
		{
			return "cpos: " + cPos + "\ncrot: " + cRot + "\nsrot " + cScl + "\n compressed matric composite: " + ExtractBitstream();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as CompressedMatrix);
		}

		/// <summary>
		/// Compare the values of this CompressedMatrix with the values of another.
		/// </summary>
		/// <param name="other"></param>
		/// <returns>True if the values match, false if not.</returns>
		public bool Equals(CompressedMatrix other)
		{
			return
				!ReferenceEquals(other, null) &&
				cPos.Equals(other.cPos) &&
				cRot.Equals(other.cRot) &&
				cScl.Equals(other.cScl);
		}

		public override int GetHashCode()
		{
			var hashCode = 94804922;
			hashCode = hashCode * -1521134295 + cPos.GetHashCode();
			hashCode = hashCode * -1521134295 + cRot.GetHashCode();
			hashCode = hashCode * -1521134295 + cScl.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<TransformCrusher>.Default.GetHashCode(crusher);
			return hashCode;
		}
	}
}


