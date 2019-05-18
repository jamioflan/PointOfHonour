using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	// Public editor fields
	public float m_InitialHealth = 100.0f;
	public string m_MoveXAxis = "P1X";
	public string m_MoveYAxis = "P1Y";
	public string m_PunchAxis = "P1Punch";
	public string m_JumpAxis = "P1Jump";
	public string m_BlockAxis = "P1Block";
	public string m_KickAxis = "P1Kick";
	public string m_SpecialAxis = "P1Special";

	// Internal workings
	private float m_CurrentHealth = 100.0f;
	private List<Downgrade> m_Downgrades = new List<Downgrade>();
	private Controls m_CurrentInput;
	private Controls m_LastInput;

	// On Fire Runtime Data
	private float timeSinceLastFire = 0.0f;
	public ParticleSystem m_FireParticleGenerator;

	// 

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
		// Movement update
		m_LastInput = m_CurrentInput;
		m_CurrentInput.moveX = Input.GetAxis(m_MoveXAxis);
		m_CurrentInput.moveY = Input.GetAxis(m_MoveYAxis);
		m_CurrentInput.jump = Input.GetAxis(m_JumpAxis) > 0.0f;
		m_CurrentInput.punch = Input.GetAxis(m_PunchAxis) > 0.0f;
		m_CurrentInput.kick = Input.GetAxis(m_KickAxis) > 0.0f;
		m_CurrentInput.block = Input.GetAxis(m_BlockAxis) > 0.0f;
		m_CurrentInput.special = Input.GetAxis(m_SpecialAxis) > 0.0f;


		// Downgrade updates
		// Fire update
		timeSinceLastFire += Time.deltaTime;

        if (HasDowngrade(Downgrade.ON_FIRE) && m_CurrentHealth >= 25 & timeSinceLastFire >= 5)
        {
            m_CurrentHealth = Mathf.Max(m_CurrentHealth - 2, 25);
        }
    }
}
