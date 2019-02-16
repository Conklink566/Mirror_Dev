using UnityEngine;
using emotitron.NST;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class BasicThirdPersonController : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

		[HideInInspector] public GameObject cachedGameObject;
		[HideInInspector] public Transform cachedTransform;

		private void Start()
        {
			if (!GetComponent<NSTNetAdapter>().IsMine)
				Destroy(this);

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }

		private void Awake()
		{
			cachedGameObject = gameObject;
			cachedTransform = transform;
		}

		private void Update()
        {
            if (!m_Jump)
            {
                m_Jump = Input.GetButtonDown("Jump");
            }
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
			// read inputs
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

			m_Move = Mathf.Max(0, v) * cachedTransform.forward + h * cachedTransform.right;
            //}
#if !MOBILE_INPUT
			// walk speed multiplier
	        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

            // pass all parameters to the character control script
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }
    }
}
