using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smooth;
using App.Level;

namespace App.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMotor : MonoBehaviour
    {
        /// <summary>
        /// velocity
        /// </summary>
        private Vector3 _Velocity = Vector3.zero;

        /// <summary>
        /// Rotation
        /// </summary>
        private Vector3 _Rotation = Vector3.zero;

        /// <summary>
        /// Camera Rotation
        /// </summary>
        private Vector3 _CameraRotaton = Vector3.zero;

        /// <summary>
        /// Get this component
        /// </summary>
        public Rigidbody Rigidbody { get; set; }

        /// <summary>
        /// Animator Component
        /// </summary>
        [SerializeField]
        private Animator _Animator;

        /// <summary>
        /// Get this component
        /// </summary>
        [SerializeField]
        private GameObject _CameraPivot;

        /// <summary>
        /// Min/Max Camera Rotation for X
        /// </summary>
        [SerializeField]
        private Vector2 _MinMaxCameraRotationX;

        /// <summary>
        /// Check to see if grounded;
        /// </summary>
        public bool IsGrounded = true;

        /// <summary>
        /// Start this instance
        /// </summary>
        private void Start()
        {
            this.Rigidbody = this.GetComponent<Rigidbody>();
            this._Animator.SetBool("IsRunning", false);
        }

        /// <summary>
        /// Move this object
        /// </summary>
        /// <param name="velocity"></param>
        public void Move(Vector3 velocity)
        {
            this._Velocity = velocity;
        }

        /// <summary>
        /// Move this object
        /// </summary>
        /// <param name="rotation"></param>
        public void Rotate(Vector3 rotation)
        {
            this._Rotation = rotation;
        }

        /// <summary>
        /// Jumping with force
        /// </summary>
        /// <param name="force"></param>
        public void Jump(float force)
        {
            this.Rigidbody.AddForce(Vector3.up * force, ForceMode.Acceleration);
            this.IsGrounded = false;
            this._Animator.SetBool("IsJumping", true);
        }

        /// <summary>
        /// Rotate Camera
        /// </summary>
        /// <param name="CamRotation"></param>
        public void RotateCamera(Vector3 camRotation)
        {
            this._CameraRotaton += camRotation;
        }

        /// <summary>
        /// Run every fixed frame
        /// </summary>
        private void FixedUpdate()
        {
            
            PerformMovement();
            PerformRotation();
        }

        /// <summary>
        /// Perform Movement
        /// </summary>
        private void PerformMovement()
        {
            if (this._Velocity != Vector3.zero)
            {
                this.Rigidbody.MovePosition(this.Rigidbody.position + this._Velocity * Time.fixedDeltaTime);
                this._Animator.SetBool("IsRunning", true);
                this._Velocity = Vector3.zero;
            }
            else
            {
                this._Animator.SetBool("IsRunning", false);
            }
        }

        /// <summary>
        /// Perform Rotation
        /// </summary>
        private void PerformRotation()
        {
            if (this._Rotation != Vector3.zero)
            {
                this.Rigidbody.MoveRotation(this.Rigidbody.rotation * Quaternion.Euler(this._Rotation));
                this._Rotation = Vector3.zero;
            }
            if(this._CameraRotaton != Vector3.zero &&
                this._CameraPivot != null)
            {
                if (this._CameraRotaton.x > this._MinMaxCameraRotationX.y)
                    this._CameraRotaton = new Vector3(this._MinMaxCameraRotationX.y, this._CameraRotaton.y, this._CameraRotaton.z);
                if (this._CameraRotaton.x < this._MinMaxCameraRotationX.x)
                    this._CameraRotaton = new Vector3(this._MinMaxCameraRotationX.x, this._CameraRotaton.y, this._CameraRotaton.z);
                this._CameraPivot.transform.localEulerAngles = -this._CameraRotaton;
            }
        }

        /// <summary>
        /// On Collision Enter
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            RaycastHit hit;
            MovingPlatform platform = collision.gameObject.GetComponent<MovingPlatform>();
            if(Physics.Raycast(this.transform.position, Vector3.down, out hit, 1.2f))
            {
                this._Animator.SetBool("IsJumping", false);
                this.IsGrounded = true;
            }
            if(platform == null)
                return;
            if (hit.collider == null)
                return;
            this.transform.SetParent(collision.gameObject.transform);
        }

        /// <summary>
        /// On Collision Exit
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionExit(Collision collision)
        {
            if (this.transform.parent == null)
                return;
            if (this.transform.parent == collision.gameObject.transform)
            {
                this.transform.parent = null;
            }
            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, Vector3.down, out hit, 1.2f))
            {
                this._Animator.SetBool("IsJumping", false);
                this.IsGrounded = true;
            }
            else
            {
                this._Animator.SetBool("IsJumping", true);
                this.IsGrounded = false;
            }
        }
    }
}

