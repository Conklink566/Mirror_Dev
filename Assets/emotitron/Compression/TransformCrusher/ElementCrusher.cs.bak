//Copyright 2018, Davin Carten, All rights reserved

using UnityEngine;
using emotitron.Debugging;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Compression
{
	public interface IOnElementCrusherChange
	{
		void OnCrusherChange(ElementCrusher ec);
	}


	[System.Serializable]
	public class ElementCrusher : Crusher<ElementCrusher>, IEquatable<ElementCrusher>, ICrusherCopy<ElementCrusher>
	{
		public enum UniformAxes { NonUniform = 0, XY = 3, XZ = 5, YZ = 6, XYZ = 7 }
		public enum StaticTRSType
		{
			Position = 0,
			Euler = 1,
			Quaternion = 2,
			Scale = 3,
			Generic = 4
		}
		/// <summary>
		/// Experiemental collection of non-changing crushers, to avoid redundant crushers.
		/// </summary>
		public static Dictionary<int, ElementCrusher> staticElementCrushers = new Dictionary<int, ElementCrusher>();

		#region Static Crushers

		public static ElementCrusher GetStaticPositionCrusher(Bounds bounds, int resolution)
		{
			// Build a new EC with the supplied values.
			ElementCrusher ec = new ElementCrusher(StaticTRSType.Position)
			{
				xcrusher = FloatCrusher.GetStaticFloatCrusher(resolution, bounds.min.x, bounds.max.x, Axis.Generic, TRSType.Position),
				ycrusher = FloatCrusher.GetStaticFloatCrusher(resolution, bounds.min.y, bounds.max.y, Axis.Generic, TRSType.Position),
				zcrusher = FloatCrusher.GetStaticFloatCrusher(resolution, bounds.min.z, bounds.max.z, Axis.Generic, TRSType.Position)
			};
			// See if this crusher is a repeat of an existing one
			return CheckAgainstStatics(ec);
		}

		public static ElementCrusher GetStaticQuatCrusher(int minBits)
		{
			// Build a new EC with the supplied values.
			ElementCrusher ec = new ElementCrusher(StaticTRSType.Quaternion)
			{
				qcrusher = new QuatCrusher(false, false) { Bits = minBits }
			};
			// See if this crusher is a repeat of an existing one
			return CheckAgainstStatics(ec);
		}

		/// <summary>
		/// Checks to see if a crusher with the same settings has been registered with our dictionary. If a crusher with the same value exists there, 
		/// that crusher is returned. If no duplicate of of the supplied crusher exists, the supplied crusher is returned.
		/// </summary>
		/// <param name="ec"></param>
		/// <returns></returns>
		public static ElementCrusher CheckAgainstStatics(ElementCrusher ec, bool CheckAgainstFloatCrushersAsWell = true)
		{
			if (ReferenceEquals(ec, null))
				return null;

			if (CheckAgainstFloatCrushersAsWell)
			{
				if (ec.cache_xEnabled)
					ec.xcrusher = FloatCrusher.CheckAgainstStatics(ec.xcrusher);
				if (ec.cache_yEnabled)
					ec.ycrusher = FloatCrusher.CheckAgainstStatics(ec.ycrusher);
				if (ec.cache_zEnabled)
					ec.zcrusher = FloatCrusher.CheckAgainstStatics(ec.zcrusher);
				if (ec.cache_uEnabled)
					ec.ucrusher = FloatCrusher.CheckAgainstStatics(ec.ucrusher);
			}

			int hash = ec.GetHashCode();

			if (staticElementCrushers.ContainsKey(hash))
			{
				return staticElementCrushers[hash];
			}

			staticElementCrushers.Add(hash, ec);
			return ec;
		}

		public static ElementCrusher defaultUncompressedElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		{
			xcrusher = FloatCrusher.defaultUncompressedCrusher,
			ycrusher = FloatCrusher.defaultUncompressedCrusher,
			zcrusher = FloatCrusher.defaultUncompressedCrusher,
			ucrusher = FloatCrusher.defaultUncompressedCrusher,
		});

		public static ElementCrusher defaultUncompressedPosCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Position)
		{
			xcrusher = FloatCrusher.defaultUncompressedCrusher,
			ycrusher = FloatCrusher.defaultUncompressedCrusher,
			zcrusher = FloatCrusher.defaultUncompressedCrusher,
			ucrusher = FloatCrusher.defaultUncompressedCrusher,
		});

		public static ElementCrusher defaultUncompressedSclCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Position)
		{
			xcrusher = FloatCrusher.defaultUncompressedCrusher,
			ycrusher = FloatCrusher.defaultUncompressedCrusher,
			zcrusher = FloatCrusher.defaultUncompressedCrusher,
			ucrusher = FloatCrusher.defaultUncompressedCrusher,
		});

		public static ElementCrusher defaultHalfFloatElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		{
			xcrusher = FloatCrusher.defaulHalfFloatCrusher,
			ycrusher = FloatCrusher.defaulHalfFloatCrusher,
			zcrusher = FloatCrusher.defaulHalfFloatCrusher,
			ucrusher = FloatCrusher.defaulHalfFloatCrusher,
		});

		public static ElementCrusher defaultHalfFloatPosCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Position)
		{
			xcrusher = FloatCrusher.defaulHalfFloatCrusher,
			ycrusher = FloatCrusher.defaulHalfFloatCrusher,
			zcrusher = FloatCrusher.defaulHalfFloatCrusher,
			ucrusher = FloatCrusher.defaulHalfFloatCrusher,
		});

		public static ElementCrusher defaultHalfFloatSclCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Scale)
		{
			xcrusher = FloatCrusher.defaulHalfFloatCrusher,
			ycrusher = FloatCrusher.defaulHalfFloatCrusher,
			zcrusher = FloatCrusher.defaulHalfFloatCrusher,
			ucrusher = FloatCrusher.defaulHalfFloatCrusher,
		});

		///// <summary>
		///// Static Constructor
		///// </summary>
		//static ElementCrusher()
		//{
		//	ElementCrusher defaultUncompressedElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		//	{
		//		xcrusher = FloatCrusher.defaultUncompressedCrusher,
		//		ycrusher = FloatCrusher.defaultUncompressedCrusher,
		//		zcrusher = FloatCrusher.defaultUncompressedCrusher,
		//		ucrusher = FloatCrusher.defaultUncompressedCrusher,
		//	});
		//	ElementCrusher defaultHalfFloatElementCrusher = CheckAgainstStatics(new ElementCrusher(StaticTRSType.Generic)
		//	{
		//		xcrusher = FloatCrusher.defaulHalfFloatCrusher,
		//		ycrusher = FloatCrusher.defaulHalfFloatCrusher,
		//		zcrusher = FloatCrusher.defaulHalfFloatCrusher,
		//		ucrusher = FloatCrusher.defaulHalfFloatCrusher,
		//	});

		//	Debug.Log("STATIC CONS EC " + (ElementCrusher.defaultUncompressedElementCrusher != null) + " " 
		//		+ (ElementCrusher.defaultUncompressedElementCrusher.xcrusher != null));

		//}

		#endregion

#if UNITY_EDITOR
		public bool isExpanded = true;
