using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	public List<AnimData> m_Anims;

	[System.Serializable]
	public class AnimData
	{
		public Anim m_Anim;
		public GameObject m_Root;

		public FrameData[] m_Frames;

		[System.Serializable]
		public class FrameData
		{
			public float m_Duration;
			public GameObject m_Frame;
		}
	}


	private Anim m_CurrentAnim = Anim.IDLE;
	private Anim m_NextAnim = Anim.IDLE;
	private float m_CurrentAnimProgress = 0.0f;

	private Dictionary<Anim, AnimData> m_SortedAnims = new Dictionary<Anim, AnimData>();


	public void SetAnimationInstant(Anim anim, Anim nextAnim)
	{
		m_SortedAnims[m_CurrentAnim].m_Root.SetActive(false);

		m_CurrentAnim = anim;
		m_NextAnim = nextAnim;
		m_CurrentAnimProgress = 0.0f;
		UpdateCurrentFrame();

		m_SortedAnims[m_CurrentAnim].m_Root.SetActive(true);
	}

	private void UpdateCurrentFrame()
	{
		AnimData currentAnim = m_SortedAnims[m_CurrentAnim];

		// Find current frame and activate it
		float negTimeRemainig = m_CurrentAnimProgress;
		for (int i = 0; i < currentAnim.m_Frames.Length; i++)
		{
			bool isCurrentFrame = negTimeRemainig >= 0.0f && negTimeRemainig < currentAnim.m_Frames[i].m_Duration;

			negTimeRemainig -= currentAnim.m_Frames[i].m_Duration;

			currentAnim.m_Frames[i].m_Frame.SetActive(isCurrentFrame);
		}

		// If we finished the anim, move on
		if (negTimeRemainig >= 0.0f)
		{
			SetAnimationInstant(m_NextAnim, Anim.IDLE);
		}
	}

	void Start()
	{
		foreach(AnimData animData in m_Anims)
		{
			m_SortedAnims.Add(animData.m_Anim, animData);
			animData.m_Root.SetActive(false);
		}

		SetAnimationInstant(Anim.IDLE, Anim.IDLE);
	}

	void Update()
	{
		m_CurrentAnimProgress += Time.deltaTime;

		UpdateCurrentFrame();
	}
}
