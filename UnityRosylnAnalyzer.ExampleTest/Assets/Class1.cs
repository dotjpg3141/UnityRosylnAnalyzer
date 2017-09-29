using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SampleBehaviour : MonoBehaviour
{

	private void Update()
	{
		var blink = Time.time % 2 < 1;
		var color = blink ? Color.red : Color.green;
		GetComponent<SpriteRenderer>().color = color;
	}

}
