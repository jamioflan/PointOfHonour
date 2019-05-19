using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public float speed;
	public float hitRad = 0.1f;
	public int damage = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		foreach (Collider collider in Physics.OverlapSphere(transform.position, hitRad))
		{
			Player player = collider.GetComponent<Player>();
			if (player != null && player != this)
			{
				player.Attack(damage);
				Destroy(this);
				return;
			}
		}

		transform.position += new Vector3(speed, 0.0f, 0.0f) * Time.deltaTime;
    }
}
