//
// Parago Media GmbH & Co. KG, Jürgen Bäurle (jbaurle@parago.de)
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//


namespace Parago.Windows
{
	public class ProgressDialogSettings
	{
		public static ProgressDialogSettings WithLabelOnly = new ProgressDialogSettings(false, false, true);
		public static ProgressDialogSettings WithSubLabel = new ProgressDialogSettings(true, false, true);
		public static ProgressDialogSettings WithSubLabelAndCancel = new ProgressDialogSettings(true, true, true);

		public bool ShowSubLabel { get; set; }
		public bool ShowCancelButton { get; set; }
		public bool ShowProgressBarIndeterminate { get; set; }

		public ProgressDialogSettings()
		{
			ShowSubLabel = false;
			ShowCancelButton = false;
			ShowProgressBarIndeterminate = true;
		}

		public ProgressDialogSettings(bool showSubLabel, bool showCancelButton, bool showProgressBarIndeterminate)
		{
			ShowSubLabel = showSubLabel;
			ShowCancelButton = showCancelButton;
			ShowProgressBarIndeterminate = showProgressBarIndeterminate;
		}
	}
}
