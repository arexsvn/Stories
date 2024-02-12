using signals;
using System.Collections.Generic;

namespace command
{
	public class CommandSequence : ICommand
	{
		public bool haltOnError = false;
		private bool _errorHit = false;
		private int _index = 0;
		readonly Signal _completed;
		readonly Signal<string> _error;
		readonly List<ICommand> _queue;

		public CommandSequence()
		{
			_completed = new Signal();
			_error = new Signal<string>();
			_queue = new List<ICommand>();
		}
			
		public virtual void execute()
		{
			reset();
			next();
		}

		public void reset()
		{
			_index = 0;
			_errorHit = false;
		}

		protected virtual void complete()
		{
			_completed.Dispatch();
		}
			
		protected virtual void next()
		{
			if (_index < _queue.Count)
			{
				if (!_errorHit || !haltOnError)
				{
					ICommand command = _queue[_index];
					_index++;
					command.completed.AddOnce(commandComplete);
					command.error.AddOnce(commandError);
					command.execute();
				}
			} 
			else
			{
				complete();
			}
		}

		private void commandComplete()
		{
			next();
		}
			
		private void commandError(string errorText)
		{
			_errorHit = true;
			_error.Dispatch(errorText);
		}

		public ICommand currentCommand
		{
			get
			{
				return _queue[_index];
			}
		}

		public ICommand previousCommand
		{
			get
			{
				return _queue[_index - 1];
			}
		}

		public ICommand add(ICommand command)
		{
			_queue.Add(command);

			return this;
		}

		public ICommand remove(ICommand command)
		{
			_queue.Remove(command);

			return this;
		}

		public ICommand pop()
		{
			_queue.RemoveAt(_queue.Count - 1);

			return this;
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

		public int index
		{
			get
			{
				return _index;
			}
		}
	}
}