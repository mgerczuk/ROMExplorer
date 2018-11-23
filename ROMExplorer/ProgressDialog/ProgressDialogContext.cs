using System;
using System.ComponentModel;

namespace Parago.Windows
{
	public class ProgressDialogContext
	{
		public BackgroundWorker Worker { get; private set; }
		public DoWorkEventArgs Arguments { get; private set; }

		public ProgressDialogContext(BackgroundWorker worker, DoWorkEventArgs arguments)
		{
			if(worker == null)
				throw new ArgumentNullException("worker");
			if(arguments == null)
				throw new ArgumentNullException("arguments");

			Worker = worker;
			Arguments = arguments;
		}

		public bool CheckCancellationPending()
		{
			if(Worker.WorkerSupportsCancellation && Worker.CancellationPending)
				Arguments.Cancel = true;

			return Arguments.Cancel;
		}

		public void ThrowIfCancellationPending()
		{
			if(CheckCancellationPending())
				throw new ProgressDialogCancellationExcpetion();
		}

		public void Report(string message)
		{
			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(0, message);
		}

		public void Report(string format, params object[] arg)
		{
			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(0, string.Format(format, arg));
		}

		public void Report(int percentProgress, string message)
		{
			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(percentProgress, message);
		}

		public void Report(int percentProgress, string format, params object[] arg)
		{
			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(percentProgress, string.Format(format, arg));
		}

		public void ReportWithCancellationCheck(string message)
		{
			ThrowIfCancellationPending();

			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(0, message);
		}

		public void ReportWithCancellationCheck(string format, params object[] arg)
		{
			ThrowIfCancellationPending();

			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(0, string.Format(format, arg));
		}

		public void ReportWithCancellationCheck(int percentProgress, string message)
		{
			ThrowIfCancellationPending();

			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(percentProgress, message);
		}

		public void ReportWithCancellationCheck(int percentProgress, string format, params object[] arg)
		{
			ThrowIfCancellationPending();

			if(Worker.WorkerReportsProgress)
				Worker.ReportProgress(percentProgress, string.Format(format, arg));
		}
	}
}
