using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	// Public editor fields
	public int m_InitialHealth = 10;

	// Internal workings
	private bool m_IsActive = false;
	private PlayerStatBlock m_StatBlock;
	private int m_CurrentHealth = 10;
	private List<Downgrade> m_Downgrades = new List<Downgrade>();
	private Controls m_CurrentInput;
	private Controls m_LastInput;
	private int consecJumps = 0;
	private Controller m_Controller;
	private Vector3 myVelocity;
	private float jumpTimer;
	private readonly float jumpLength = 0.1f;
	private readonly float jumpMag = 0.3f;
	private readonly float moveSpeed = 4.0f;
	private readonly float gravityMag = 0.15f;
	private readonly float dragMag = 0.13f;

	// On Fire Runtime Data
	private float timeSinceLastFire = 0.0f;
	public ParticleSystem m_FireParticleGenerator;

	// 

	public void SetActive(Controller controller)
	{
		m_IsActive = true;
		m_Controller = controller;
		m_StatBlock = controller.statBlock;
		m_StatBlock.SetHealth(m_CurrentHealth, m_InitialHealth);
	}

	public void SetInactive()
	{
		m_IsActive = false;
		m_Controller = null;
	}

	public void GiveDowngrade(Downgrade downgrade)
	{
		m_Downgrades.Add(downgrade);

		switch (downgrade)
		{
			case Downgrade.ON_FIRE:
				m_StatBlock.m_OnFire.SetActive(true);
				break;
			case Downgrade.WINDY:
				m_StatBlock.m_Windy.SetActive(true);
				break;
			case Downgrade.BACK_ATTACK:
				m_StatBlock.m_Back.SetActive(true);
				break;
		}
		
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

		UpdatePhysics();

		// Update current attack
		if (m_CurrentAttack != AttackType.NONE)
		{
			m_CurrentLockoutTimer -= Time.deltaTime;
			if (m_CurrentLockoutTimer <= 0.0f)
			{
				EndAttack();
				StartAttack(AttackType.NONE, 0.0f);
			}
		}
		// If we aren't locked out, check for attack input
		if (m_CurrentLockoutTimer <= 0.0f)
		{
			// Punch
			if (m_CurrentInput.punch && !m_LastInput.punch)
			{
				StartAttack(AttackType.PUNCH, m_PunchLockoutTime);
			}
			// Kick
			if(m_CurrentInput.kick && !m_LastInput.kick)
			{
				StartAttack(AttackType.KICK, m_KickLockoutTime);
			}
		}

		

		// Downgrade updates
		// Fire update
		timeSinceLastFire += Time.deltaTime;

        if (HasDowngrade(Downgrade.ON_FIRE) && m_CurrentHealth >= 25 & timeSinceLastFire >= 5)
        {
            m_CurrentHealth = Mathf.Max(m_CurrentHealth - 2, 25);
        }
    }

	private Vector3 m_Vel = Vector3.zero;

	private void UpdatePhysics()
	{
		// Drag and gravity
		m_Vel += Physics.gravity * gravityMag;

		// Move input
		// Switch directions quickly
		//if (Mathf.Sign(m_CurrentInput.moveX) != Mathf.Sign(m_Vel.x))
		//{
		//	m_Vel.x = 0;
		//}
		if (m_CurrentInput.moveX > 0.0f && m_Vel.x < 0.0f)
			m_Vel.x = 0.0f;
		if (m_CurrentInput.moveX < 0.0f && m_Vel.x > 0.0f)
			m_Vel.x = 0.0f;

		if(onFloor)
			m_Vel += new Vector3(moveSpeed * m_CurrentInput.moveX, 0, 0);
		else
			m_Vel += new Vector3(moveSpeed * m_CurrentInput.moveX * 0.75f, 0, 0);

		// Jump
		if (m_CurrentInput.jump && !m_LastInput.jump)
		{
			m_Vel.y = 26.0f;
		}

		// Collision detec
		Vector3 target = transform.position + m_Vel * Time.deltaTime;
		onFloor = false;
		if (-7.5f <= target.x && target.x <= 7.5f && transform.position.y >= 0.0f && target.y <= 0.0f)
		{
			target.y = 0.0f;
			onFloor = true;
		}

		if (-4.5f <= target.x && target.x <= -1.5f && transform.position.y >= 2.0f && target.y <= 2.0f)
		{
			target.y = 2.0f;
			onFloor = true;
		}

		if (1.5f <= target.x && target.x <= 4.5f && transform.position.y >= 2.0f && target.y <= 2.0f)
		{
			target.y = 2.0f;
			onFloor = true;
		}

		if (-1.5f <= target.x && target.x <= 1.5f && transform.position.y >= 4.0f && target.y <= 4.0f)
		{
			target.y = 4.0f;
			onFloor = true;
		}

		if(onFloor)
		{
			m_Vel.y = 0.0f;
		}

		m_Vel.x *= 0.65f;// * Mathf.Exp(-Time.deltaTime);
		m_Vel.y *= 0.95f;// * Mathf.Exp(-Time.deltaTime);

		transform.position = target;
	}

	private bool onFloor = false;

	private void EndAttack()
	{
		switch (m_CurrentAttack)
		{
			case AttackType.KICK:
				// Kick happens at the end
				foreach (Collider collider in Physics.OverlapSphere(m_KickVolume.center, m_KickVolume.radius))
				{
					Player player = collider.GetComponent<Player>();
					if (player != this)
					{
						player.Attack(m_KickDamage);
					}
				}
				break;
		}
	}

	private void StartAttack(AttackType type, float timer)
	{
		if (m_CurrentAttack != AttackType.NONE)
		{
			m_CurrentAttack = type;
			m_CurrentLockoutTimer = timer;

			switch (m_CurrentAttack)
			{
				case AttackType.PUNCH:
					m_animation.SetAnimationInstant(Anim.PUNCH, Anim.IDLE);
					Debug.Log("Punch Initiatiated");
					// Punch happens immediately
					foreach (Collider collider in Physics.OverlapSphere(m_PunchVolume.center, m_PunchVolume.radius))
					{
						Player player = collider.GetComponent<Player>();
						if (player != this)
						{
							player.Attack(m_PunchDamage);
						}
					}
					break;
				case AttackType.KICK:
					m_animation.SetAnimationInstant(Anim.KICK, Anim.IDLE);
					break;
			}
		}
	}

	public void Attack(int damage)
	{
		m_CurrentHealth -= damage;
		m_StatBlock.SetHealth(m_CurrentHealth, m_InitialHealth);
		if (m_CurrentHealth <= 0)
			Die();
	}

	public void Die()
	{

	}

	private enum AttackType
	{
		NONE,
		PUNCH,
		KICK,
		SPECIAL_THUNK,
		SPECIAL_DIGITO,
		SPECIAL_ANGRY,
	}

	[Header("Punch")]
	public float m_PunchLockoutTime = 0.5f;
	public int m_PunchDamage = 1;
	public SphereCollider m_PunchVolume;

	[Header("Kick")]
	public float m_KickLockoutTime = 1.5f;
	public int m_KickDamage = 2;
	public SphereCollider m_KickVolume;

	private AttackType m_CurrentAttack = AttackType.NONE;
	private float m_CurrentLockoutTimer = 0.0f;
}
