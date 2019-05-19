using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
	public float speed = 1.0f;
	private float turn;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		turn += speed * Time.deltaTime;
		transform.localEulerAngles = new Vector3(0.0f, turn, 0.0f);
    }
}
