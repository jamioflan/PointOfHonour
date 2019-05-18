using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
	public string m_MoveXAxis = "P1X";
	public string m_MoveYAxis = "P1Y";
	public string m_PunchAxis = "P1Punch";
	public string m_JumpAxis = "P1Jump";
	public string m_BlockAxis = "P1Block";
	public string m_KickAxis = "P1Kick";
	public string m_SpecialAxis = "P1Special";

	public Player[] playerPrefabOptions;
	public Transform spawnPoint;

	public bool HasPressedAnyButton() { return m_HasPressedAnyButton; }
	public bool IsReady() { return m_Ready; }

	private bool m_HasPressedAnyButton = false;
	private Player m_SpawnedPlayer;
	private int m_SelectedPlayerIndex = 0;
	private bool m_Ready;
	private bool m_IsPlaying = false;
	private Controls m_LastInput;
	private Controls m_CurrentInput;

	public void CycleCharacter()
	{
		if(m_SpawnedPlayer != null)
		{
			Destroy(m_SpawnedPlayer.gameObject);
		}
		m_SelectedPlayerIndex = (m_SelectedPlayerIndex + 1) % playerPrefabOptions.Length;
		m_SpawnedPlayer = Instantiate<Player>(playerPrefabOptions[m_SelectedPlayerIndex]);
		m_SpawnedPlayer.transform.position = spawnPoint.transform.position;
		m_SpawnedPlayer.transform.rotation = Quaternion.identity;
	}

	public void StartGame()
	{
		m_IsPlaying = true;
		m_SpawnedPlayer.SetActive(this);
	}

	// Start is called before the first frame update
	void Start()
    {
		CycleCharacter();
    }

    // Update is called once per frame
    void Update()
    {
		if (!m_IsPlaying)
		{
			// Just the controls we need
			m_LastInput = m_CurrentInput;
			m_CurrentInput.jump = Input.GetAxis(m_JumpAxis) > 0.0f;
			m_CurrentInput.punch = Input.GetAxis(m_PunchAxis) > 0.0f;

			m_HasPressedAnyButton = m_HasPressedAnyButton || m_CurrentInput.jump || m_CurrentInput.punch;

			// Jump = Switch character
			if (m_CurrentInput.jump && !m_LastInput.jump)
			{
				if(m_Ready)
				{
					m_Ready = false;
				}
				else
				{
					CycleCharacter();
				}
			}

			// Punch = Confirm character
			if (m_CurrentInput.punch && !m_LastInput.punch)
			{
				m_Ready = true;
			}
		}
	}
}
