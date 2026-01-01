
using System;
using System.Collections.Generic;
using Zen.Debug;

namespace Zen.Core
{
	public interface ILifeTime
	{
		void EndLifeTime();
		void BindLifeTime(ILifeTimeListener listener);
		void UnbindLifeTime(ILifeTimeListener listener);
	}

	public interface ILifeTimeListener
	{
		void OnLifeEnd();
	}

	public class LifeTime : ILifeTime, ILifeTimeListener
	{
		private readonly HashSet<WeakReference<ILifeTimeListener>> _listeners =
			new HashSet<WeakReference<ILifeTimeListener>>();

		private bool _finished = false;

		public void BindLifeTime(ILifeTimeListener listener)
		{
			if (_finished)
			{
				ZenLog.Error(LogCategory.System, "[Life Time] -> Try to bind listener to finished lifetime");
				return;
			}

			_listeners.Add(new WeakReference<ILifeTimeListener>(listener));
		}

		public void UnbindLifeTime(ILifeTimeListener listener)
		{
			if (_finished)
			{
				ZenLog.Error(LogCategory.System, "[Life Time] -> Try to unbind listener from finished lifetime");
				return;
			}

			_listeners.RemoveWhere(wr => !wr.TryGetTarget(out var target) || target == listener);
		}

		public void EndLifeTime()
		{
			if (_finished)
			{
				ZenLog.Error(LogCategory.System, "[Life Time] -> Try to end a LifeTime that is already finished");
				return;
			}

			_finished = true;

			foreach (var listener in _listeners)
			{
				if (listener.TryGetTarget(out var target))
				{
					target.OnLifeEnd();
				}
			}

			_listeners.Clear();
		}

		// A LifeTime can be binded to another LifeTime
		void ILifeTimeListener.OnLifeEnd()
		{
			EndLifeTime();
		}
	}
}