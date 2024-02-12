using UnityEngine;
using System.Timers;
using command;

public class TestCommand : Command
{
	readonly string _name;
	private Timer _timer;

	public TestCommand(string name)
    {
        _name = name;
    }

    override public void execute()
	{
		Debug.Log(_name + ".execute");

		_timer = new Timer();
		_timer.Interval = 500;
		_timer.Elapsed += timerComplete;
		_timer.Start();
	}
		
	override protected void complete()
	{
		Debug.Log(_name + ".complete");

		_timer.Stop();
		_timer.Elapsed -= timerComplete;

		base.complete();
	}

	private void timerComplete(object sender, ElapsedEventArgs e)
	{
		complete();
	}
}