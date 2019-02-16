//Copyright 2018, Davin Carten, All rights reserved

using emotitron.Utilities;
using UnityEngine;

namespace emotitron.Compression
{
	/// <summary>
	/// A class that holds TRS (Position / Rotation / Scale) values as well as a reference to the crusher that was used to
	/// restore it, and the RotationType enum to indicate if this is using Quaterion or Eulers for rotation.
	/// </summary>
	public class Matrix
	{
		public TransformCrusher crusher;

		public Vector3 position;
		public Element rotation;
		public Vector3 scale;

		// Constructor
		public Matrix()
		{
		}
		// Constructor
		public Matrix(TransformCrusher crusher)
		{
			this.crusher = crusher;
		}
		// Constructor
		public Matrix(TransformCrusher crusher, Vector3 position, Element rotation, Vector3 scale)
		{
			this.crusher = crusher;
			this.position = position;
			this.scale = scale;
			this.rotation = rotation;
		}

		// Constructor
		public Matrix(TransformCrusher crusher, Transform transform)
		{
			this.crusher = crusher;
			this.position = transform.position;

			// Not sure the idea option for scale... lossy or local.
			this.scale = transform.localScale;

			var rotcrusher = crusher.RotCrusher;
			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
				this.rotation = transform.rotation;
			else
				this.rotation = transform.eulerAngles;
		}

		public void Set(TransformCrusher crusher, Vector3 position, Element rotation, Vector3 scale)
		{
			this.crusher = crusher;
			this.position = position;
			this.scale = scale;
			this.rotation = rotation;
		}

		public void Set(TransformCrusher crusher, Transform transform)
		{
			this.crusher = crusher;
			this.position = transform.position;

			// Not sure the idea option for scale... lossy or local.
			this.scale = transform.localScale;

			var rotcrusher = crusher.RotCrusher;
			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
				this.rotation = transform.rotation;
			else
				this.rotation = transform.eulerAngles;
		}

		public void Set(Transform transform)
		{
			this.position = transform.position;

			// Not sure the idea option for scale... lossy or local.
			this.scale = transform.localScale;

			var rotcrusher = crusher.RotCrusher;
			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
				this.rotation = transform.rotation;
			else
				this.rotation = transform.eulerAngles;
		}

		public void Clear()
		{
			this.crusher = null;
		}

		/// <summary>
		/// Compress this matrix using the crusher it was previously created with.
		/// </summary>
		/// <returns></returns>
		public void Compress(CompressedMatrix nonalloc)
		{
			crusher.Compress(nonalloc, this);
		}
		/// <summary>
		/// Compress this matrix using its associated crusher, and return a bitstream.
		/// </summary>
		/// <returns>CompressedMatrix serialized to a bitstream.</returns>
		public Bitstream Compress()
		{
			return crusher.Compress(this);
		}



		/// <summary>
		/// Apply this TRS Matrix to the default transform, using the crusher that created this TRS Matrix. Unused Axes will be left unchanged.
		/// </summary>
		[System.Obsolete("Supply the transform to Apply to. Default Transform has been deprecated to allow shared TransformCrushers.")]
		public void Apply()
		{
			crusher.Apply(this);
		}

		/// <summary>
		/// Apply this TRS Matrix to the supplied transform, using the crusher that created this TRS Matrix. Unused Axes will be left unchanged.
		/// </summary>
		public void Apply(Transform t)
		{
			if (crusher == null)
			{
				Debug.LogError("No crusher defined for this matrix. This matrix has not yet had a value assigned to it most likely, but you are trying to apply it to a transform.");
				return;
			}
			crusher.Apply(t, this);
		}

		public void Apply(Rigidbody rb)
		{
			if (crusher == null)
			{
				Debug.LogError("No crusher defined for this matrix. This matrix has not yet had a value assigned to it most likely, but you are trying to apply it to a transform.");
				return;
			}
			crusher.Apply(rb, this);
		}

		/// <summary>
		/// Lerp the position, rotation and scale of two Matrix objects, writing the results to the target Matrix.
		/// </summary>
		/// <param name="target">Result target.</param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="t"></param>
		/// <returns>Returns a reference to the supplied target matrix.</returns>
		public static Matrix Lerp(Matrix target, Matrix start, Matrix end, float t)
		{
			var crusher = end.crusher;
			target.crusher = crusher;

			target.position = Vector3.Lerp(start.position, end.position, t);

			/// TODO: Cache this
			var rotcrusher = crusher.RotCrusher;
			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
				target.rotation = Quaternion.Slerp(start.rotation, end.rotation, t);
			else
				target.rotation = Quaternion.Slerp(start.rotation, end.rotation, t).eulerAngles;

			target.scale = Vector3.Lerp(start.scale, end.scale, t);

			return target;
		}

		/// <summary>
		/// Unclamped Lerp the position, rotation and scale of two Matrix objects, writing the results to the target Matrix.
		/// </summary>
		/// <param name="target">Result target.</param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="t"></param>
		/// <returns>Returns a reference to the supplied target matrix.</returns>
		public static Matrix LerpUnclamped(Matrix target, Matrix start, Matrix end, float t)
		{
			var crusher = end.crusher;

			target.crusher = crusher;

			target.position = Vector3.LerpUnclamped(start.position, end.position, t);

			var rotcrusher = crusher.RotCrusher;

			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
				target.rotation = Quaternion.SlerpUnclamped(start.rotation, end.rotation, t);
			else
				target.rotation = Quaternion.SlerpUnclamped(start.rotation, end.rotation, t).eulerAngles;

			target.scale = Vector3.LerpUnclamped(start.scale, end.scale, t);

			return target;
		}

		public static Matrix CatmullRomLerpUnclamped(Matrix target, Matrix pre, Matrix start, Matrix end, Matrix post, float t)
		{
			var crusher = end.crusher;

			target.crusher = crusher;

			target.position = CatmulRom.CatmullRomLerp(pre.position, start.position, end.position, post.position, t);

			var rotcrusher = crusher.RotCrusher;

			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
				target.rotation = Quaternion.SlerpUnclamped(start.rotation, end.rotation, t);
			else
				target.rotation = Quaternion.SlerpUnclamped(start.rotation, end.rotation, t).eulerAngles;

			target.scale = CatmulRom.CatmullRomLerp(pre.scale, start.scale, end.scale, post.scale, t);

			return target;
		}

		public static Matrix CatmullRomLerpUnclamped(Matrix target, Matrix pre, Matrix start, Matrix end, float t)
		{
			var crusher = end.crusher;

			target.crusher = crusher;

			target.position = CatmulRom.CatmullRomLerp(pre.position, start.position, end.position, t);

			var rotcrusher = crusher.RotCrusher;

			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
				target.rotation = Quaternion.SlerpUnclamped(start.rotation, end.rotation, t);
			else
				target.rotation = Quaternion.SlerpUnclamped(start.rotation, end.rotation, t).eulerAngles;

			target.scale = CatmulRom.CatmullRomLerp(pre.scale, start.scale, end.scale, t);

			return target;
		}


		public override string ToString()
		{
			return "MATRIX pos: " + position + " rot: " + rotation + " scale: " + scale + "  rottype: " + rotation.vectorType;
		}
	}


	public static class MatrixExtensions
	{
		public static void CopyFrom(this Matrix target, Matrix src)
		{
			target.crusher = src.crusher;
			target.position = src.position;
			target.rotation = src.rotation;
			target.scale = src.scale;
		}


	}

}

