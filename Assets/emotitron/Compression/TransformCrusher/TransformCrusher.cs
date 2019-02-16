//Copyright 2018, Davin Carten, All rights reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Compression
{
	/// <summary>
	/// VERY basic interface, just to make it easy to find transform crusher in another example component.
	/// </summary>
	public interface IHasTransformCrusher
	{
		TransformCrusher TC { get; }
	}

	[System.Serializable]
	public class TransformCrusher : Crusher<TransformCrusher>, IOnElementCrusherChange, ICrusherCopy<TransformCrusher>
	{
		
		#region Static Crushers

		public static Dictionary<int, TransformCrusher> staticTransformCrushers = new Dictionary<int, TransformCrusher>();

		/// <summary>
		/// See if a crusher with these exact settings exists in the static crushers list. If so, return that already
		/// cataloged crusher. You may allow the crusher given as an argument be garbage collected. NOTE: Any changes to static crushers
		/// will break things. Currently there are no safeguards against this.
		/// </summary>
		/// <param name="tc"></param>
		/// <returns></returns>
		public static TransformCrusher CheckAgainstStatics(TransformCrusher tc, bool CheckElementCrusherAsWell = true)
		{
			if (ReferenceEquals(tc, null))
				return null;

			if (CheckElementCrusherAsWell)
			{
				tc.posCrusher = ElementCrusher.CheckAgainstStatics(tc.posCrusher);
				tc.rotCrusher = ElementCrusher.CheckAgainstStatics(tc.rotCrusher);
				tc.sclCrusher = ElementCrusher.CheckAgainstStatics(tc.sclCrusher);
			}

			int hash = tc.GetHashCode();
			if (staticTransformCrushers.ContainsKey(hash))
			{
				return staticTransformCrushers[hash];
			}

			staticTransformCrushers.Add(hash, tc);
			return tc;
		}

		#endregion

		[HideInInspector]
		[System.Obsolete("Default Transform breaks crusher sharing across multiple instances and is now deprecated.")]
		[Tooltip("This is the default assumed transform when no transform or gameobject is given to methods.")]
		public Transform defaultTransform;

		// Set up the default Crushers so they add up to 64 bits
		[SerializeField] private ElementCrusher posCrusher;
		[SerializeField] private ElementCrusher rotCrusher;
		[SerializeField] private ElementCrusher sclCrusher;

		public TransformCrusher()
		{
			ConstructDefault(false);
		}
		/// <summary>
		/// Default constructor for TransformCrusher.
		/// </summary>
		/// <param name="isStatic">Set this as true if this crusher is not meant to be serialized. Static crushers are created in code, and are not meant to be modified after creation.
		/// This allows them to be indexed by their hashcodes and reused.
		/// </param>
		public TransformCrusher(bool isStatic = false)
		{
			ConstructDefault(isStatic);
		}

		private void ConstructDefault(bool isStatic = false)
		{
			if (isStatic)
			{
				// Statics initialize all crushers as null.
			}
			else
			{
				PosCrusher = new ElementCrusher(TRSType.Position, false);
				RotCrusher = new ElementCrusher(TRSType.Euler, false)
				{
					xcrusher = new FloatCrusher(BitPresets.Bits12, -90f, 90f, Axis.X, TRSType.Euler, true),
					ycrusher = new FloatCrusher(BitPresets.Bits12, -180f, 180f, Axis.Y, TRSType.Euler, true),
					zcrusher = new FloatCrusher(BitPresets.Disabled, -180f, 180f, Axis.Z, TRSType.Euler, true)
				};
				SclCrusher = new ElementCrusher(TRSType.Scale, false)
				{
					uniformAxes = ElementCrusher.UniformAxes.XYZ,
					ucrusher = new FloatCrusher(8, 0f, 2f, Axis.Uniform, TRSType.Scale, true)
				};
			}
		}

		/// <summary>
		/// Sets the position crusher to the assigned reference, and reruns CacheValues().
		/// </summary>
		public ElementCrusher PosCrusher
		{
			get { return posCrusher; }
			set
			{
				if (posCrusher != null)
					posCrusher.onChangeCallbacks.Remove(this);

				posCrusher = value;
				CacheValues();

				if (posCrusher != null)
					posCrusher.onChangeCallbacks.Add(this);
			}
		}
		/// <summary>
		/// Sets the scale crusher to the assigned reference, and reruns CacheValues().
		/// </summary>
		public ElementCrusher RotCrusher
		{
			get { return rotCrusher; }
			set
			{
				if (rotCrusher != null)
					rotCrusher.onChangeCallbacks.Remove(this);

				rotCrusher = value;
				CacheValues();

				if (rotCrusher != null)
					rotCrusher.onChangeCallbacks.Add(this);
			}
		}
		/// <summary>
		/// Sets the scale crusher to the assigned reference, and reruns CacheValues().
		/// </summary>
		public ElementCrusher SclCrusher
		{
			get { return sclCrusher; }
			set
			{
				if (sclCrusher != null)
					sclCrusher.onChangeCallbacks.Remove(this);

				sclCrusher = value;
				CacheValues();

				if (sclCrusher != null)
					sclCrusher.onChangeCallbacks.Add(this);
			}
		}

#if UNITY_EDITOR
#pragma warning disable 0414
		[SerializeField]
		private bool isExpanded = true;
#pragma warning restore 0414
#endif

		public void OnCrusherChange(ElementCrusher ec)
		{
			CacheValues();
		}

		/// Temporary CompressedMatrix used internally when a non-alloc is not provided and no return CM or M is required.
		private static readonly CompressedMatrix reusableCM = new CompressedMatrix();
		private static readonly Matrix reusableM = new Matrix();


		#region Cached compression values

		private int cached_pBits, cached_rBits, cached_sBits;

		private bool cached;

		public void CacheValues()
		{
			this.
			cached_pBits = (posCrusher == null) ? 0 : posCrusher.TallyBits();
			cached_rBits = (rotCrusher == null) ? 0 : rotCrusher.TallyBits();
			cached_sBits = (sclCrusher == null) ? 0 : sclCrusher.TallyBits();
			cached = true;
		}

		#endregion

		#region Byte[] Writers

		public void Write(CompressedMatrix cm, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cached_pBits > 0)
				posCrusher.Write(cm.cPos, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
			if (cached_rBits > 0)
				rotCrusher.Write(cm.cRot, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
			if (cached_sBits > 0)
				sclCrusher.Write(cm.cScl, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
		}

		[System.Obsolete("Default Transform is being removed, and all operations that require a transform need to explicitly supply one. Default Transform breaks the ability to share crushers across multiple objects.")]
		public Bitstream Write(byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Debug.Assert(defaultTransform, transformMissingError);

			Write(reusableCM, defaultTransform, buffer, ref bitposition, bcl);
			return reusableCM.ExtractBitstream();
		}

		public void Write(CompressedMatrix nonalloc, Transform transform, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			nonalloc.crusher = this;
			if (cached_pBits > 0)
				posCrusher.Write(nonalloc.cPos, transform, buffer, ref bitposition, bcl);
			if (cached_rBits > 0)
				rotCrusher.Write(nonalloc.cRot, transform, buffer, ref bitposition, bcl);
			if (cached_sBits > 0)
				sclCrusher.Write(nonalloc.cScl, transform, buffer, ref bitposition, bcl);
		}
		public Bitstream Write(Transform transform, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Write(reusableCM, transform, buffer, ref bitposition, bcl);
			return reusableCM.ExtractBitstream();
		}

		#endregion

		#region Byte[] Readers

		[System.Obsolete()]
		public Matrix ReadAndDecompress(byte[] array, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			return ReadAndDecompress(array, ref bitposition, bcl);
		}

		// Skips intermediate step of creating a compressedMatrx
		public void ReadAndDecompress(Matrix nonalloc, byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, array, ref bitposition, bcl);
			Decompress(nonalloc, reusableCM);
		}
		[System.Obsolete()]
		public Matrix ReadAndDecompress(byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			return new Matrix(
				this,
				(cached_pBits > 0) ? posCrusher.Decompress(posCrusher.Read(array, ref bitposition, bcl)) : new Element(),
				(cached_rBits > 0) ? rotCrusher.Decompress(rotCrusher.Read(array, ref bitposition, bcl)) : new Element(),
				(cached_sBits > 0) ? sclCrusher.Decompress(sclCrusher.Read(array, ref bitposition, bcl)) : new Element()
				);
		}

		// UNTESTED
		public void Read(CompressedMatrix nonalloc, byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			nonalloc.crusher = this;
			if (cached_pBits > 0)
				posCrusher.Read(nonalloc.cPos, array, ref bitposition, IncludedAxes.XYZ, bcl);
			if (cached_rBits > 0)
				rotCrusher.Read(nonalloc.cRot, array, ref bitposition, IncludedAxes.XYZ, bcl);
			if (cached_sBits > 0)
				sclCrusher.Read(nonalloc.cScl, array, ref bitposition, IncludedAxes.XYZ, bcl);
		}
		// UNTESTED
		public Bitstream Read(byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, array, ref bitposition, bcl);
			return reusableCM.ExtractBitstream();
		}


		#endregion

		#region ULong Buffer Writers

		/// <summary>
		/// Compress a transform using this crusher, store the compressed results in a supplied CompressedMatrix, and serialize the compressed values to the buffer.
		/// </summary>
		/// <param name="nonalloc">Populate this CompressedMatrix with the results of the cmopression operation.</param>
		/// <param name="transform">The transform to compress.</param>
		/// <param name="buffer">The write target.</param>
		/// <param name="bitposition">The write position for the buffer.</param>
		/// <param name="bcl"></param>
		public void Write(CompressedMatrix nonalloc, Transform transform, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			nonalloc.crusher = this;
			if (cached_pBits > 0)
				posCrusher.Write(nonalloc.cPos, transform, ref buffer, ref bitposition, bcl);
			if (cached_rBits > 0)
				rotCrusher.Write(nonalloc.cRot, transform, ref buffer, ref bitposition, bcl);
			if (cached_sBits > 0)
				sclCrusher.Write(nonalloc.cScl, transform, ref buffer, ref bitposition, bcl);
		}

		/// <summary>
		/// Compress and write all of the components of transform, without creating any intermediary CompressedMatrix or Bitstream. This is the most efficient way to
		/// compress and write a transform, but it will not return any compresed values for you to store or compare.
		/// </summary>
		/// <param name="transform">The transform to compress.</param>
		/// <param name="buffer">The write target.</param>
		/// <param name="bitposition">The write position for the buffer.</param>
		/// <param name="bcl"></param>
		public void Write(Transform transform, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cached_pBits > 0)
				posCrusher.Write(transform, ref buffer, ref bitposition, bcl);
			if (cached_rBits > 0)
				rotCrusher.Write(transform, ref buffer, ref bitposition, bcl);
			if (cached_sBits > 0)
				sclCrusher.Write(transform, ref buffer, ref bitposition, bcl);
		}

		/// <summary>
		/// Serialize a CompressedMatrix to a bitstream.
		/// </summary>
		/// <param name="cm">Results of a previously compressed Transform Matrix.</param>
		/// <param name="bitstream">The write target.</param>
		/// <param name="bcl"></param>
		public void Write(CompressedMatrix cm, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (cached_pBits > 0)
				posCrusher.Write(cm.cPos, ref bitstream, bcl);
			if (cached_rBits > 0)
				rotCrusher.Write(cm.cRot, ref bitstream, bcl);
			if (cached_sBits > 0)
				sclCrusher.Write(cm.cScl, ref bitstream, bcl);
		}

		/// <summary>
		/// Compress a transform using this crusher, store the compressed results in a supplied CompressedMatrix, and serialize the compressed values to the bitstream.
		/// </summary>
		/// <param name="nonalloc">Populate this CompressedMatrix with the results of the cmopression operation.</param>
		/// <param name="transform">The transform to compress.</param>
		/// <param name="bitstream">The write target.</param>
		/// <param name="bcl"></param>
		public void Write(CompressedMatrix nonalloc, Transform transform, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			Compress(nonalloc, transform);

			if (cached_pBits > 0)
				posCrusher.Write(nonalloc.cPos, ref bitstream, bcl);
			if (cached_rBits > 0)
				rotCrusher.Write(nonalloc.cRot, ref bitstream, bcl);
			if (cached_sBits > 0)
				sclCrusher.Write(nonalloc.cScl, ref bitstream, bcl);
		}
		/// <summary>
		/// Compress a transform using this crusher, and serialize the compressed values to the bitstream.
		/// </summary>
		/// <param name="transform">The transform to compress.</param>
		/// <param name="bitstream">The write target.</param>
		/// <param name="bcl"></param>
		public void Write(Transform transform, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			reusableCM.crusher = this;
			Compress(reusableCM, transform);

			if (cached_pBits > 0)
				posCrusher.Write(reusableCM.cPos, ref bitstream, bcl);
			if (cached_rBits > 0)
				rotCrusher.Write(reusableCM.cRot, ref bitstream, bcl);
			if (cached_sBits > 0)
				sclCrusher.Write(reusableCM.cScl, ref bitstream, bcl);
		}

		#endregion

		#region Read and Decompress

		public void ReadAndDecompress(Matrix nonalloc, ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			ReadAndDecompress(nonalloc, buffer, ref bitposition, bcl);
		}
		[System.Obsolete("Use the nonalloc overload instead and supply a target Matrix. Matrix is now a class rather than a struct")]
		public Matrix ReadAndDecompress(ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			return ReadAndDecompress(buffer, ref bitposition, bcl);
		}

		public void ReadAndDecompress(Matrix nonalloc, ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, buffer, ref bitposition, bcl);
			Decompress(nonalloc, reusableCM);
		}
		[System.Obsolete("Use the nonalloc overload instead and supply a target Matrix. Matrix is now a class rather than a struct")]
		public Matrix ReadAndDecompress(ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, buffer, ref bitposition, bcl);
			return Decompress(reusableCM);
		}

		public void ReadAndDecompress(Matrix nonalloc, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, ref bitstream, bcl);
			Decompress(nonalloc, reusableCM);
		}
		[System.Obsolete("Use the nonalloc overload instead and supply a target Matrix. Matrix is now a class rather than a struct")]
		public Matrix ReadAndDecompress(ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, ref bitstream, bcl);
			return Decompress(reusableCM);
		}


		#endregion

		#region ReadAndApply

		/// <summary>
		/// Read the compressed value from a buffer, decompress it, and apply it to the target transform.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="bitstream"></param>
		/// <param name="bcl"></param>
		public void ReadAndApply(Transform target, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, ref bitstream, bcl);
			Apply(target, reusableCM);
		}

		/// <summary>
		/// Read the compressed value from a buffer, decompress it, and apply it to the target transform.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="bitstream"></param>
		/// <param name="bcl"></param>
		public void ReadAndApply(Transform target, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, buffer, ref bitposition, bcl);
			Apply(target, reusableCM);
		}

		#endregion

		#region Fragments Reader

		/// <summary>
		/// Reconstruct a CompressedMatrix from fragments.
		/// </summary>
		public void Read(CompressedMatrix nonalloc, ulong fragment0, ulong fragment1 = 0, ulong fragment2 = 0, ulong fragment3 = 0, uint fragment4 = 0, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Bitstream bitstream = new Bitstream(fragment0, fragment1, fragment2, fragment3, fragment4);
			Read(nonalloc, ref bitstream, bcl);
		}

		public void ReadAndDecompress(Matrix nonalloc, ulong fragment0, ulong fragment1 = 0, ulong fragment2 = 0, ulong fragment3 = 0, uint fragment4 = 0, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Bitstream bitstream = new Bitstream(fragment0, fragment1, fragment2, fragment3, fragment4);

			if (cached_pBits > 0)
				posCrusher.Read(reusableCM.cPos, ref bitstream, bcl);
			if (cached_rBits > 0)
				rotCrusher.Read(reusableCM.cRot, ref bitstream, bcl);
			if (cached_sBits > 0)
				sclCrusher.Read(reusableCM.cScl, ref bitstream, bcl);

			Decompress(nonalloc, reusableCM);

		}
		[System.Obsolete("Create a new Bitstream(frag, frag, frag) instead.")]
		public Bitstream Read(ulong fragment0, ulong fragment1 = 0, ulong fragment2 = 0, ulong fragment3 = 0, uint fragment4 = 0, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			return new Bitstream(fragment0, fragment1, fragment2, fragment3, fragment4);
		}

		/// <summary>
		/// Read compressed data from a Bitstream, and populates the suppled CompressedMatrix with the results.
		/// </summary>
		/// <param name="bitstream">Bitstream to read from.</param>
		/// <returns></returns>
		public void Read(CompressedMatrix nonalloc, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			nonalloc.crusher = this;
			if (cached_pBits > 0)
				posCrusher.Read(nonalloc.cPos, ref bitstream, bcl);
			if (cached_rBits > 0)
				rotCrusher.Read(nonalloc.cRot, ref bitstream, bcl);
			if (cached_sBits > 0)
				sclCrusher.Read(nonalloc.cScl, ref bitstream, bcl);
		}

		#endregion

		/// <summary>
		/// Deserialize the bitstream into the internal reusable CompressedMatrix, and return a bitstream representing that CM's serialized data.
		/// </summary>
		/// <param name="bitstream"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		[System.Obsolete("This probably had a use at some point... can't think of any now.")]
		public Bitstream Read(ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, ref bitstream, bcl);
			return reusableCM.ExtractBitstream();
		}

		#region ULong Buffer Readers

		/// <summary>
		/// Extract a CompressedMatrix from a primitive buffer. Results will overwrite the supplied CompressedMatrix non-alloc.
		/// </summary>
		/// <param name="nonalloc">Target of the Read.</param>
		/// <param name="buffer">Serialized source.</param>
		/// <param name="bcl"></param>
		public void Read(CompressedMatrix nonalloc, ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Read(nonalloc, buffer, ref bitposition, bcl);
		}
		public Bitstream Read(ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Read(reusableCM, buffer, ref bitposition, bcl);
			return reusableCM.ExtractBitstream();
		}

		/// <summary>
		/// Extract a CompressedMatrix from a primitive buffer. Results will overwrite the supplied CompressedMatrix non-alloc.
		/// </summary>
		/// <param name="nonalloc">Target of the Read.</param>
		/// <param name="buffer">Serialized source.</param>
		/// <param name="bitposition">The read start position of the buffer. This value will be incremented by the number of bits read.</param>
		/// <param name="bcl"></param>
		public void Read(CompressedMatrix nonalloc, ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			nonalloc.crusher = this;

			if (cached_pBits > 0)
				posCrusher.Read(nonalloc.cPos, buffer, ref bitposition, bcl);
			if (cached_rBits > 0)
				rotCrusher.Read(nonalloc.cRot, buffer, ref bitposition, bcl);
			if (cached_sBits > 0)
				sclCrusher.Read(nonalloc.cScl, buffer, ref bitposition, bcl);
		}
		public Bitstream Read(ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, buffer, ref bitposition, bcl);
			return reusableCM.ExtractBitstream();
		}

		#endregion

		#region Compress

		/// <summary>
		/// Compress the transform of the default gameobject. (Only avavilable if this crusher is serialized in the editor).
		/// </summary>
		/// <returns></returns>
		[System.Obsolete("Supply the transform to compress. Default Transform has be deprecated.")]
		public void Compress(CompressedMatrix nonalloc)
		{
			Debug.Assert(defaultTransform, transformMissingError);

			Compress(nonalloc, defaultTransform);
		}
		[System.Obsolete("Supply the transform to compress. Default Transform has be deprecated.")]
		public Bitstream Compress()
		{
			Debug.Assert(defaultTransform, transformMissingError);

			return Compress(defaultTransform);
		}

		public void Compress(CompressedMatrix nonalloc, Matrix matrix)
		{
			if (!cached)
				CacheValues();

			nonalloc.crusher = this;
			if (cached_pBits > 0) posCrusher.Compress(nonalloc.cPos, matrix.position); else nonalloc.cPos.Clear();
			if (cached_rBits > 0) rotCrusher.Compress(nonalloc.cRot, matrix.rotation); else nonalloc.cRot.Clear();
			if (cached_sBits > 0) sclCrusher.Compress(nonalloc.cScl, matrix.scale); else nonalloc.cScl.Clear();

		}
		public Bitstream Compress(Matrix matrix)
		{
			Compress(reusableCM, matrix);
			return reusableCM.ExtractBitstream();
		}

		public void Compress(CompressedMatrix nonalloc, Transform transform)
		{
			if (!cached)
				CacheValues();

			nonalloc.crusher = this;
			if (cached_pBits > 0) posCrusher.Compress(nonalloc.cPos, transform); else nonalloc.cPos.Clear();
			if (cached_rBits > 0) rotCrusher.Compress(nonalloc.cRot, transform); else nonalloc.cRot.Clear();
			if (cached_sBits > 0) sclCrusher.Compress(nonalloc.cScl, transform); else nonalloc.cScl.Clear();

		}
		//[System.Obsolete("Use the nonalloc overload instead and supply a target CompressedMatrix. CompressedMatrix is now a class rather than a struct")]
		public Bitstream Compress(Transform transform)
		{
			Compress(reusableCM, transform);
			return reusableCM.ExtractBitstream();
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		[System.Obsolete("Supply the transform to compress. Default Transform has be deprecated.")]
		public void CompressAndWrite(ref Bitstream bitstream)
		{
			if (!cached)
				CacheValues();

			Debug.Assert(defaultTransform, transformMissingError);

			if (cached_pBits > 0)
				posCrusher.CompressAndWrite(defaultTransform, ref bitstream);
			if (cached_rBits > 0)
				rotCrusher.CompressAndWrite(defaultTransform, ref bitstream);
			if (cached_sBits > 0)
				sclCrusher.CompressAndWrite(defaultTransform, ref bitstream);
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="bitstream"></param>
		public void CompressAndWrite(Matrix matrix, ref Bitstream bitstream)
		{
			if (!cached)
				CacheValues();

			if (cached_pBits > 0)
				posCrusher.CompressAndWrite(matrix.position, ref bitstream);
			if (cached_rBits > 0)
				rotCrusher.CompressAndWrite(matrix.rotation, ref bitstream);
			if (cached_sBits > 0)
				sclCrusher.CompressAndWrite(matrix.scale, ref bitstream);
		}
		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="bitstream"></param>
		public void CompressAndWrite(Matrix matrix, byte[] buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			Debug.Log(cached_pBits);

			if (cached_pBits > 0)
				posCrusher.CompressAndWrite(matrix.position, buffer, ref bitposition);
			if (cached_rBits > 0)
				rotCrusher.CompressAndWrite(matrix.rotation, buffer, ref bitposition);
			if (cached_sBits > 0)
				sclCrusher.CompressAndWrite(matrix.scale, buffer, ref bitposition);
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Transform transform, ref Bitstream bitstream)
		{
			if (!cached)
				CacheValues();

			if (cached_pBits > 0)
				posCrusher.CompressAndWrite(transform, ref bitstream);
			if (cached_rBits > 0)
				rotCrusher.CompressAndWrite(transform, ref bitstream);
			if (cached_sBits > 0)
				sclCrusher.CompressAndWrite(transform, ref bitstream);
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Transform transform, byte[] buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			if (cached_pBits > 0) { }
			posCrusher.CompressAndWrite(transform, buffer, ref bitposition);
			if (cached_rBits > 0) { }
			rotCrusher.CompressAndWrite(transform, buffer, ref bitposition);
			if (cached_sBits > 0)
				sclCrusher.CompressAndWrite(transform, buffer, ref bitposition);
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Rigidbody rb, byte[] buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			if (cached_pBits > 0) { }
			posCrusher.CompressAndWrite(rb.position, buffer, ref bitposition);
			if (cached_rBits > 0) { }
			rotCrusher.CompressAndWrite(rb.rotation, buffer, ref bitposition);
			if (cached_sBits > 0)
				sclCrusher.CompressAndWrite(rb.transform, buffer, ref bitposition);
		}

		#endregion

		#region Decompress

		public void Decompress(Matrix nonalloc, ulong compressed, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, compressed, bcl);
			Decompress(nonalloc, reusableCM);
		}
		[System.Obsolete("Use the nonalloc overload instead and supply a target Matrix. Matrix is now a class rather than a struct")]
		public Matrix Decompress(ulong compressed, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCM, compressed, bcl);
			return Decompress(reusableCM);
		}

		public void Decompress(Matrix nonalloc, CompressedMatrix compMatrix)
		{
			if (!cached)
				CacheValues();

			nonalloc.Set(
				this,
				(cached_pBits > 0) ? posCrusher.Decompress(compMatrix.cPos) : new Element(),
				(cached_rBits > 0) ? rotCrusher.Decompress(compMatrix.cRot) : new Element(),
				(cached_sBits > 0) ? sclCrusher.Decompress(compMatrix.cScl) : new Element()
				);
		}
		[System.Obsolete("Use the nonalloc overload instead and supply a target Matrix. Matrix is now a class rather than a struct")]
		public Matrix Decompress(CompressedMatrix compMatrix)
		{
			if (!cached)
				CacheValues();

			return new Matrix(
				this,
				(cached_pBits > 0) ? posCrusher.Decompress(compMatrix.cPos) : new Element(),
				(cached_rBits > 0) ? rotCrusher.Decompress(compMatrix.cRot) : new Element(),
				(cached_sBits > 0) ? sclCrusher.Decompress(compMatrix.cScl) : new Element()
				);
		}

		#endregion

		#region Apply

		const string transformMissingError = "The 'defaultTransform' is null and has not be set in the inspector. " +
				"For non-editor usages of TransformCrusher you need to pass the target transform to this method.";

		[System.Obsolete("Supply the transform to compress. Default Transform has be deprecated.")]
		public void Apply(ulong cvalue)
		{
			Debug.Assert(defaultTransform, transformMissingError);

			Apply(defaultTransform, cvalue);
		}

		public void Apply(Transform t, ulong cvalue)
		{
			Decompress(reusableM, cvalue);
			Apply(t, reusableM);
		}

		[System.Obsolete("Supply the transform to compress. Default Transform has be deprecated.")]
		public void Apply(ulong u0, ulong u1, ulong u2, ulong u3, uint u4)
		{
			Debug.Assert(defaultTransform, transformMissingError);

			Apply(defaultTransform, u0, u1, u2, u3, u4);
		}

		public void Apply(Transform t, ulong u0, ulong u1, ulong u2, ulong u3, uint u4)
		{
			Read(reusableCM, u0, u1, u2, u3, u4);
			Apply(t, reusableCM);
		}

		/// <summary>
		/// Apply the CompressedMatrix to a transform. Any axes not included in the Crusher are left as is.
		/// </summary>
		[System.Obsolete("Supply the transform to Apply to. Default Transform has be deprecated.")]
		public void Apply(CompressedMatrix cmatrix)
		{
			Debug.Assert(defaultTransform, transformMissingError);

			Apply(defaultTransform, cmatrix);
		}

		/// <summary>
		/// Apply the CompressedMatrix to a transform. Any axes not included in the Crusher are left as is.
		/// </summary>
		public void Apply(Transform t, CompressedMatrix cmatrix)
		{
			if (cached_pBits > 0)
				posCrusher.Apply(t, cmatrix.cPos);
			if (cached_rBits > 0)
				rotCrusher.Apply(t, cmatrix.cRot);
			if (cached_sBits > 0)
				sclCrusher.Apply(t, cmatrix.cScl);
		}


		/// <summary>
		/// Apply the TRS matrix to a transform. Any axes not included in the Crusher are left as is.
		/// </summary>
		[System.Obsolete("Supply the transform to Apply to. Default Transform has be deprecated.")]
		public void Apply(Matrix matrix)
		{
			Debug.Assert(defaultTransform, transformMissingError);

			Apply(defaultTransform, matrix);
		}

		/// <summary>
		/// Apply the TRS matrix to a transform. Any axes not included in the Crusher are left as is.
		/// </summary>
		public void Apply(Transform transform, Matrix matrix)
		{
			if (cached_pBits > 0)
				posCrusher.Apply(transform, matrix.position);

			if (cached_rBits > 0)
			{
				//if (matrix.rotationType == RotationType.Quaternion)
				rotCrusher.Apply(transform, matrix.rotation);
				//else
				//	rotCrusher.Apply(transform, matrix.eulers);
			}

			if (cached_sBits > 0)
				sclCrusher.Apply(transform, matrix.scale);
		}

		/// <summary>
		/// Apply the TRS matrix to a transform. Any axes not included in the Crusher are left as is.
		/// </summary>
		public void Apply(Rigidbody rb, Matrix matrix)
		{
			if (cached_pBits > 0)
				posCrusher.Apply(rb, matrix.position);

			if (cached_rBits > 0)
			{
				//if (matrix.rotationType == RotationType.Quaternion)
				rotCrusher.Apply(rb, matrix.rotation);
				//else
				//	rotCrusher.Apply(transform, matrix.eulers);
			}

			if (cached_sBits > 0)
				sclCrusher.Apply(rb.transform, matrix.scale);
		}

		#endregion

		/// <summary>
		/// Get the total number of bits this Transform is set to write.
		/// </summary>
		public int TallyBits(BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			return posCrusher.TallyBits(bcl) + rotCrusher.TallyBits(bcl) + sclCrusher.TallyBits(bcl);
		}

		public void CopyFrom(TransformCrusher source)
		{
			posCrusher.CopyFrom(source.posCrusher);
			rotCrusher.CopyFrom(source.rotCrusher);
			sclCrusher.CopyFrom(source.sclCrusher);

			CacheValues();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TransformCrusher);
		}

		public bool Equals(TransformCrusher other)
		{
			return other != null &&
				//EqualityComparer<Transform>.Default.Equals(defaultTransform, other.defaultTransform) &&
				(posCrusher == null ? other.posCrusher == null : posCrusher.Equals(other.posCrusher)) &&
				(rotCrusher == null ? other.rotCrusher == null : rotCrusher.Equals(other.rotCrusher)) &&
				(sclCrusher == null ? other.sclCrusher == null : sclCrusher.Equals(other.sclCrusher));
		}

		public override int GetHashCode()
		{
			var hashCode = -453726296;
			//hashCode = hashCode * -1521134295 + EqualityComparer<Transform>.Default.GetHashCode(defaultTransform);
			hashCode = hashCode * -1521134295 + ((posCrusher == null) ? 0 : posCrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((rotCrusher == null) ? 0 : rotCrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((sclCrusher == null) ? 0 : sclCrusher.GetHashCode());
			return hashCode;
		}


		public static bool operator ==(TransformCrusher crusher1, TransformCrusher crusher2)
		{
			return EqualityComparer<TransformCrusher>.Default.Equals(crusher1, crusher2);
		}

		public static bool operator !=(TransformCrusher crusher1, TransformCrusher crusher2)
		{
			return !(crusher1 == crusher2);
		}
	}

}


