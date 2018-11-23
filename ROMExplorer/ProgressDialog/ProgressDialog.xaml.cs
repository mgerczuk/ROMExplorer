//
// Parago Media GmbH & Co. KG, Jürgen Bäurle (jbaurle@parago.de)
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

// taken from http://www.parago.de/blog/2011/04/22/how-to-implement-a-modern-progress-dialog-for-wpf-applications.html

namespace Parago.Windows
{
	public partial class ProgressDialog : Window
	{
		public static ProgressDialogContext Current { get; set; }

		volatile bool _isBusy;
		BackgroundWorker _worker;

		public string Label
		{
			get { return TextLabel.Text; }
			set { TextLabel.Text = value; }
		}

		public string SubLabel
		{
			get { return SubTextLabel.Text; }
			set { SubTextLabel.Text = value; }
		}

		internal ProgressDialogResult Result { get; private set; }

		public ProgressDialog(ProgressDialogSettings settings)
		{
			InitializeComponent();

			if(settings == null)
				settings = ProgressDialogSettings.WithLabelOnly;

			if(settings.ShowSubLabel)
			{
				Height = 140;
				MinHeight = 140;
				SubTextLabel.Visibility = Visibility.Visible;
			}
			else
			{
				Height = 110;
				MinHeight = 110;
				SubTextLabel.Visibility = Visibility.Collapsed;
			}

			CancelButton.Visibility = settings.ShowCancelButton ? Visibility.Visible : Visibility.Collapsed;

			ProgressBar.IsIndeterminate = settings.ShowProgressBarIndeterminate;
		}

		internal ProgressDialogResult Execute(object operation)
		{
			if(operation == null)
				throw new ArgumentNullException("operation");

			ProgressDialogResult result = null;

			_isBusy = true;

			_worker = new BackgroundWorker();
			_worker.WorkerReportsProgress = true;
			_worker.WorkerSupportsCancellation = true;

			_worker.DoWork +=
				(s, e) => {

					try
					{
						ProgressDialog.Current = new ProgressDialogContext(s as BackgroundWorker, e as DoWorkEventArgs);

						if(operation is Action)
							((Action)operation)();
						else if(operation is Func<object>)
							e.Result = ((Func<object>)operation)();
						else
							throw new InvalidOperationException("Operation type is not supoorted");

						// NOTE: Always do this check in order to avoid default processing after the Cancel button has been pressed.
						// This call will set the Cancelled flag on the result structure.
						ProgressDialog.Current.CheckCancellationPending();
					}
					catch(ProgressDialogCancellationExcpetion)
					{ }
					catch(Exception ex)
					{
						if(!ProgressDialog.Current.CheckCancellationPending())
							throw ex;
					}
					finally
					{
						ProgressDialog.Current = null;
					}

				};

			_worker.RunWorkerCompleted +=
				(s, e) => {

					result = new ProgressDialogResult(e);

					Dispatcher.BeginInvoke(DispatcherPriority.Send, (SendOrPostCallback)delegate {
						_isBusy = false;
						Close();
					}, null);

				};

			_worker.ProgressChanged +=
				(s, e) => {

					if(!_worker.CancellationPending)
					{
						SubLabel = (e.UserState as string) ?? string.Empty;
						ProgressBar.Value = e.ProgressPercentage;
					}

				};

			_worker.RunWorkerAsync();

			ShowDialog();

			return result;
		}

		void OnCancelButtonClick(object sender, RoutedEventArgs e)
		{
			if(_worker != null && _worker.WorkerSupportsCancellation)
			{
				SubLabel = "Please wait while process will be cancelled...";
				CancelButton.IsEnabled = false;
				_worker.CancelAsync();
			}
		}

		void OnClosing(object sender, CancelEventArgs e)
		{
			e.Cancel = _isBusy;
		}

		internal static ProgressDialogResult Execute(Window owner, string label, Action operation)
		{
			return ExecuteInternal(owner, label, (object)operation, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, Action operation, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, (object)operation, settings);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, Func<object> operationWithResult)
		{
			return ExecuteInternal(owner, label, (object)operationWithResult, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, Func<object> operationWithResult, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, (object)operationWithResult, settings);
		}

		internal static void Execute(Window owner, string label, Action operation, Action<ProgressDialogResult> successOperation, Action<ProgressDialogResult> failureOperation = null, Action<ProgressDialogResult> cancelledOperation = null)
		{
			ProgressDialogResult result = ExecuteInternal(owner, label, operation, null);

			if(result.Cancelled && cancelledOperation != null)
				cancelledOperation(result);
			else if(result.OperationFailed && failureOperation != null)
				failureOperation(result);
			else if(successOperation != null)
				successOperation(result);
		}

		internal static ProgressDialogResult ExecuteInternal(Window owner, string label, object operation, ProgressDialogSettings settings)
		{
			ProgressDialog dialog = new ProgressDialog(settings);
			dialog.Owner = owner;

			if(!string.IsNullOrEmpty(label))
				dialog.Label = label;

			return dialog.Execute(operation);
		}
	}
}
