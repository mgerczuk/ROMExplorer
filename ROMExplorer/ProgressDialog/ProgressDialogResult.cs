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

namespace Parago.Windows
{
	internal class ProgressDialogResult
	{
		public object Result { get; private set; }
		public bool Cancelled { get; private set; }
		public Exception Error { get; private set; }

		public bool OperationFailed
		{
			get { return Error != null; }
		}

		public ProgressDialogResult(RunWorkerCompletedEventArgs e)
		{
			if(e.Cancelled)
				Cancelled = true;
			else if(e.Error != null)
				Error = e.Error;
			else
				Result = e.Result;
		}
	}
}