#endif
		public bool hideFieldName = false;

		[SerializeField] private TRSType _trsType;
		public TRSType TRSType
		{
			get { return _trsType; }
			set
			{
				_trsType = value;
				xcrusher.TRSType = value;
				ycrusher.TRSType = value;
				zcrusher.TRSType = value;
			}
		}

		[SerializeField] public Transform defaultTransform;
		[SerializeField] public UniformAxes uniformAxes;
		[SerializeField] public FloatCrusher xcrusher;
		[SerializeField] public FloatCrusher ycrusher;
		[SerializeField] public FloatCrusher zcrusher;
		[SerializeField] public FloatCrusher ucrusher;
		[SerializeField] public QuatCrusher qcrusher;
		[SerializeField] public bool local;
		[SerializeField] public bool useWorldBounds;

		[SerializeField] public bool enableTRSTypeSelector;
		[SerializeField] public bool enableLocalSelector = true;

		/// Temporary CompressedMatrix used internally when a non-alloc is not provided and no return CE required.
		private static CompressedElement reusableCE = new CompressedElement();

		#region Cached values

		// cache values
		[System.NonSerialized]
		private bool cached;

		[System.NonSerialized] private bool cache_xEnabled, cache_yEnabled, cache_zEnabled, cache_uEnabled, cache_qEnabled;
		[System.NonSerialized] private bool cache_isUniformScale;
		[System.NonSerialized] private int[] cache_xBits, cache_yBits, cache_zBits, cache_uBits, cache_TotalBits;
		[System.NonSerialized] private int cache_qBits;
		[System.NonSerialized] private bool cache_mustCorrectRotationX;

		public Bounds bounds = new Bounds();

		/// <summary>
		/// Get will return a Bounds struct with the ranges of the x/y/z crushers. Set will set the x/y/z crusher to match those of the Bounds value.
		/// </summary>
		public Bounds Bounds
		{
			get
			{
				bounds.SetMinMax(
					new Vector3(
						(xcrusher != null) ? xcrusher.Min : 0,
						(ycrusher != null) ? ycrusher.Min : 0,
						(zcrusher != null) ? zcrusher.Min : 0),
					new Vector3(
						(xcrusher != null) ? xcrusher.Max : 0,
						(ycrusher != null) ? ycrusher.Max : 0,
						(zcrusher != null) ? zcrusher.Max : 0)
					);

				return bounds;
			}

			set
			{
				if (xcrusher != null)
					xcrusher.SetRange(value.min.x, value.max.x);
				if (ycrusher != null)
					ycrusher.SetRange(value.min.y, value.max.y);
				if (zcrusher != null)
					zcrusher.SetRange(value.min.z, value.max.z);

				CacheValues();
			}
		}

		#region  OnChange callbacks

		public List<IOnElementCrusherChange> onChangeCallbacks = new List<IOnElementCrusherChange>();


		private static List<IOnElementCrusherChange> callbackCleanupList;
		public void BroadCastOnChange()
		{
			int cnt = onChangeCallbacks.Count;
			//for (int i = (cnt -1); i >= 0; --i)
			for (int i = 0; i < cnt; ++i)
			{
				var cb = onChangeCallbacks[i];
				if (cb != null)
					onChangeCallbacks[i].OnCrusherChange(this);
				else
					onChangeCallbacks.RemoveAt(i);
			}
		}

		public void CacheValues()
		{
			if (Application.isPlaying && !Application.isEditor)
				NullUnusedCrushers();

			if (_trsType == TRSType.Quaternion)
			{
				cache_qEnabled = (qcrusher != null) && qcrusher.enabled && qcrusher.Bits > 0;
				cache_qBits = (qcrusher != null) ? qcrusher.Bits : 0;

			}
			else if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				cache_isUniformScale = true;
				cache_uEnabled = (ucrusher != null) && ucrusher.Enabled;
				cache_uBits = new int[4];

				for (int i = 0; i < 4; ++i)
					cache_uBits[i] = (ucrusher != null) ? ucrusher.GetBitsAtCullLevel((BitCullingLevel)i) : 0;

			}
			else
			{
				cache_mustCorrectRotationX = _trsType == TRSType.Euler && xcrusher.UseHalfRangeX;

				cache_xEnabled = (xcrusher != null) && xcrusher.Enabled;
				cache_yEnabled = (ycrusher != null) && ycrusher.Enabled;
				cache_zEnabled = (zcrusher != null) && zcrusher.Enabled;
				cache_xBits = new int[4];
				cache_yBits = new int[4];
				cache_zBits = new int[4];

				for (int i = 0; i < 4; ++i)
				{
					cache_xBits[i] = (xcrusher != null) ? xcrusher.GetBitsAtCullLevel((BitCullingLevel)i) : 0;
					cache_yBits[i] = (ycrusher != null) ? ycrusher.GetBitsAtCullLevel((BitCullingLevel)i) : 0;
					cache_zBits[i] = (zcrusher != null) ? zcrusher.GetBitsAtCullLevel((BitCullingLevel)i) : 0;

				}
			}

			cache_TotalBits = new int[4];

			for (int i = 0; i < 4; ++i)
				cache_TotalBits[i] = TallyBits((BitCullingLevel)i);

			cached = true;

			/// Trigger OnChange callback
			BroadCastOnChange();
		}

		#endregion

		/// <summary>
		/// Called at startup of builds to clear any crushers that were stored for the editor, but are not used in builds. Generates some startup GC, but the alternative is breaking
		/// unused crushers in SOs in the editor every time a build is made.
		/// </summary>
		private void NullUnusedCrushers()
		{
			if (_trsType == TRSType.Quaternion)
			{
				xcrusher = null;
				ycrusher = null;
				zcrusher = null;
				ucrusher = null;
			}
			else if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				xcrusher = null;
				ycrusher = null;
				zcrusher = null;
				qcrusher = null;
			}
			else
			{
				qcrusher = null;
				ucrusher = null;
			}
		}

		/// <summary>
		/// Property that returns if this element crusher is effectively enabled (has any enabled float/quat crushers using bits > 0)
		/// </summary>
		public bool Enabled
		{
			get
			{
				if (TRSType == TRSType.Quaternion)
					return (qcrusher.enabled && qcrusher.Bits > 0);

				else if (TRSType == TRSType.Scale && uniformAxes != 0)
					return ucrusher.Enabled;

				return xcrusher.Enabled | ycrusher.Enabled | zcrusher.Enabled;
			}
		}



		#endregion

		/// <summary>
		/// Indexer returns the component crushers.
		/// </summary>
		/// <param name="axis"></param>
		/// <returns></returns>
		public FloatCrusher this[int axis]
		{
			get
			{
				switch (axis)
				{
					case 0:
						return xcrusher;
					case 1:
						return ycrusher;
					case 2:
						return zcrusher;

					default:
						Debug.Log("AXIS " + axis + " should not be calling happening");
						return null;
				}
			}
		}

		#region Constructors

		public ElementCrusher()
		{
			Defaults(TRSType.Generic);
		}

		/// <summary>
		/// Static crushers don't initialize unused crusher. Using this constructor will leave unused crushers null, since these types are not meant
		/// to be shown in the inspector.
		/// </summary>
		/// <param name="staticTrsType"></param>
		public ElementCrusher(StaticTRSType staticTrsType)
		{
			_trsType = (TRSType)staticTrsType;
		}

		// Constructor
		public ElementCrusher(bool enableTRSTypeSelector = true)
		{
			this._trsType = TRSType.Generic;
			Defaults(TRSType.Generic);

			this.enableTRSTypeSelector = enableTRSTypeSelector;
		}

		// Constructor
		public ElementCrusher(TRSType trsType, bool enableTRSTypeSelector = true)
		{
			this._trsType = trsType;
			Defaults(trsType);

			this.enableTRSTypeSelector = enableTRSTypeSelector;
		}

		private void Defaults(TRSType trs)
		{
			if (trs == TRSType.Quaternion || trs == TRSType.Euler)
			{
				xcrusher = new FloatCrusher(BitPresets.Bits10, -90f, 90f, Axis.X, TRSType.Euler, true);
				ycrusher = new FloatCrusher(BitPresets.Bits12, -180f, 180f, Axis.Y, TRSType.Euler, true);
				zcrusher = new FloatCrusher(BitPresets.Bits10, -180f, 180f, Axis.Z, TRSType.Euler, true);
				//ucrusher = new FloatCrusher(Axis.Uniform, TRSType.Scale, true);
				qcrusher = new QuatCrusher(true, false);
			}
			else if (trs == TRSType.Scale)
			{
				xcrusher = new FloatCrusher(BitPresets.Bits12, 0f, 2f, Axis.X, TRSType.Scale, true);
				ycrusher = new FloatCrusher(BitPresets.Bits10, 0f, 2f, Axis.Y, TRSType.Scale, true);
				zcrusher = new FloatCrusher(BitPresets.Bits10, 0f, 2f, Axis.Z, TRSType.Scale, true);
				ucrusher = new FloatCrusher(BitPresets.Bits10, 0f, 2f, Axis.Uniform, TRSType.Scale, true);
			}
			else
			{
				xcrusher = new FloatCrusher(BitPresets.Bits12, -20f, 20f, Axis.X, trs, true);
				ycrusher = new FloatCrusher(BitPresets.Bits10, -5f, 5f, Axis.Y, trs, true);
				zcrusher = new FloatCrusher(BitPresets.Bits10, -5f, 5f, Axis.Z, trs, true);
			}
		}

		#endregion

		/// <summary>
		/// </summary>
		public void Write(CompressedElement ce, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (TRSType == TRSType.Quaternion)
			{
				qcrusher.Write(ce.cQuat, ref bitstream);
			}

			else if (TRSType == TRSType.Scale && cache_uEnabled)
			{
				ucrusher.Write(ce.cUniform, ref bitstream, bcl);
			}

			else
			{
				if (cache_xEnabled)
					xcrusher.Write(ce.cx, ref bitstream, bcl);
				if (cache_yEnabled)
					ycrusher.Write(ce.cy, ref bitstream, bcl);
				if (cache_zEnabled)
					zcrusher.Write(ce.cz, ref bitstream, bcl);
			}
		}


		#region Array Writers
		/// <summary>
		/// Automatically use the correct transform TRS element based on the TRSType and local settings of each Crusher.
		/// </summary>
		public void Write(CompressedElement nonalloc, Transform trans, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:

					Write(nonalloc, (local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);
					return;

				case TRSType.Euler:
					Write(nonalloc, (local) ? trans.localEulerAngles : trans.eulerAngles, bytes, ref bitposition, bcl);
					return;

				case TRSType.Quaternion:
					Write(nonalloc, (local) ? trans.localRotation : trans.rotation, bytes, ref bitposition, bcl);
					return;

				case TRSType.Scale:
					Write(nonalloc, trans.localScale, bytes, ref bitposition, bcl);
					return;

				default:
					XDebug.Log("You are sending a transform to be crushed, but the Element Type is Generic - did you want Position?");
					Write(nonalloc, (local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);
					return;
			}
		}
		[System.Obsolete()]
		public CompressedElement Write(Transform trans, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					return Write((local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);

				case TRSType.Euler:
					return Write((local) ? trans.localEulerAngles : trans.eulerAngles, bytes, ref bitposition, bcl);

				case TRSType.Quaternion:
					return Write((local) ? trans.localRotation : trans.rotation, bytes, ref bitposition, bcl);

				case TRSType.Scale:
					return Write(trans.localScale, bytes, ref bitposition, bcl);

				default:
					XDebug.Log("You are sending a transform to be crushed, but the Element Type is Generic - did you want Position?");
					return Write((local) ? trans.localPosition : trans.position, bytes, ref bitposition, bcl);
			}
		}

		/// <summary>
		/// Serialize a CompressedElement into a byte[] buffer.
		/// </summary>
		/// <param name="ce"></param>
		/// <param name="bytes"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		public void Write(CompressedElement ce, byte[] bytes, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				qcrusher.Write(ce.cQuat, bytes, ref bitposition);
			}
			else if (cache_isUniformScale)
			{
				ucrusher.Write(ce.cUniform, bytes, ref bitposition, bcl);
			}
			else
			{
				if (cache_xEnabled && ((int)ia & 1) != 0) xcrusher.Write(ce.cx, bytes, ref bitposition, bcl);
				if (cache_yEnabled && ((int)ia & 2) != 0) ycrusher.Write(ce.cy, bytes, ref bitposition, bcl);
				if (cache_zEnabled && ((int)ia & 4) != 0) zcrusher.Write(ce.cz, bytes, ref bitposition, bcl);
			}
		}

		public void Write(CompressedElement nonalloc, Vector3 v3, byte[] bytes, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Write(nonalloc, v3, bytes, ref bitposition);
		}
		[System.Obsolete()]
		public CompressedElement Write(Vector3 v3, byte[] bytes, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			return Write(v3, bytes, ref bitposition);
		}

		/// <summary>
		/// Compress and then write a vector3 value into a byte[] buffer.
		/// </summary>
		/// <param name="nonalloc">Overwrite this CompressedElement with the compressed value.</param>
		/// <param name="v3">The uncompressed value to compress and serialize.</param>
		/// <param name="bytes">The target buffer.</param>
		/// <param name="bitposition">Write position of the target buffer.</param>
		/// <param name="bcl"></param>
		public void Write(CompressedElement nonalloc, Vector3 v3, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_isUniformScale)
			{
				CompressedFloat c = ucrusher.Write(uniformAxes == UniformAxes.YZ ? v3.y : v3.x, bytes, ref bitposition, bcl);
				nonalloc.Set(this, (uint)c.cvalue);
			}

			else if (TRSType == TRSType.Quaternion)
			{
				ulong c = qcrusher.Write(Quaternion.Euler(v3), bytes, ref bitposition);
				nonalloc.Set(this, c);
			}
			else
			{
				if (cache_mustCorrectRotationX)
					v3 = FloatCrusherUtilities.GetXCorrectedEuler(v3);

				nonalloc.Set(
					this,
					cache_xEnabled ? xcrusher.Write(v3.x, bytes, ref bitposition, bcl) : new CompressedFloat(),
					cache_yEnabled ? ycrusher.Write(v3.y, bytes, ref bitposition, bcl) : new CompressedFloat(),
					cache_zEnabled ? zcrusher.Write(v3.z, bytes, ref bitposition, bcl) : new CompressedFloat());
			}
		}
		[System.Obsolete()]
		public CompressedElement Write(Vector3 v3, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_isUniformScale)
			{
				CompressedValue c = ucrusher.Write(uniformAxes == UniformAxes.YZ ? v3.y : v3.x, bytes, ref bitposition, bcl);
				return new CompressedElement(this, (uint)c.cvalue);
			}

			else if (TRSType == TRSType.Quaternion)
			{
				ulong c = qcrusher.Write(Quaternion.Euler(v3), bytes, ref bitposition);
				return new CompressedElement(this, c);
			}

			else if (cache_mustCorrectRotationX)
				v3 = FloatCrusherUtilities.GetXCorrectedEuler(v3);

			return new CompressedElement(
				this,
				cache_xEnabled ? (uint)xcrusher.Write(v3.x, bytes, ref bitposition, bcl) : 0,
				cache_yEnabled ? (uint)ycrusher.Write(v3.y, bytes, ref bitposition, bcl) : 0,
				cache_zEnabled ? (uint)zcrusher.Write(v3.z, bytes, ref bitposition, bcl) : 0);
		}

		public void Write(CompressedElement nonalloc, Quaternion quat, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			XDebug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
				System.Enum.GetName(typeof(TRSType), TRSType) + ".", TRSType != TRSType.Quaternion, true);

			nonalloc.Set(this, qcrusher.Write(quat, bytes, ref bitposition));
		}
		[System.Obsolete()]
		public CompressedElement Write(Quaternion quat, byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			XDebug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
				System.Enum.GetName(typeof(TRSType), TRSType) + ".", TRSType != TRSType.Quaternion, true);

			return new CompressedElement(this, qcrusher.Write(quat, bytes, ref bitposition));
		}

		#endregion

		#region Array Readers
		public void Read(CompressedElement nonalloc, byte[] bytes, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Read(nonalloc, bytes, ref bitposition, ia, bcl);
		}
		[System.Obsolete]
		public CompressedElement Read(byte[] bytes, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			return Read(bytes, ref bitposition, bcl);
		}



		/// <summary>
		/// Reads out the commpressed value for this vector/quaternion from a buffer. Needs to be decompressed still to become vector3/quaterion.
		/// </summary>
		public void Read(CompressedElement nonalloc, byte[] bytes, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				nonalloc.Set(this, (ulong)bytes.Read(ref bitposition, cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				nonalloc.Set(this, (uint)bytes.Read(ref bitposition, cache_uBits[(int)bcl]));
			}
			else
			{
				CompressedFloat cx, cy, cz;
				if ((ia & IncludedAxes.X) != 0)
				{
					int xbits = cache_xBits[(int)bcl];
					cx = cache_xEnabled ? new CompressedFloat(xcrusher, (uint)bytes.Read(ref bitposition, xbits)) : new CompressedFloat();
				}
				else cx = new CompressedFloat();

				if ((ia & IncludedAxes.Y) != 0)
				{
					int ybits = cache_yBits[(int)bcl];
					cy = cache_yEnabled ? new CompressedFloat(ycrusher, (uint)bytes.Read(ref bitposition, ybits)) : new CompressedFloat();

				}
				else cy = new CompressedFloat();

				if ((ia & IncludedAxes.Z) != 0)
				{
					int zbits = cache_zBits[(int)bcl];
					cz = cache_zEnabled ? new CompressedFloat(zcrusher, (uint)bytes.Read(ref bitposition, zbits)) : new CompressedFloat();
				}
				else cz = new CompressedFloat();

				nonalloc.Set(this, cx, cy, cz);
			}

		}
		[System.Obsolete()]
		public CompressedElement Read(byte[] bytes, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				return new CompressedElement(this, (ulong)bytes.Read(ref bitposition, cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				return new CompressedElement(this, (uint)bytes.Read(ref bitposition, cache_uBits[(int)bcl]));
			}

			int xbits = cache_xBits[(int)bcl];
			int ybits = cache_yBits[(int)bcl];
			int zbits = cache_zBits[(int)bcl];

			var cx = cache_xEnabled ? new CompressedFloat(xcrusher, (uint)bytes.Read(ref bitposition, xbits)) : new CompressedFloat();
			var cy = cache_yEnabled ? new CompressedFloat(ycrusher, (uint)bytes.Read(ref bitposition, ybits)) : new CompressedFloat();
			var cz = cache_zEnabled ? new CompressedFloat(zcrusher, (uint)bytes.Read(ref bitposition, zbits)) : new CompressedFloat();

			return new CompressedElement(this, cx, cy, cz);
		}

		public Element ReadAndDecompress(byte[] bytes, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			Read(reusableCE, bytes, ref bitposition, ia, bcl);
			return Decompress(reusableCE);
		}

		#endregion

		#region ULong Buffer Writers

		/// <summary>
		/// Automatically use the correct transform element based on the TRSType for this Crusher.
		/// </summary>
		public void Write(CompressedElement nonalloc, Transform trans, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					Write(nonalloc, (local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Euler:
					Write(nonalloc, (local) ? trans.localEulerAngles : trans.eulerAngles, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Quaternion:
					Write(nonalloc, (local) ? trans.localRotation : trans.rotation, ref buffer, ref bitposition);
					return;

				case TRSType.Scale:
					Write(nonalloc, trans.localScale, ref buffer, ref bitposition, bcl);
					return;

				default:
					XDebug.Log("You are sending a transform to be crushed, but the Element Type is Generic - did you want Position?");
					Write(nonalloc, (local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;
			}
		}
		public void Write(Transform trans, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					Write((local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Euler:
					Write((local) ? trans.localEulerAngles : trans.eulerAngles, ref buffer, ref bitposition, bcl);
					return;

				case TRSType.Quaternion:
					Write((local) ? trans.localRotation : trans.rotation, ref buffer, ref bitposition);
					return;

				case TRSType.Scale:
					Write(trans.localScale, ref buffer, ref bitposition, bcl);
					return;

				default:
					XDebug.Log("You are sending a transform to be crushed, but the Element Type is Generic - did you want Position?");
					Write((local) ? trans.localPosition : trans.position, ref buffer, ref bitposition, bcl);
					return;
			}
		}

		public void Write(CompressedElement nonalloc, Vector3 v3, ref ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Write(nonalloc, v3, ref buffer, ref bitposition, bcl);
		}
		public void Write(Vector3 v3, ref ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Write(v3, ref buffer, ref bitposition, bcl);
		}

		public void Write(CompressedElement nonalloc, Vector3 v3, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			CompressedFloat cx = cache_xEnabled ? xcrusher.Write(v3.x, ref buffer, ref bitposition, bcl) : new CompressedFloat();
			CompressedFloat cy = cache_yEnabled ? ycrusher.Write(v3.y, ref buffer, ref bitposition, bcl) : new CompressedFloat();
			CompressedFloat cz = cache_zEnabled ? zcrusher.Write(v3.z, ref buffer, ref bitposition, bcl) : new CompressedFloat();

			nonalloc.Set(this, cx, cy, cz);
		}
		public void Write(Vector3 v3, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_xEnabled) xcrusher.Write(v3.x, ref buffer, ref bitposition, bcl);
			if (cache_yEnabled) ycrusher.Write(v3.y, ref buffer, ref bitposition, bcl);
			if (cache_zEnabled) zcrusher.Write(v3.z, ref buffer, ref bitposition, bcl);

		}

		public void Write(CompressedElement nonalloc, Quaternion quat, ref ulong buffer)
		{
			int bitposition = 0;
			Write(nonalloc, quat, ref buffer, ref bitposition);
		}
		public void Write(Quaternion quat, ref ulong buffer)
		{
			int bitposition = 0;
			Write(quat, ref buffer, ref bitposition);
		}

		public void Write(CompressedElement nonalloc, Quaternion quat, ref ulong buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			ulong cq = cache_qEnabled ? qcrusher.Write(quat, ref buffer, ref bitposition) : 0;

			nonalloc.Set(this, cq);
		}
		public void Write(Quaternion quat, ref ulong buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			if (cache_qEnabled) qcrusher.Write(quat, ref buffer, ref bitposition);
		}

		public CompressedElement Write(CompressedElement ce, ref ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			return Write(ce, ref buffer, ref bitposition, bcl);
		}
		public CompressedElement Write(CompressedElement ce, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (cache_qEnabled)
				ce.cQuat.cvalue.Inject(ref buffer, ref bitposition, cache_qBits);
			else if (cache_uEnabled)
				ce.cUniform.cvalue.Inject(ref buffer, ref bitposition, cache_uBits[(int)bcl]);
			else
			{
				if (cache_xEnabled)
					ce.cx.cvalue.Inject(ref buffer, ref bitposition, cache_xBits[(int)bcl]);
				if (cache_yEnabled)
					ce.cy.cvalue.Inject(ref buffer, ref bitposition, cache_yBits[(int)bcl]);
				if (cache_zEnabled)
					ce.cz.cvalue.Inject(ref buffer, ref bitposition, cache_zBits[(int)bcl]);

			}
			return ce;
		}

		#endregion


		public Element Read(ref Bitstream buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				return qcrusher.Decompress(buffer.Read(cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				float val = ucrusher.ReadAndDecompress(ref buffer, bcl);
				return new Vector3(val, val, val);
			}

			Debug.Log("Read Element " + cache_xEnabled);
			return new Vector3(
				cache_xEnabled ? xcrusher.ReadAndDecompress(ref buffer, bcl) : 0f,
				cache_yEnabled ? ycrusher.ReadAndDecompress(ref buffer, bcl) : 0f,
				cache_zEnabled ? zcrusher.ReadAndDecompress(ref buffer, bcl) : 0f
				);
		}

		public void Read(CompressedElement nonalloc, ref Bitstream bitstream, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();
			if (TRSType == TRSType.Quaternion)
			{
				nonalloc.Set(this, bitstream.Read(cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				nonalloc.Set(this, (uint)bitstream.Read(cache_uBits[(int)bcl]));
			}
			else
			{
				CompressedFloat cx = cache_xEnabled ? new CompressedFloat(xcrusher, (uint)bitstream.Read(cache_xBits[(int)bcl])) : new CompressedFloat();
				CompressedFloat cy = cache_yEnabled ? new CompressedFloat(ycrusher, (uint)bitstream.Read(cache_yBits[(int)bcl])) : new CompressedFloat();
				CompressedFloat cz = cache_zEnabled ? new CompressedFloat(zcrusher, (uint)bitstream.Read(cache_zBits[(int)bcl])) : new CompressedFloat();

				nonalloc.Set(this, cx, cy, cz);
			}

		}


		#region ULong Buffer Readers


		public Element Read(ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();
			int bitposition = 0;

			if (TRSType == TRSType.Quaternion)
			{
				ulong c = buffer.Extract(ref bitposition, cache_qBits);
				return qcrusher.Decompress(c);
			}

			else if (cache_isUniformScale)
			{
				float val = ucrusher.ReadAndDecompress(buffer, ref bitposition, bcl);
				return new Vector3(val, val, val);
			}
			else
			{
				return new Vector3(

				cache_xEnabled ? xcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_yEnabled ? ycrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_zEnabled ? zcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f
				);
			}

		}

		/// <summary>
		/// Deserialize a compressed element directly from the buffer stream into a vector3/quaternion. This is the most efficient read, but
		/// it does not return any intermediary compressed values.
		/// </summary>
		/// <param name="buffer">Serialized source.</param>
		/// <param name="bitposition">Current read position in source.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public Element Read(ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				ulong val = buffer.Extract(ref bitposition, cache_qBits);
				return qcrusher.Decompress(val);
			}

			else if (cache_isUniformScale)
			{
				float f = ucrusher.ReadAndDecompress(buffer, ref bitposition, bcl);
				return new Vector3(f, f, f);
			}

			return new Vector3(
				cache_xEnabled ? xcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_yEnabled ? ycrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f,
				cache_zEnabled ? zcrusher.ReadAndDecompress(buffer, ref bitposition, bcl) : 0f
				);
		}

		public void Read(CompressedElement nonalloc, ulong buffer, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			Read(nonalloc, buffer, ref bitposition, bcl);
		}
		public void Read(CompressedElement nonalloc, ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				nonalloc.Set(this, (ulong)buffer.Extract(ref bitposition, cache_qBits));
			}

			else if (cache_isUniformScale)
			{
				nonalloc.Set(this, (uint)buffer.Extract(ref bitposition, cache_uBits[(int)bcl]));
			}
			else
			{
				CompressedFloat cx = cache_xEnabled ? new CompressedFloat(xcrusher, buffer.Extract(ref bitposition, cache_xBits[(int)bcl])) : new CompressedFloat();
				CompressedFloat cy = cache_yEnabled ? new CompressedFloat(ycrusher, buffer.Extract(ref bitposition, cache_yBits[(int)bcl])) : new CompressedFloat();
				CompressedFloat cz = cache_zEnabled ? new CompressedFloat(zcrusher, buffer.Extract(ref bitposition, cache_zBits[(int)bcl])) : new CompressedFloat();

				nonalloc.Set(this, cx, cy, cz);
			}
		}

		#endregion

		#region Compressors

		public void Compress(CompressedElement nonalloc, Transform trans)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					Compress(nonalloc, (local) ? trans.localPosition : trans.position);
					return;

				case TRSType.Euler:
					Compress(nonalloc, (local) ? trans.localEulerAngles : trans.eulerAngles);
					return;

				case TRSType.Quaternion:
					Compress(nonalloc, (local) ? trans.localRotation : trans.rotation);
					return;

				case TRSType.Scale:
					Compress(nonalloc, (local) ? trans.localScale : trans.lossyScale);
					return;

				default:
					XDebug.LogWarning("You are sending a transform to be crushed, but the Element Type is Generic?  Assuming position - change the crusher from Generic to the correct TRS.", true, true);
					Compress(nonalloc, (local) ? trans.localPosition : trans.position);
					return;
			}
		}
		public Bitstream Compress(Transform trans)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					Compress(reusableCE, (local) ? trans.localPosition : trans.position);
					break;

				case TRSType.Euler:
					Compress(reusableCE, (local) ? trans.localEulerAngles : trans.eulerAngles);
					break;

				case TRSType.Quaternion:
					Compress(reusableCE, (local) ? trans.localRotation : trans.rotation);
					break;

				case TRSType.Scale:
					Compress(reusableCE, (local) ? trans.localScale : trans.lossyScale);
					break;

				default:
					XDebug.LogWarning("You are sending a transform to be crushed, but the Element Type is Generic?  Assuming position - change the crusher from Generic to the correct TRS.", true, true);
					Compress(reusableCE, (local) ? trans.localPosition : trans.position);
					break;
			}
			return reusableCE.ExtractBitstream();
		}

		//[System.Obsolete()]
		//public CompressedElement Compress(Transform trans)
		//{
		//	switch (TRSType)
		//	{
		//		case TRSType.Position:
		//			return Compress((local) ? trans.localPosition : trans.position);

		//		case TRSType.Euler:
		//			return Compress((local) ? trans.localEulerAngles : trans.eulerAngles);

		//		case TRSType.Quaternion:
		//			return Compress((local) ? trans.localRotation : trans.rotation);

		//		case TRSType.Scale:
		//			return Compress((local) ? trans.localScale : trans.lossyScale);

		//		default:
		//			XDebug.LogWarning("You are sending a transform to be crushed, but the Element Type is Generic?  Assuming position - change the crusher from Generic to the correct TRS.", true, true);
		//			return Compress((local) ? trans.localPosition : trans.position);
		//	}
		//}


		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Transform trans, ref Bitstream bitstream)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					CompressAndWrite((local) ? trans.localPosition : trans.position, ref bitstream);
					break;

				case TRSType.Euler:
					CompressAndWrite((local) ? trans.localEulerAngles : trans.eulerAngles, ref bitstream);
					break;

				case TRSType.Quaternion:
					CompressAndWrite((local) ? trans.localRotation : trans.rotation, ref bitstream);
					break;

				case TRSType.Scale:
					CompressAndWrite((local) ? trans.localScale : trans.lossyScale, ref bitstream);
					break;

				default:
					XDebug.LogWarning("You are sending a transform to be crushed, but the Element Type is Generic?  Assuming position - change the crusher from Generic to the correct TRS.", true, true);
					CompressAndWrite((local) ? trans.localPosition : trans.position, ref bitstream);
					break;
			}
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Transform trans, byte[] buffer, ref int bitposition)
		{
			switch (TRSType)
			{
				case TRSType.Position:
					CompressAndWrite((local) ? trans.localPosition : trans.position, buffer, ref bitposition);
					break;

				case TRSType.Euler:
					CompressAndWrite((local) ? trans.localEulerAngles : trans.eulerAngles, buffer, ref bitposition);
					break;

				case TRSType.Quaternion:
					CompressAndWrite((local) ? trans.localRotation : trans.rotation, buffer, ref bitposition);
					break;

				case TRSType.Scale:
					CompressAndWrite((local) ? trans.localScale : trans.lossyScale, buffer, ref bitposition);
					break;

				default:
					XDebug.LogWarning("You are sending a transform to be crushed, but the Element Type is Generic?  Assuming position - change the crusher from Generic to the correct TRS.", true, true);
					CompressAndWrite((local) ? trans.localPosition : trans.position, buffer, ref bitposition);
					break;
			}
		}

		public void Compress(CompressedElement nonalloc, Element e)
		{
			if (TRSType == TRSType.Quaternion)
				Compress(nonalloc, e.quat);
			else
				Compress(nonalloc, e.v);
		}
		public Bitstream Compress(Element e)
		{
			if (TRSType == TRSType.Quaternion)
				Compress(reusableCE, e.quat);
			else
				Compress(reusableCE, e.v);

			return reusableCE.ExtractBitstream();
		}
		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Element e, ref Bitstream bitstream)
		{
			if (TRSType == TRSType.Quaternion)
				CompressAndWrite(e.quat, ref bitstream);
			else
				CompressAndWrite(e.v, ref bitstream);
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Element e, byte[] buffer, ref int bitposition)
		{
			if (TRSType == TRSType.Quaternion)
				CompressAndWrite(e.quat, buffer, ref bitposition);
			else
				CompressAndWrite(e.v, buffer, ref bitposition);
		}

		public void Compress(CompressedElement nonalloc, Vector3 v, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Quaternion)
			{
				Debug.LogError("We shouldn't be seeing this. Quats should not be getting compressed from Eulers!");
				if (cache_qEnabled)
					nonalloc.Set(this, qcrusher.Compress(Quaternion.Euler(v)));
			}
			else if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				//Debug.Log(this + " Compress <b>UNIFORM</b>");
				if (cache_uEnabled)
					nonalloc.Set(this, ucrusher.Compress((uniformAxes == UniformAxes.YZ) ? v.y : v.x));
			}

			else
			{
				//FloatCrusherUtilities.CheckBitCount(xcrusher.GetBits(0) + ycrusher.GetBits(0) + zcrusher.GetBits(0), 96);

				CompressedFloat cx = (cache_xEnabled && ((int)ia & 1) != 0 ? xcrusher.Compress(v.x) : new CompressedFloat());
				CompressedFloat cy = (cache_yEnabled && ((int)ia & 2) != 0 ? ycrusher.Compress(v.y) : new CompressedFloat());
				CompressedFloat cz = (cache_zEnabled && ((int)ia & 4) != 0 ? zcrusher.Compress(v.z) : new CompressedFloat());

				nonalloc.Set(this, cx, cy, cz);
			}
		}
		[System.Obsolete()]
		public CompressedElement Compress(Vector3 v)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Quaternion)
			{
				Debug.LogError("We shouldn't be seeing this. Quats should not be getting compressed from Eulers!");
				return (cache_qEnabled) ? new CompressedElement(this, qcrusher.Compress(Quaternion.Euler(v))) : null;
			}
			else if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				//Debug.Log(this + " Compress <b>UNIFORM</b>");
				ulong cu = (cache_uEnabled) ? ucrusher.Compress((uniformAxes == UniformAxes.YZ) ? v.y : v.x) : (ulong)0;
				return new CompressedElement(this, cu);
			}

			else
			{
				//FloatCrusherUtilities.CheckBitCount(xcrusher.GetBits(0) + ycrusher.GetBits(0) + zcrusher.GetBits(0), 96);

				var cx = (cache_xEnabled ? (uint)xcrusher.Compress(v.x) : 0);
				var cy = (cache_yEnabled ? (uint)ycrusher.Compress(v.y) : 0);
				var cz = (cache_zEnabled ? (uint)zcrusher.Compress(v.z) : 0);

				return new CompressedElement(this, cx, cy, cz);
			}
		}




		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Vector3 v, ref Bitstream bitstream, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				//Debug.Log(this + " Compress <b>UNIFORM</b>");
				ulong cu = (cache_uEnabled) ? ucrusher.Compress((uniformAxes == UniformAxes.YZ) ? v.y : v.x) : (ulong)0;
				bitstream.Write(cu, cache_uBits[0]);

			}
			else if (_trsType == TRSType.Quaternion)
			{
				Debug.Log("We shouldn't be seeing this. Quats should not be getting compressed from Eulers!");
				if (cache_qEnabled)
					bitstream.Write(qcrusher.Compress(Quaternion.Euler(v)), cache_qBits);
			}
			else
			{
				//FloatCrusherUtilities.CheckBitCount(xcrusher.GetBits(0) + ycrusher.GetBits(0) + zcrusher.GetBits(0), 96);

				if (cache_xEnabled)
					bitstream.Write(xcrusher.Compress(v.x), cache_xBits[0]);
				if (cache_yEnabled)
					bitstream.Write(ycrusher.Compress(v.y), cache_yBits[0]);
				if (cache_zEnabled)
					bitstream.Write(zcrusher.Compress(v.z), cache_zBits[0]);

			}
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Vector3 v, byte[] buffer, ref int bitposition, IncludedAxes ia = IncludedAxes.XYZ)
		{

			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				//Debug.Log(this + " Compress <b>UNIFORM</b>");
				ulong cu = (cache_uEnabled) ? ucrusher.Compress((uniformAxes == UniformAxes.YZ) ? v.y : v.x) : (ulong)0;
				buffer.Write(cu, ref bitposition, cache_uBits[0]);

			}
			else if (_trsType == TRSType.Quaternion)
			{
				Debug.Log("We shouldn't be seeing this. Quats should not be getting compressed from Eulers!");
				if (cache_qEnabled)
					buffer.Write(qcrusher.Compress(Quaternion.Euler(v)), ref bitposition, cache_qBits);
			}
			else
			{
				//FloatCrusherUtilities.CheckBitCount(xcrusher.GetBits(0) + ycrusher.GetBits(0) + zcrusher.GetBits(0), 96);
				if (cache_xEnabled)
					buffer.Write(xcrusher.Compress(v.x).cvalue, ref bitposition, cache_xBits[0]);
				if (cache_yEnabled)
					buffer.Write(ycrusher.Compress(v.y).cvalue, ref bitposition, cache_yBits[0]);
				if (cache_zEnabled)
					buffer.Write(zcrusher.Compress(v.z).cvalue, ref bitposition, cache_zBits[0]);
			}
		}

		/// <summary>
		/// Compress and bitpack the enabled vectors into a generic unsigned int.
		/// </summary>
		public void Compress(CompressedElement nonalloc, Quaternion quat)
		{
			if (!cached)
				CacheValues();

			XDebug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
				System.Enum.GetName(typeof(TRSType), TRSType) + ".", TRSType != TRSType.Quaternion, true);

			if (cache_qEnabled)
				nonalloc.Set(this, qcrusher.Compress(quat));
		}
		[System.Obsolete()]
		public CompressedElement Compress(Quaternion quat)
		{
			if (!cached)
				CacheValues();

			XDebug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
				System.Enum.GetName(typeof(TRSType), TRSType) + ".", TRSType != TRSType.Quaternion, true);

			return cache_qEnabled ? new CompressedElement(this, qcrusher.Compress(quat)) : null;
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Quaternion quat, ref Bitstream bitstream)
		{
			if (!cached)
				CacheValues();

			XDebug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
				System.Enum.GetName(typeof(TRSType), TRSType) + ".", TRSType != TRSType.Quaternion, true);

			if (cache_qEnabled)
				bitstream.Write(qcrusher.Compress(quat), cache_qBits);
		}

		/// <summary>
		/// CompressAndWrite doesn't produce any temporary 40byte bitstream structs, but rather will compress and write directly to the supplied bitstream.
		/// Use this rather than Write() and Compress() when you don't need the lossy or compressed value returned.
		/// </summary>
		public void CompressAndWrite(Quaternion quat, byte[] buffer, ref int bitposition)
		{
			if (!cached)
				CacheValues();

			XDebug.LogError("You seem to be trying to compress a Quaternion with a crusher that is set up for " +
				System.Enum.GetName(typeof(TRSType), TRSType) + ".", TRSType != TRSType.Quaternion, true);

			if (cache_qEnabled)
				buffer.Write(qcrusher.Compress(quat), ref bitposition, cache_qBits);
		}
		#endregion

		#region Decompress

		/// <summary>
		/// Decode (decompresss) and restore an element that was compressed by this crusher.
		/// </summary>
		public Element Decompress(CompressedElement compressed)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				float val = ucrusher.Decompress((uint)compressed.cUniform);
				return new Vector3(val, val, val);
			}
			else if (_trsType == TRSType.Quaternion)
			{
				return qcrusher.Decompress(compressed.cQuat);
			}
			else
			{
				// Issue log error for trying to write more than 64 bits to the ulong buffer
				//FloatCrusherUtilities.CheckBitCount(cache_TotalBits[0], 64);

				return new Vector3(
					cache_xEnabled ? (xcrusher.Decompress((uint)compressed.cx)) : 0,
					cache_yEnabled ? (ycrusher.Decompress((uint)compressed.cy)) : 0,
					cache_zEnabled ? (zcrusher.Decompress((uint)compressed.cz)) : 0
					);
			}
		}

		public Element Decompress(ulong cval, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				float val = ucrusher.Decompress((uint)cval);
				return new Vector3(val, val, val);
			}
			else if (_trsType == TRSType.Quaternion)
			{
				//Debug.Log("We should not see this! Quats should be getting called to DecompressToQuat");
				return qcrusher.Decompress(cval);
			}
			else
			{
				// Issue log error for trying to write more than 64 bits to the ulong buffer
				//FloatCrusherUtilities.CheckBitCount(cache_TotalBits[0], 64);

				int bitposition = 0;
				return new Vector3(
					(cache_xEnabled && ((int)ia & 1) != 0) ? (xcrusher.ReadAndDecompress(cval, ref bitposition)) : 0,
					(cache_yEnabled && ((int)ia & 2) != 0) ? (ycrusher.ReadAndDecompress(cval, ref bitposition)) : 0,
					(cache_zEnabled && ((int)ia & 4) != 0) ? (zcrusher.ReadAndDecompress(cval, ref bitposition)) : 0
					);
			}
		}

		//public Quaternion DecompressToQuat(CompressedElement compressed)
		//{
		//	if (!cached)
		//		CacheValues();

		//	DebugX.LogError("You seem to be trying to decompress a Quaternion from a crusher that is set up for " +
		//		System.Enum.GetName(typeof(TRSType), TRSType) + ". This likely won't end well.", TRSType != TRSType.Quaternion, true);

		//	Quaternion quat = qcrusher.Decompress(compressed.cQuat);
		//	return quat;
		//}

		#endregion

		/// <summary>
		/// Return a value clamped to the Min/Max values defined for each axis by this Crusher.
		/// </summary>
		/// <param name="v3"></param>
		/// <returns></returns>
		public Vector3 Clamp(Vector3 v3)
		{
			if (!cached)
				CacheValues();

			if (TRSType == TRSType.Quaternion)
			{
				XDebug.LogError("You cannot clamp a quaternion.");
				return v3;
			}
			if (TRSType == TRSType.Scale)
			{
				if (uniformAxes == UniformAxes.NonUniform)
				{
					return new Vector3(
						(cache_xEnabled) ? xcrusher.Clamp(v3.x) : 0,
						(cache_yEnabled) ? ycrusher.Clamp(v3.y) : 0,
						(cache_zEnabled) ? zcrusher.Clamp(v3.z) : 0
					);
				}
				else
				{
					return new Vector3(
					((uniformAxes & (UniformAxes)1) != 0) ? ucrusher.Clamp(v3.x) : 0,
					((uniformAxes & (UniformAxes)2) != 0) ? ucrusher.Clamp(v3.x) : 0,
					((uniformAxes & (UniformAxes)4) != 0) ? ucrusher.Clamp(v3.x) : 0
					);
				}
			}
			if (TRSType == TRSType.Euler)
			{
				return new Vector3(
					(cache_xEnabled) ? xcrusher.ClampRotation(v3.x) : 0,
					(cache_yEnabled) ? ycrusher.ClampRotation(v3.y) : 0,
					(cache_zEnabled) ? zcrusher.ClampRotation(v3.z) : 0
					);
			}
			else
			{
				return new Vector3(
						(cache_xEnabled) ? xcrusher.Clamp(v3.x) : 0,
						(cache_yEnabled) ? ycrusher.Clamp(v3.y) : 0,
						(cache_zEnabled) ? zcrusher.Clamp(v3.z) : 0
					);
			}
		}

		#region Apply

		/// <summary>
		/// Applies only the enabled axes to the transform, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Apply(Transform trans, CompressedElement ce, IncludedAxes ia = IncludedAxes.XYZ)
		{
			Apply(trans, Decompress(ce), ia);
		}

		/// <summary>
		/// Applies only the enabled axes to the transform, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Apply(Rigidbody rb, CompressedElement ce, IncludedAxes ia = IncludedAxes.XYZ)
		{
			Apply(rb, Decompress(ce), ia);
		}

		/// <summary>
		/// Applies only the enabled axes to the rigidbody, leaving the disabled axes untouched.
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="e"></param>
		/// <param name="ia">The indicated axis will be used (assuming they are enabled for the crusher). 
		/// For example if an object only moves up and down in place, IncludedAxes.Y would only apply the Y axis compression values, 
		/// and would leave the X and Z values as they currently are.</param>
		public void Apply(Rigidbody rb, Element e, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			switch (_trsType)
			{
				case TRSType.Quaternion:

					if (cache_qEnabled)
					{
						rb.MoveRotation(e.quat);
						//rb.rotation = e.quat;
					}

					return;

				case TRSType.Position:

					//rb.position = new Vector3(
					rb.MovePosition(new Vector3(
						cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : rb.position.x,
						cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : rb.position.y,
						cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : rb.position.z
						));
					return;

				case TRSType.Euler:

					//rb.rotation = Quaternion.Euler(
					rb.MoveRotation(Quaternion.Euler(
						cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : rb.rotation.eulerAngles.x,
						cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : rb.rotation.eulerAngles.y,
						cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : rb.rotation.eulerAngles.z
						));
					return;

				default:
					Debug.LogError("Are you trying to Apply scale to a Rigidbody?");
					return;
			}
		}

		/// <summary>
		/// Applies only the enabled axes to the transform, leaving the disabled axes untouched.
		/// </summary>
		public void Apply(Transform trans, Element e, IncludedAxes ia = IncludedAxes.XYZ)
		{
			if (!cached)
				CacheValues();

			switch (_trsType)
			{
				case TRSType.Quaternion:

					if (cache_qEnabled)
					{
						if (local)
							trans.localRotation = e.quat;
						else
							trans.rotation = e.quat;
					}

					return;

				case TRSType.Position:

					if (local)
					{
						trans.localPosition = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.localPosition.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.localPosition.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.localPosition.z
							);
					}
					else
					{
						trans.position = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.position.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.position.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.position.z
							);
					}
					return;

				case TRSType.Euler:

					if (local)
					{
						trans.localEulerAngles = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.localEulerAngles.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.localEulerAngles.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.localEulerAngles.z
							);
					}
					else
					{
						trans.eulerAngles = new Vector3(
							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.eulerAngles.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.eulerAngles.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.eulerAngles.z
							);
					}
					return;

				default:
					if (local)
					{
						if (uniformAxes == UniformAxes.NonUniform)
						{
							trans.localScale = new Vector3(

							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.localScale.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.localScale.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.localScale.z
							);
						}

						// Is a uniform scale
						else
						{
							float uniform = ((int)uniformAxes & 1) != 0 ? e.v.x : e.v.y;
							trans.localScale = new Vector3
								(
								((int)uniformAxes & 1) != 0 ? uniform : trans.localScale.x,
								((int)uniformAxes & 2) != 0 ? uniform : trans.localScale.y,
								((int)uniformAxes & 4) != 0 ? uniform : trans.localScale.z
								);
						}
					}
					else
					{
						if (uniformAxes == UniformAxes.NonUniform)
						{
							trans.localScale = new Vector3(

							cache_xEnabled && ((int)ia & 1) != 0 ? e.v.x : trans.lossyScale.x,
							cache_yEnabled && ((int)ia & 2) != 0 ? e.v.y : trans.lossyScale.y,
							cache_zEnabled && ((int)ia & 4) != 0 ? e.v.z : trans.lossyScale.z
							);
						}

						// Is a uniform scale
						else
						{
							float uniform = ((int)uniformAxes & 1) != 0 ? e.v.x : e.v.y;
							trans.localScale = new Vector3
								(
								((int)uniformAxes & 1) != 0 ? uniform : trans.lossyScale.x,
								((int)uniformAxes & 2) != 0 ? uniform : trans.lossyScale.y,
								((int)uniformAxes & 4) != 0 ? uniform : trans.lossyScale.z
								);
						}
					}
					return;
			}
		}

		//public void Apply(Transform trans, Quaternion q)
		//{
		//	if (!cached)
		//		CacheValues();

		//	if (_trsType == TRSType.Quaternion)
		//	{
		//		if (cache_qEnabled)
		//			if (local)
		//				trans.rotation = q;
		//			else
		//				trans.localRotation = q;
		//		return;
		//	}

		//	DebugX.LogError("You seem to be trying to apply a Quaternion to " + System.Enum.GetName(typeof(TRSType), _trsType) + ".", true, true);
		//}

		#endregion


		/// <summary>
		/// Return the smallest bit culling level that will be able to communicate the changes between two compressed elements.
		/// </summary>
		public BitCullingLevel FindBestBitCullLevel(CompressedElement a, CompressedElement b, BitCullingLevel maxCulling)
		{
			/// Quats can't cull upper bits, so its an all or nothing. Either the bits match or they don't
			if (TRSType == TRSType.Quaternion)
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

		private bool TestMatchingUpper(uint a, uint b, int lowerbits)
		{
			return (((a >> lowerbits) << lowerbits) == ((b >> lowerbits) << lowerbits));
		}

		public bool TestMatchingUpper(CompressedElement a, CompressedElement b, BitCullingLevel bcl)
		{
			return
				(
				TestMatchingUpper(a.cx, b.cx, xcrusher.GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cy, b.cy, ycrusher.GetBitsAtCullLevel(bcl)) &&
				TestMatchingUpper(a.cz, b.cz, zcrusher.GetBitsAtCullLevel(bcl))
				);
		}

		/// <summary>
		/// Get the total number of bits this Vector3 is set to write.
		/// </summary>
		public int TallyBits(BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			if (_trsType == TRSType.Scale && uniformAxes != UniformAxes.NonUniform)
			{
				return ucrusher.enabled ? ucrusher.GetBitsAtCullLevel(bcl) : 0;
			}
			else if (_trsType == TRSType.Quaternion)
			{
				return qcrusher.enabled ? qcrusher.Bits : 0;
			}
			else
			{
				return
					(xcrusher.GetBitsAtCullLevel(bcl)) +
					(ycrusher.GetBitsAtCullLevel(bcl)) +
					(zcrusher.GetBitsAtCullLevel(bcl));
			}
		}

		public void CopyFrom(ElementCrusher src)
		{
			_trsType = src._trsType;
			uniformAxes = src.uniformAxes;
			if (xcrusher != null && src.xcrusher != null) xcrusher.CopyFrom(src.xcrusher);
			if (ycrusher != null && src.ycrusher != null) ycrusher.CopyFrom(src.ycrusher);
			if (zcrusher != null && src.zcrusher != null) zcrusher.CopyFrom(src.zcrusher);
			if (ucrusher != null && src.ucrusher != null) ucrusher.CopyFrom(src.ucrusher);
			if (qcrusher != null && src.qcrusher != null) qcrusher.CopyFrom(src.qcrusher);
			local = src.local;

		}

		public override string ToString()
		{
			return "ElementCrusher [" + _trsType + "] ";
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ElementCrusher);
		}

		public bool Equals(ElementCrusher other)
		{
			return other != null &&
				   _trsType == other._trsType &&
				   EqualityComparer<Transform>.Default.Equals(defaultTransform, other.defaultTransform) &&
				   uniformAxes == other.uniformAxes &&
				   
				   (xcrusher == null ? (other.xcrusher == null) : xcrusher.Equals(other.xcrusher)) &&
				   (ycrusher == null ? (other.ycrusher == null) : ycrusher.Equals(other.ycrusher)) &&
				   (zcrusher == null ? (other.zcrusher == null) : zcrusher.Equals(other.zcrusher)) &&
				   (ucrusher == null ? (other.ucrusher == null) : ucrusher.Equals(other.ucrusher)) &&
				   (qcrusher == null ? (other.qcrusher == null) : qcrusher.Equals(other.qcrusher)) &&
				   
				   local == other.local;
		}

		public override int GetHashCode()
		{

			var hashCode = -1042106911;
			hashCode = hashCode * -1521134295 + _trsType.GetHashCode();
			//hashCode = hashCode * -1521134295 + EqualityComparer<Transform>.Default.GetHashCode(defaultTransform);
			hashCode = hashCode * -1521134295 + uniformAxes.GetHashCode();
			hashCode = hashCode * -1521134295 + ((xcrusher == null) ? 0 : xcrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((ycrusher == null) ? 0 : ycrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((zcrusher == null) ? 0 : zcrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((ucrusher == null) ? 0 : ucrusher.GetHashCode());
			hashCode = hashCode * -1521134295 + ((qcrusher == null) ? 0 : qcrusher.GetHashCode());
			//hashCode = hashCode * -1521134295 + EqualityComparer<FloatCrusher>.Default.GetHashCode(ycrusher);
			//hashCode = hashCode * -1521134295 + EqualityComparer<FloatCrusher>.Default.GetHashCode(zcrusher);
			//hashCode = hashCode * -1521134295 + EqualityComparer<FloatCrusher>.Default.GetHashCode(ucrusher);
			//hashCode = hashCode * -1521134295 + EqualityComparer<QuatCrusher>.Default.GetHashCode(qcrusher);
			hashCode = hashCode * -1521134295 + local.GetHashCode();

			return hashCode;
		}

		public static bool operator ==(ElementCrusher crusher1, ElementCrusher crusher2)
		{
			return EqualityComparer<ElementCrusher>.Default.Equals(crusher1, crusher2);
		}

		public static bool operator !=(ElementCrusher crusher1, ElementCrusher crusher2)
		{
			return !(crusher1 == crusher2);
		}
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(ElementCrusher))]
	[CanEditMultipleObjects]
	[AddComponentMenu("Crusher/Element Crusher")]

	public class ElementCrusherDrawer : CrusherDrawer
	{
		public const float TOP_PAD = 2f;
		public const float BTM_PAD = 2f;
		//public const float BTM_PAD_SINGLE = 2f;
		private const float TITL_HGHT = 16f;
		bool haschanged;

		private static GUIContent gc = new GUIContent();
		private int holdindent;

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(r, label, property);

			haschanged = false;

			gc.text = label.text;
			gc.tooltip = label.tooltip;

			base.OnGUI(r, property, label);

			holdindent = EditorGUI.indentLevel;

			// Hacky way to get the real object
			ElementCrusher target = (ElementCrusher)DrawerUtils.GetParent(property.FindPropertyRelative("xcrusher"));

			//SerializedProperty uniformAxes = property.FindPropertyRelative("uniformAxes");
			SerializedProperty x = property.FindPropertyRelative("xcrusher");
			SerializedProperty y = property.FindPropertyRelative("ycrusher");
			SerializedProperty z = property.FindPropertyRelative("zcrusher");
			SerializedProperty u = property.FindPropertyRelative("ucrusher");
			SerializedProperty q = property.FindPropertyRelative("qcrusher");
			SerializedProperty hideFieldName = property.FindPropertyRelative("hideFieldName");

			float xh = EditorGUI.GetPropertyHeight(x);
			float yh = EditorGUI.GetPropertyHeight(y);
			float zh = EditorGUI.GetPropertyHeight(z);
			float wh = EditorGUI.GetPropertyHeight(u);
			float qh = EditorGUI.GetPropertyHeight(q);

			
			//bool isQuatCrush = target.TRSType == TRSType.Quaternion;
			//bool isUniformScale = target.TRSType == TRSType.Scale && target.uniformAxes != 0;
			bool isWrappedInTransformCrusher = DrawerUtils.GetParent(property) is TransformCrusher;
			bool showHeader = !hideFieldName.boolValue && !isWrappedInTransformCrusher;

			Rect ir = EditorGUI.IndentedRect(r);

			float currentline = r.yMin;

			if (showHeader)
			{
				EditorGUI.LabelField(new Rect(r.xMin, currentline, r.width, LINEHEIGHT), gc); //*/ property.displayName);
				currentline += LINEHEIGHT + SPACING;
				ir.yMin += LINEHEIGHT;
			}
			else
			{
				currentline += SPACING;
			}

			//ir.yMin += currentline;
			Rect framer = ir;
			framer.height -= BTTM_MARGIN;
			Rect oframer = new Rect(framer.xMin - 1, framer.yMin - 1, framer.width + 2, framer.height + 2);
			SolidTextures.DrawTexture(oframer, SolidTextures.lowcontrast2D);
			SolidTextures.DrawTexture(framer, SolidTextures.contrastgray2D);


			//GUI.Box(new Rect(ir.xMin - 2, currentline - 2, ir.width + 4, ir.height - 2), GUIContent.none, (GUIStyle)"GroupBox");
			//SolidTextures.DrawTexture(new Rect(ir.xMin - 2, currentline -2, ir.width + 4, ir.height), SolidTextures.highcontrast2D);
			//SolidTextures.DrawTexture(new Rect(ir.xMin - 1, currentline - 1, ir.width + 2, ir.height - 1), SolidTextures.white2D);

			//SolidTextures.DrawTexture(new Rect(ir.xMin - 2, currentline - 2, ir.width + 4, ir.height - BTM_PAD), SolidTextures.lowcontrast2D);
			//SolidTextures.DrawTexture(new Rect(ir.xMin, currentline, ir.width, 16 + 1/*+ SPACING*/), SolidTextures.contrastgray2D);

			const int localtoggleleft = 60;

			float fcLeft = ir.xMin + 15;
			float enumwidth = (ir.width - 99) /2 - 1;
			float fcLeft2 = fcLeft + enumwidth+ 2;

			target.isExpanded = GUI.Toggle(new Rect(ir.xMin + 2, currentline, 16, LINEHEIGHT), target.isExpanded, GUIContent.none, (GUIStyle)"Foldout");

			if (target.enableTRSTypeSelector)
			{
				EditorGUI.indentLevel = 0;
				var trsType = (TRSType)EditorGUI.EnumPopup(new Rect(fcLeft, currentline, enumwidth, LINEHEIGHT), target.TRSType, (GUIStyle)"GV Gizmo DropDown");
				EditorGUI.indentLevel = holdindent;
				if (target.TRSType != trsType)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Select TRS Type");
					target.TRSType = trsType;
				}
			}
			else if (target.TRSType == TRSType.Quaternion || target.TRSType == TRSType.Euler)
			{
				EditorGUI.indentLevel = 0;
				var trsType = (TRSType)EditorGUI.EnumPopup(new Rect(fcLeft, currentline, enumwidth, LINEHEIGHT), GUIContent.none, (RotationType)target.TRSType, (GUIStyle)"GV Gizmo DropDown");
				EditorGUI.indentLevel = holdindent;
				if (target.TRSType != trsType)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Select Rotation Type");
					target.TRSType = trsType;
				}
			}
			else
			{
				GUIContent title = target.TRSType == TRSType.Generic ? new GUIContent(property.displayName) : new GUIContent(Enum.GetName(typeof(TRSType), target.TRSType)); // + " Crshr");
				GUI.Label(new Rect(fcLeft, currentline, r.width, LINEHEIGHT), title, (GUIStyle)"MiniBoldLabel");
				//EditorGUI.LabelField(new Rect(fcLeft, currentline, r.width, LINEHEIGHT), title, (GUIStyle)"MiniBoldLabel");
			}

			if (target.enableLocalSelector)
			{
				EditorGUI.indentLevel = 0;

				GUI.Label(new Rect(paddedwidth - localtoggleleft + 14, currentline, 80, LINEHEIGHT), new GUIContent("Lcl"), (GUIStyle)"MiniLabel");

				bool local = GUI.Toggle(new Rect(paddedwidth - localtoggleleft, currentline, 20, LINEHEIGHT), target.local, GUIContent.none, (GUIStyle)"OL Toggle");
				//bool local = GUI.Toggle(new Rect(paddedright - localtoggleleft, currentline + 1, 20, LINEHEIGHT), target.local, GUIContent.none, (GUIStyle)"OL Toggle");
				if (target.local != local)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Toggle Local");
					target.local = local;
				}

				EditorGUI.indentLevel = holdindent;

				
			}

			EditorGUI.LabelField(new Rect(paddedleft, currentline, paddedwidth, 16), target.TallyBits() + " Bits", miniLabelRight);

			// Scale Uniform Enum
			if (target.TRSType == TRSType.Scale)
			{
				EditorGUI.indentLevel = 0;

				var uniformAxes =
					(ElementCrusher.UniformAxes)EditorGUI.EnumPopup(new Rect(fcLeft2, currentline, enumwidth, LINEHEIGHT), GUIContent.none, target.uniformAxes, (GUIStyle)"GV Gizmo DropDown");
				if (target.uniformAxes != uniformAxes)
				{
					haschanged = true;
					Undo.RecordObject(property.serializedObject.targetObject, "Select Uniform Axes");
					target.uniformAxes = uniformAxes;
				}
				EditorGUI.indentLevel = holdindent;
			}


			

			if (target.isExpanded)
			{
				currentline += TITL_HGHT + SPACING;
				bool isSingleElement = (target.TRSType == TRSType.Scale && target.uniformAxes != 0) || (target.TRSType == TRSType.Quaternion);
				Rect propr = new Rect(r.xMin + PADDING, currentline, r.width - PADDING * 2, isSingleElement ? 
					((target.TRSType == TRSType.Quaternion) ? qh : wh) : 
					xh + yh + zh);

				if (target.TRSType == TRSType.Scale && target.uniformAxes != 0)
				{
					EditorGUI.PropertyField(propr, u);
				}
				else if (target.TRSType == TRSType.Quaternion)
				{
					EditorGUI.PropertyField(propr, q);
				}
				else
				{
					EditorGUI.PropertyField(propr, x);
					propr.yMin += xh;
					EditorGUI.PropertyField(propr, y);
					propr.yMin += yh;
					EditorGUI.PropertyField(propr, z);
				}
			}

			if (haschanged)
			{
				EditorUtility.SetDirty(property.serializedObject.targetObject);
				//AssetDatabase.SaveAssets();
			}

			EditorGUI.indentLevel = holdindent;

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty trsType = property.FindPropertyRelative("_trsType");
			SerializedProperty uniformAxes = property.FindPropertyRelative("uniformAxes");
			SerializedProperty x = property.FindPropertyRelative("xcrusher");
			SerializedProperty y = property.FindPropertyRelative("ycrusher");
			SerializedProperty z = property.FindPropertyRelative("zcrusher");
			SerializedProperty u = property.FindPropertyRelative("ucrusher");
			SerializedProperty q = property.FindPropertyRelative("qcrusher");
			SerializedProperty isExpanded = property.FindPropertyRelative("isExpanded");
			SerializedProperty hideFieldName = property.FindPropertyRelative("hideFieldName");

			bool showHeader = !hideFieldName.boolValue && !(DrawerUtils.GetParent(property) is TransformCrusher);
			bool isexpanded = isExpanded.boolValue;

			float topAndBottom = PADDING + TITL_HGHT  + BTTM_MARGIN + ((showHeader) ? LINEHEIGHT : 0); // + TOP_PAD : TOP_PAD;

			if (!isexpanded)
				return topAndBottom;

			if (trsType.enumValueIndex == (int)TRSType.Scale && uniformAxes.enumValueIndex != 0)
			{
				float wh = EditorGUI.GetPropertyHeight(u);
				return topAndBottom + SPACING + wh + PADDING - 1;
			}
			else if (trsType.enumValueIndex == (int)TRSType.Quaternion)
			{
				float qh = (isexpanded) ? EditorGUI.GetPropertyHeight(q) : 0;
				return topAndBottom + SPACING + qh + PADDING - 1;
			}
			else
			{
				

				float xh = EditorGUI.GetPropertyHeight(x);
				float yh = EditorGUI.GetPropertyHeight(y);
				float zh = EditorGUI.GetPropertyHeight(z);

				return topAndBottom + SPACING + xh + yh + zh + PADDING - 1;
			}
		}
	}
#endif
}
