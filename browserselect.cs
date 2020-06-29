// htm, html, shtml, xht, xhtml
// ftp:// http:// https://

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("BrowserSelect")]
[assembly: AssemblyDefaultAlias("BrowserSelect")]
[assembly: AssemblyProduct("BrowserSelect")]
[assembly: AssemblyDescription("BrowserSelect")]
[assembly: AssemblyCompany("Deathbaron")]
[assembly: AssemblyCopyright("Deathbaron - 2017")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyCulture("")]

namespace BrowserSelect
{
	public class BrowserSelect : Form
	{
		protected class ErrorException : Exception
		{
			public ErrorException(string message) : base(message) { }
		}
		
		protected class Browser
		{
			public String Name { get; set; }
			public String Path { get; set; }
			public String Icon { get; set; }
			
			public Browser(String Name, String Path, String Icon)
			{
				this.Name = Name;
				this.Path = Path.Trim('"');
				this.Icon = Icon;
			}
		}
		
		protected class Result
		{
			public Browser Browser { get; set; }
			public FormState State { get; set; }
			public String Url { get; set; }
			
			public Result()
			{
				this.Browser = null;
				this.State = FormState.Invalid;
				this.Url = "";
			}
		}
		
		protected enum FormState {Invalid, Browser} //TODO: add urlquery option
		
		public BrowserSelect(String[] args)
		{
			InitializeComponent(args);
		}
		
		protected Result result = new Result();
		
		private void InitializeComponent(String[] args)
		{
			try
			{
				this.SuspendLayout();
				List<Browser> browsers = new List<Browser>();
				
				if (args.Length != 1)
				{
					throw new ErrorException("Invalid Arg");
				}
				this.result.Url = args[0];
				
				//List browsers
				RegistryKey browserKeys;
				browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");
				if (browserKeys == null)
					browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
				string[] browserSubKeys = browserKeys.GetSubKeyNames();
				Array.Sort(browserSubKeys);
				foreach(string browser in browserSubKeys)
				{
					RegistryKey browserKey = browserKeys.OpenSubKey(browser);
					RegistryKey browserKeyPath = browserKey.OpenSubKey(@"shell\open\command");
					RegistryKey browserIconPath = browserKey.OpenSubKey(@"DefaultIcon");
					browsers.Add(new Browser((string)browserKey.GetValue(null), (string)browserKeyPath.GetValue(null), (string)browserIconPath.GetValue(null)));
				}
				
				if (! browsers.Any())
				{
					throw new ErrorException("No browser found");
				}
				
				// Create form
				int bw = 120;
				int bh = 60;
				int uw = 480;
				int uh = 20;
				
				int ww = 0;
				int wh = 0;
				
				this.Text = "BrowserSelect";
				this.FormBorderStyle = FormBorderStyle.FixedSingle;
				this.MaximizeBox = false;
				wh += uh;
				foreach (Browser browser in browsers)
				{
					Button btn = new Button();
					btn.Image = Icon.ExtractAssociatedIcon(browser.Path).ToBitmap(); //TODO: use icon
					btn.Text = browser.Name;
					btn.Size = new Size(bw, bh);
					btn.Location = new Point(ww, wh);
					btn.ImageAlign = ContentAlignment.TopCenter;    
					btn.TextAlign = ContentAlignment.BottomCenter;
					this.Controls.Add(btn);
					btn.Click += new EventHandler((sender, e) => { this.result.Browser = browser; this.result.State = FormState.Browser; this.Close(); });
					this.KeyPreview = true;
					this.KeyDown += new KeyEventHandler((sender, e) => { if (e.KeyCode == Keys.Escape) { this.Close(); } });
					ww += bw;
				}
				wh += bh;
				ww = Math.Max(ww, uw);
				uw = ww;
				TextBox urlBox = new TextBox();
				urlBox.Size = new Size(uw, uh);
				urlBox.Text = this.result.Url;
				urlBox.TextChanged += new EventHandler((sender, e) => { this.result.Url = ((TextBox)sender).Text; });
				this.Controls.Add(urlBox);
				this.ClientSize = new Size(ww, wh);
				
				this.FormClosing += new FormClosingEventHandler((sender, e) => 
				{
					if (this.result.Browser != null)
					{
						String bin = result.Browser.Path;
						String arg = result.Url;
						
						//TODO: fix browser extra commands, (new tab)?
						
						Process.Start(bin, arg);
					}
				});
				this.ResumeLayout(false);
			}
			catch ( ErrorException error )
			{
				MessageBox.Show(error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.Load += (s, e) => this.Close();
			}
		}
		
		[STAThread]
		public static void Main(String[] args)
		{
			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new BrowserSelect(args));
		}
	}
}
