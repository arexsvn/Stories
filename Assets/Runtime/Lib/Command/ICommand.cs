using signals;

namespace command
{
	public interface ICommand
	{
		void execute();
		Signal completed { get; }
		Signal<string> error { get; }
	}
}