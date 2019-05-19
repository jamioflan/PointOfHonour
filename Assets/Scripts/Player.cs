using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	// Public editor fields
	public int m_InitialHealth = 10;

	public string dispName = "UNNAMED";

	public bool Ded() { return m_CurrentHealth <= 0; }

	// Internal workings
	private bool m_IsActive = false;
	private PlayerStatBlock m_StatBlock;
	private int m_CurrentHealth = 10;
	private List<Downgrade> m_Downgrades = new List<Downgrade>();
	private Controls m_CurrentInput = new Controls();
	private Controls m_LastInput = new Controls();
	private int airJumps = 0;
	private Controller m_Controller;
	private Vector3 myVelocity;
	private readonly float moveSpeed = 4.0f;
	private readonly float gravityMag = 0.15f;
	private float startDir;

	// On Fire Runtime Data
	private float timeSinceLastFire = 0.0f;
	public ParticleSystem m_FireParticleGenerator;

	// 
	private PlayerAnimation m_animation;
	public void SetActive(Controller controller)
	{
		m_IsActive = true;
		m_Controller = controller;
		m_StatBlock = controller.statBlock;
		m_StatBlock.SetHealth(m_CurrentHealth, m_InitialHealth);
		startDir = controller.startingDir;
	}

	public void SetInactive()
	{
		m_IsActive = false;
		m_Controller = null;
	}

	public int NumDowngrades() { return m_Downgrades.Count; }

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
		m_animation = GetComponent<PlayerAnimation>();
	}

	// Update is called once per frame
	void Update()
	{
		// Skip update if we aren't active yet
		if (!m_IsActive)
			return;

		// Movement update
		m_LastInput.moveX = m_CurrentInput.moveX;
		m_LastInput.moveY = m_CurrentInput.moveY;
		m_LastInput.jump = m_CurrentInput.jump;
		m_LastInput.punch = m_CurrentInput.punch;
		m_LastInput.kick = m_CurrentInput.kick;
		m_LastInput.block = m_CurrentInput.block;
		m_LastInput.special = m_CurrentInput.special;

		m_CurrentInput.moveX = Input.GetAxis(m_Controller.m_MoveXAxis);
		m_CurrentInput.moveY = Input.GetAxis(m_Controller.m_MoveYAxis);
		m_CurrentInput.jump = Input.GetButton(m_Controller.m_JumpAxis);
		m_CurrentInput.punch = Input.GetButton(m_Controller.m_PunchAxis);
		m_CurrentInput.kick = Input.GetButton(m_Controller.m_KickAxis);
		m_CurrentInput.block = Input.GetButton(m_Controller.m_BlockAxis);
		m_CurrentInput.special = Input.GetButton(m_Controller.m_SpecialAxis);

		UpdatePhysics();

		// Update current attack
		if (m_CurrentAttack != AttackType.NONE)
		{
			m_CurrentLockoutTimer -= Time.deltaTime;
			if (m_CurrentLockoutTimer <= 0.0f)
			{
				EndAttack();
				m_CurrentLockoutTimer = 0f;
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

		UpdateCurrentAttack();

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

		if (onFloor)
		{
			airJumps = 0;
			if (m_animation.getCurrentAnimation() == Anim.JUMP)
			{
				m_animation.SetAnimationInstant(Anim.IDLE, Anim.IDLE);
			}
			m_Vel += new Vector3(moveSpeed * m_CurrentInput.moveX, 0, 0);
		}
		else
			m_Vel += new Vector3(moveSpeed * m_CurrentInput.moveX * 0.75f, 0, 0);

		// Jump
		if (m_CurrentInput.jump && !m_LastInput.jump)
		{
			if(onFloor)
			{
				m_Vel.y = 26.0f;
				airJumps = 0;
				m_animation.SetAnimationInstant(Anim.JUMP, Anim.JUMP);
			}
			else if (airJumps < 1)
			{
				m_Vel.y = 20.0f;
				airJumps += 1;
				m_animation.SetAnimationInstant(Anim.JUMP, Anim.JUMP);
			}
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
		if (target.y <= -5.0f)
		{
			Die();
		}

		if(onFloor)
		{
			m_Vel.y = 0.0f;
		}

		if (HasDowngrade(Downgrade.WINDY))
		{
			m_Vel.x += startDir;
		}

		m_Vel.x *= 0.65f;// * Mathf.Exp(-Time.deltaTime);
		m_Vel.y *= 0.95f;// * Mathf.Exp(-Time.deltaTime);

		transform.position = target;
		if (m_Vel.x != 0)
		{
			if (HasDowngrade(Downgrade.BACK_ATTACK))
			{
				transform.localScale = new Vector3(Mathf.Sign(m_Vel.x), 1, 1);
			}
			else
			{
				transform.localScale = new Vector3(Mathf.Sign(m_Vel.x) * -1f, 1, 1);
			}
		}
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
					if (player != null && player != this)
					{
						player.Attack(m_KickDamage);
					}
				}
				break;
		}
	}

	private void StartAttack(AttackType type, float timer)
	{
		if (m_CurrentAttack == AttackType.NONE)
		{
			m_CurrentAttack = type;
			m_CurrentLockoutTimer = timer;

			switch (m_CurrentAttack)
			{
				case AttackType.PUNCH:
					m_animation.SetAnimationInstant(Anim.PUNCH, Anim.IDLE);
					// Punch happens immediately
					foreach (Collider collider in Physics.OverlapSphere(m_PunchVolume.center, m_PunchVolume.radius))
					{
						Player player = collider.GetComponent<Player>();
						if (player != null && player != this)
						{
							player.Attack(m_PunchDamage);
						}
					}
					break;
				case AttackType.KICK:
					m_animation.SetAnimationInstant(Anim.KICK, Anim.IDLE);
					break;
				case AttackType.SPECIAL_THUNK:
				case AttackType.SPECIAL_ANGRY:
					m_animation.SetAnimationInstant(Anim.SPECIAL, Anim.IDLE);
					break;
			}
		}
		else if (type == AttackType.NONE)
		{
			m_CurrentAttack = type;
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
		m_CurrentHealth = 0;
	}

	public void Respawn()
	{
		m_CurrentHealth = m_InitialHealth;
		m_StatBlock.SetHealth(m_CurrentHealth, m_InitialHealth);
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

	[Header("ThunkSpecial")]
	public SphereCollider[] m_HitVolsL, m_HitVolsR;
	public float[] m_ThunkTimers;
	public float m_SpecialLockoutTime = 2.0f;

	private float m_ThunkProgress = 0.0f;

	private void UpdateThunkAttack()
	{
		
		float pre = m_ThunkProgress;
		float post = m_ThunkProgress + Time.deltaTime;
		for (int i = 0; i < 4; i++)
		{
			if(pre < m_ThunkTimers[i] && post >= m_ThunkTimers[i])
			{
				foreach (Collider collider in Physics.OverlapSphere(m_HitVolsL[i].center, m_HitVolsL[i].radius))
				{
					Player player = collider.GetComponent<Player>();
					if (player != null && player != this)
					{
						player.Attack(m_PunchDamage);
					}
				}
				foreach (Collider collider in Physics.OverlapSphere(m_HitVolsR[i].center, m_HitVolsR[i].radius))
				{
					Player player = collider.GetComponent<Player>();
					if (player != null && player != this)
					{
						player.Attack(m_PunchDamage);
					}
				}
			}
			pre -= m_ThunkTimers[i];
			post -= m_ThunkTimers[i];
		}

		m_ThunkProgress += Time.deltaTime;
	}

	private void UpdateCurrentAttack()
	{
		switch (m_CurrentAttack)
		{
			case AttackType.SPECIAL_ANGRY:
			case AttackType.SPECIAL_THUNK:
			{
				UpdateThunkAttack();
				break;
			}
		}
	}
}
