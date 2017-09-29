using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
	private MonoBehaviour monoBehaviour;

	private void Awake()
	{
		// start coroutine
		StartCoroutine(this.IEnumeratorCoroutine());
		StartCoroutine(this.IEnumeratorGenericCoroutine());
		StartCoroutine(OtherClass.IEnumeratorCoroutine());
		StartCoroutine(OtherClass.IEnumeratorGenericCoroutine());

		// string methods
		CancelInvoke("foo");
		Invoke("foo", 1);
		InvokeRepeating("foo", 1, 2);
		IsInvoking("foo");
		StartCoroutine("foo");
		StopCoroutine("foo");
		BroadcastMessage("foo");
		GetComponent("MonoBehaviour");
		SendMessage("foo");
		SendMessageUpwards("foo");

		// no string methods
		CancelInvoke();
		IsInvoking();
	}

	// Update is called once per frame
	void Update()
	{
		// cache method calls
		var foo1 = GameObject.Find("foo");
		var foo2 = GameObject.FindGameObjectWithTag("foo");
		var foo3 = GameObject.FindGameObjectsWithTag("foo");
		var foo4 = GameObject.FindObjectOfType<MonoBehaviour>();
		var foo5 = GameObject.FindObjectsOfType<MonoBehaviour>();
		var foo6 = GameObject.FindWithTag("foo");
		var foo7 = GetComponent<MonoBehaviour>();
		var foo8 = GetComponentInChildren<MonoBehaviour>();
		var foo9 = GetComponentInParent<MonoBehaviour>();
		var fooA = GetComponents<MonoBehaviour>();
		var fooB = GetComponentsInChildren<MonoBehaviour>();
		var fooC = GetComponentsInParent<MonoBehaviour>();
	}

	IEnumerator IEnumeratorCoroutine()
	{
		yield break;
	}

	IEnumerator<object> IEnumeratorGenericCoroutine()
	{
		yield break;
	}
}
