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

			case Phase.FIGHT:
			{
				foreach (Controller controller in m_Controllers)
				{
					Player player = controller.player();
					if (player == null)
					{
						Debug.LogError("OH NO");
						return;
					}
				}
				if (m_Controllers.Length > 1)
				{
					UpdatePlayer(m_Controllers[0].player(), m_Controllers[1].player());
					UpdatePlayer(m_Controllers[1].player(), m_Controllers[0].player());
				}
				else
					Debug.Log("Only one player");
				break;
			}

			case Phase.RESULTS:
			{
				if(Input.GetAxis("Start") > 0.0f)
				{
					Application.LoadLevel(Application.loadedLevel);
				}
				break;
			}
		}
    }

	private void UpdatePlayer( Player update, Player other )
	{
		if (update.Ded())
		{
			switch (other.NumDowngrades())
			{
				case 0:
					other.GiveDowngrade(Downgrade.WINDY);
					break;
				case 1:
					other.GiveDowngrade(Downgrade.BACK_ATTACK);
					break;
				case 2:
					other.GiveDowngrade(Downgrade.ON_FIRE);
					break;
				case 3:
					DeclareWinner(other);
					break;
			}

			//update.Respawn();
		}
	}


	public TextMesh winnerText;
	public TextMesh restartText;

	private void DeclareWinner(Player winner)
	{
		restartText.gameObject.SetActive(true);
		winnerText.gameObject.SetActive(true);
		winnerText.text = winner.dispName + " wins!";

		m_CurrentPhase = Phase.RESULTS;
	}
}


