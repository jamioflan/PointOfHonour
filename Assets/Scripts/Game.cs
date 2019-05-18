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
				/*foreach (Controller controller in m_Controllers)
				{
					if (controller.HasPressedAnyButton())
					{
						m_CurrentPhase = Phase.CHARACTER_SELECT;
					}
				}
				*/
				m_CurrentPhase = Phase.CHARACTER_SELECT;
				break;
			}

			case Phase.CHARACTER_SELECT:
			{
				bool allReady = true;
				foreach(Controller controller in m_Controllers)
				{
					if(!controller.IsReady())
					{
						allReady = false;
					}
				}

				if(allReady)
				{
					m_CurrentPhase = Phase.FIGHT;
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
