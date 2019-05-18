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

	public Phase m_CurrentPhase { get; private set; }

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
				
				break;
			}

			case Phase.CHARACTER_SELECT:
			{

				break;
			}
		}
    }
}
