using signals;

namespace command
{
	public class Command : ICommand
	{
		readonly Signal _completed;
		readonly Signal<string> _error;

		public Command()
		{
			_completed = new Signal();
			_error = new Signal<string>();
		}

		virtual public void execute()
		{
			
		}
			
		virtual protected void complete()
		{
			_completed.Dispatch();
		}

		public Signal completed 
		{ 
			get
			{
				return _completed;
			}
		}

		public Signal<string> error 
		{ 
			get
			{
				return _error;
			}
		}
	}
}