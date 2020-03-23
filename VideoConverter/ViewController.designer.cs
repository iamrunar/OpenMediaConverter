// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace VideoConverter
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButton ProcessButton { get; set; }

		[Outlet]
		AppKit.NSTextField ProcessTextField { get; set; }

		[Outlet]
		AppKit.NSButton SetupSourceDirectoryButton { get; set; }

		[Outlet]
		AppKit.NSTextField SourceDirectoryTextField { get; set; }

		[Action ("ProcessButtonClicked:")]
		partial void ProcessButtonClicked (Foundation.NSObject sender);

		[Action ("SetupSourceDirectoryButtonClicked:")]
		partial void SetupSourceDirectoryButtonClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (SetupSourceDirectoryButton != null) {
				SetupSourceDirectoryButton.Dispose ();
				SetupSourceDirectoryButton = null;
			}

			if (SourceDirectoryTextField != null) {
				SourceDirectoryTextField.Dispose ();
				SourceDirectoryTextField = null;
			}

			if (ProcessButton != null) {
				ProcessButton.Dispose ();
				ProcessButton = null;
			}

			if (ProcessTextField != null) {
				ProcessTextField.Dispose ();
				ProcessTextField = null;
			}
		}
	}
}
