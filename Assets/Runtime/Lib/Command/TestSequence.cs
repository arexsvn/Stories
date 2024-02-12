using UnityEngine;
using command;

public class TestSequence : CommandSequence
{
	readonly string _name;

	public TestSequence(string name)
	{
		_name = name;
	}

	override public void execute()
	{
		Debug.Log(_name + ".execute");

		base.execute();
	}

	override protected void complete()
	{
		Debug.Log(_name + ".complete");

		base.complete();
	}
}