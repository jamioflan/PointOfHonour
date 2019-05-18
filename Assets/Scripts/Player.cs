using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	// Public editor fields
	public float m_InitialHealth = 100.0f;

	// Internal workings
	private bool m_IsActive = false;
	private float m_CurrentHealth = 100.0f;
	private List<Downgrade> m_Downgrades = new List<Downgrade>();
	private Controls m_CurrentInput;
	private Controls m_LastInput;
	private int consecJumps;
	private Controller m_Controller;
	private bool canDoubleJump;
	private Vector3 myVelocity;
	private float jumpHeight = 0.8f;

	// On Fire Runtime Data
	private float timeSinceLastFire = 0.0f;
	public ParticleSystem m_FireParticleGenerator;

	// 

	public void SetActive(Controller controller)
	{
		m_IsActive = true;
		m_Controller = controller;
	}

	public void SetInactive()
	{
		m_IsActive = false;
		m_Controller = null;
	}

	public void GiveDowngrade(Downgrade downgrade)
	{
		m_Downgrades.Add(downgrade);
	}

	public bool HasDowngrade(Downgrade downgrade)
	{
		return m_Downgrades.Contains(downgrade);
	}

    // Start is called before the first frame update
    void Start()
    {
        m_CurrentHealth = m_InitialHealth;
	}

    // Update is called once per frame
    void Update()
    {
		// Skip update if we aren't active yet
		if (!m_IsActive)
			return;

		// Movement update
		m_LastInput = m_CurrentInput;
		m_CurrentInput.moveX = Input.GetAxis(m_Controller.m_MoveXAxis);
		m_CurrentInput.moveY = Input.GetAxis(m_Controller.m_MoveYAxis);
		m_CurrentInput.jump = Input.GetAxis(m_Controller.m_JumpAxis) > 0.0f;
		m_CurrentInput.punch = Input.GetAxis(m_Controller.m_PunchAxis) > 0.0f;
		m_CurrentInput.kick = Input.GetAxis(m_Controller.m_KickAxis) > 0.0f;
		m_CurrentInput.block = Input.GetAxis(m_Controller.m_BlockAxis) > 0.0f;
		m_CurrentInput.special = Input.GetAxis(m_Controller.m_SpecialAxis) > 0.0f;

		//X Axis Player Movement
		CharacterController thisChar = GetComponent<CharacterController>();
		myVelocity.x = 0.5f * myVelocity.x;
		myVelocity += new Vector3(0.1f*m_CurrentInput.moveX, 0, 0);

		//Gravity
		myVelocity += Physics.gravity*Time.deltaTime*0.4f;

		//Player Jump
		if (thisChar.isGrounded)
		{
			consecJumps = 0;
			myVelocity.y = 0;
		}

		if (m_CurrentInput.jump && !m_LastInput.jump)
		{
			if (thisChar.isGrounded)
			{
				myVelocity += new Vector3(0, jumpHeight, 0);
				consecJumps += 1;
			}
//			else if (consecJumps <= 1)
//			{
//				myVelocity += new Vector3(0, 0.8f*jumpHeight, 0);
//				consecJumps += 1;
//			}
		}

		// Push movement
		thisChar.Move(myVelocity);

		// Downgrade updates
		// Fire update
		timeSinceLastFire += Time.deltaTime;

        if (HasDowngrade(Downgrade.ON_FIRE) && m_CurrentHealth >= 25 & timeSinceLastFire >= 5)
        {
            m_CurrentHealth = Mathf.Max(m_CurrentHealth - 2, 25);
        }
    }
}
