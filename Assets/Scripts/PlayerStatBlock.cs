using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatBlock : MonoBehaviour
{
	public HealthPip[] m_Pips;
	public TextMesh m_HP;

	public GameObject m_OnFire, m_Windy, m_Back;

	public void SetHealth(int hp, int max)
	{
		for(int i = 0; i < m_Pips.Length; i++)
		{
			m_Pips[i].gameObject.SetActive(i < max);
			if(i < max)
			{
				m_Pips[i].full.SetActive(i < hp);
				m_Pips[i].empty.SetActive(i >= hp);
			}
		}

		m_HP.text = hp + "/" + max;
	}

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
