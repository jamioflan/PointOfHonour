using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public enum Phase
	{
		START,
		CHARACTER_SELECT,
		FIGHT,
		RESULTS,
	}

	public GameObject m_PressAScreen;
	public GameObject[] m_CharacterSelect;
	public GameObject[] m_CharacterConfirmed;
	public Controller[] m_Controllers;

	public Phase m_CurrentPhase { get; private set; } = Phase.START;

    void Start()
    {
		m_CurrentPhase = Phase.START;

	}

    void Update()
    {
        switch(m_CurrentPhase)
		{
			case Phase.START:
			{
				if(Input.GetAxis("Start") > 0.0f)
				{ 
					m_PressAScreen.SetActive(false);
					m_CurrentPhase = Phase.CHARACTER_SELECT;
				}
				break;
			}

			case Phase.CHARACTER_SELECT:
			{
				bool allReady = true;
				for(int i = 0; i < m_Controllers.Length; i++)
				{
					if (!m_Controllers[i].IsReady())
					{
						allReady = false;
					}
					m_CharacterSelect[i].SetActive(!m_Controllers[i].IsReady());
					m_CharacterConfirmed[i].SetActive(m_Controllers[i].IsReady());
				}

				if (allReady)
				{
					m_CurrentPhase = Phase.FIGHT;
					m_CharacterConfirmed[0].SetActive(false);
					m_CharacterConfirmed[1].SetActive(false);

					foreach (Controller controller in m_Controllers)
					{
						controller.StartGame();
					}
				}
				break;
			}
		}
    }
}
