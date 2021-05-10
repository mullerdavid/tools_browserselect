// htm, html, shtml, xht, xhtml
// ftp:// http:// https://

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Text.RegularExpressions;

[assembly: AssemblyTitle("BrowserSelectv2")]
[assembly: AssemblyDefaultAlias("BrowserSelectv2")]
[assembly: AssemblyProduct("BrowserSelectv2")]
[assembly: AssemblyDescription("BrowserSelectv2")]
[assembly: AssemblyCompany("Deathbaron")]
[assembly: AssemblyCopyright("Deathbaron - 2019")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
[assembly: AssemblyCulture("")]

namespace BrowserSelect
{
	public class BrowserSelect : Form
	{
		protected class ErrorException : Exception
		{
			public ErrorException(string message) : base(message) { }
		}
		
		protected abstract class Browser
		{
			public String Name { get; set; }
			public String Path { get; set; }
			
			public Browser(String Name, String Path)
			{
				this.Name = Name;
				this.Path = Path.Trim('"');
			}
			
			public abstract Bitmap GetIconBitmap();
			
		
			public virtual void Open(String Url)
			{
				// TODO: open with default browser if Path empty
				
				if (! string.IsNullOrEmpty(Url))
				{
					Url = Regex.Replace(Url, @"(\\*)" + "\"", @"$1\$0");
					Url = Regex.Replace(Url, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"", RegexOptions.Singleline);
				}
				Process.Start(this.Path, Url);
			}
		}
		
		protected class BrowserGeneric : Browser
		{
			public String Icon { get; set; }
			
			public BrowserGeneric(String Name, String Path, String Icon ) : base(Name, Path)
			{
				this.Icon = Icon;
			}
			
			public override Bitmap GetIconBitmap()
			{
				try
				{
					return System.Drawing.Icon.ExtractAssociatedIcon(this.Path).ToBitmap(); //TODO: use icon
				}
				catch (Exception)
				{
					return null;
				}
			}
		}
		
		protected class URLScan : Browser
		{
			public static String Apikey
			{ 
				get
				{
					return GetSettingDefault("URLScanAPI","");
				}
			}
			public static String Browser
			{ 
				get
				{
					return GetSettingDefault("URLScanPATH","");
				} 
			}
			
			public URLScan( ) : base("URLScan", URLScan.Browser)
			{
			}
			
			public override Bitmap GetIconBitmap()
			{
				return new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("urlscan.png"));
			}
		
			public override void Open(String Url)
			{
				using (WebClient wc = new WebClient())
				{
					SecurityProtocolType oldprotocol = System.Net.ServicePointManager.SecurityProtocol;
					System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
					wc.UseDefaultCredentials = true;
					wc.Proxy = WebRequest.GetSystemWebProxy();
					wc.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
					
					try
					{
						wc.Headers.Set("Content-Type", "application/json");
						wc.Headers.Add("API-Key", URLScan.Apikey);
						Uri api =  new Uri("https://urlscan.io/api/v1/scan/");
						String payload = "{\"url\": \""+System.Web.HttpUtility.JavaScriptStringEncode(Url)+"\", \"public\": \"off\", \"referer\": \"\"}";
						String response = System.Text.Encoding.UTF8.GetString(wc.UploadData(api, System.Text.Encoding.ASCII.GetBytes(payload)));
						System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
						dynamic json = ser.DeserializeObject(response);
						Url = json["result"];
						base.Open(Url+"loading");
					}
					catch (WebException ex)
					{
						String resp = new System.IO.StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
						MessageBox.Show("Error in API", ex.Message+"\n\n"+resp, MessageBoxButtons.OK, MessageBoxIcon.Error);
						
					}
					
					System.Net.ServicePointManager.SecurityProtocol = oldprotocol;
				}
				
			}
		}
		
		
		protected class Virustotal : Browser
		{
			public static String Apikey
			{ 
				get
				{
					return GetSettingDefault("VirustotalAPI","");
				}
			}
			public static String Browser
			{ 
				get
				{
					return GetSettingDefault("VirustotalPATH","");
				} 
			}
			
			public Virustotal( ) : base("Virustotal", Virustotal.Browser)
			{
			}
			
			public override Bitmap GetIconBitmap()
			{
				return new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("virustotal.png"));
			}
		
			public override void Open(String Url)
			{
				using (WebClient wc = new WebClient())
				{
					SecurityProtocolType oldprotocol = System.Net.ServicePointManager.SecurityProtocol;
					System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
					wc.UseDefaultCredentials = true;
					wc.Proxy = WebRequest.GetSystemWebProxy();
					wc.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
					
					try
					{
						wc.Headers.Set("Content-Type", "application/x-www-form-urlencoded");
						Uri api =  new Uri("https://www.virustotal.com/vtapi/v2/url/scan");
						String payload = "apikey="+Virustotal.Apikey+"&url="+System.Web.HttpUtility.UrlEncode(Url);
						String response = System.Text.Encoding.UTF8.GetString(wc.UploadData(api, System.Text.Encoding.ASCII.GetBytes(payload)));
						System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
						dynamic json = ser.DeserializeObject(response);
						Url = json["permalink"];
						base.Open(Url);
					}
					catch (WebException ex)
					{
						String resp = new System.IO.StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
						MessageBox.Show("Error in API", ex.Message+"\n\n"+resp, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					
					System.Net.ServicePointManager.SecurityProtocol = oldprotocol;
				}
				
			}
		}
		
		protected class Edge : Browser
		{
			public static String InstallPath { get; private set; }
			private static String AppsFolder { get; set; }
			
			public static bool IsInstalled
			{ 
				get
				{
					return ! String.IsNullOrEmpty(Edge.InstallPath);
				}
			}
			
			static Edge()
			{
				//Computer\HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Packages\Microsoft.MicrosoftEdge_41.16299.15.0_neutral__8wekyb3d8bbwe
				RegistryKey reg = Registry.ClassesRoot.OpenSubKey(@"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Packages");
				if (reg != null)
				{
					foreach (String subkey in reg.GetSubKeyNames())
					{
						if (subkey.StartsWith("Microsoft.MicrosoftEdge"))
						{
							try
							{
								RegistryKey browserKey = reg.OpenSubKey(subkey);
								Edge.InstallPath = (string)browserKey.GetValue("Path") + "\\MicrosoftEdge.exe";
								Edge.AppsFolder = (string)browserKey.GetSubKeyNames()[0]; 
								break;
							}
							catch (Exception) {}
						}
					}
				}
				
			}
			
			[DllImport("Shell32.dll")]
			public static extern int ShellExecuteA(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirecotry, int nShowCmd);
			
			public Edge() : base("Edge", Edge.InstallPath) { }
			
			public override Bitmap GetIconBitmap()
			{
				try
				{
					return System.Drawing.Icon.ExtractAssociatedIcon(this.Path).ToBitmap();
				}
				catch (Exception)
				{
					return null;
				}
			}

			public override void Open(String Url)
			{
				ShellExecuteA(System.IntPtr.Zero, "open", @"shell:Appsfolder\"+Edge.AppsFolder, Url, null, 10);
			}
		}
		
		protected class Result
		{
			public Browser Browser { get; set; }
			public String Url { get; set; }
			
			public Result()
			{
				this.Browser = null;
				this.Url = "";
			}
		}
		
		protected Result result = new Result();
		
		public BrowserSelect(String[] args)
		{
			InitializeComponent(args);
		}
		
		private void InitializeComponent(String[] args)
		{
			try
			{
				this.SuspendLayout();
				List<Browser> browsers = new List<Browser>();
				
				if (args.Length != 1)
				{
					//throw new ErrorException("Invalid Arg");
					this.result.Url = "";
				}
				else
				{
					this.result.Url = args[0];
				}
				
				//List browsers
				RegistryKey browserKeys;
				browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");
				if (browserKeys == null)
					browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
				string[] browserSubKeys = browserKeys.GetSubKeyNames();
				Array.Sort(browserSubKeys);
				foreach(string browser in browserSubKeys)
				{
					try
					{
						RegistryKey browserKey = browserKeys.OpenSubKey(browser);
						RegistryKey browserKeyPath = browserKey.OpenSubKey(@"shell\open\command");
						RegistryKey browserIconPath = browserKey.OpenSubKey(@"DefaultIcon");
						browsers.Add(new BrowserGeneric((string)browserKey.GetValue(null), (string)browserKeyPath.GetValue(null), (string)browserIconPath.GetValue(null)));
					}
					catch (Exception) {}
				}
				
				//Add Edge
				//if (Edge.IsInstalled)
				//	browsers.Add(new Edge());
					
				
				if (! browsers.Any())
				{
					throw new ErrorException("No browser found");
				}
				
				//Add URLScan
				if (! String.IsNullOrEmpty(URLScan.Apikey))
					browsers.Add(new URLScan());
				
				//Add Virustotal
				if (! String.IsNullOrEmpty(Virustotal.Apikey))
					browsers.Add(new Virustotal());
				
				
				// Create form
				int bw = 120;
				int bh = 60;
				int uw = 480;
				int uh = 20;
				int iw = 32;
				int ih = 32;
				
				int ww = 0;
				int wh = 0;
				
				this.Text = "BrowserSelect v2";
				this.FormBorderStyle = FormBorderStyle.FixedSingle;
				this.MaximizeBox = false;
				wh += uh;
				foreach (Browser browser in browsers)
				{
					Button btnMain = new Button();
					btnMain.Image = new Bitmap(browser.GetIconBitmap(),new Size(iw,ih));;
					btnMain.Text = browser.Name;
					btnMain.Size = new Size(bw, bh);
					btnMain.Location = new Point(ww, wh);
					btnMain.ImageAlign = ContentAlignment.TopCenter;    
					btnMain.TextAlign = ContentAlignment.BottomCenter;
					this.Controls.Add(btnMain);
					btnMain.Click += new EventHandler((sender, e) => { this.result.Browser = browser; this.Close(); });
					ww += bw;
				}
				wh += bh;
				
				ww = Math.Max(ww, uw);
				uw = ww;
				TextBox urlBox = new TextBox();
				urlBox.Size = new Size(uw, uh);
				urlBox.Text = this.result.Url;
				urlBox.TextChanged += new EventHandler((sender, e) => { this.result.Url = ((TextBox)sender).Text; });
				this.KeyPreview = true;
				this.KeyDown += new KeyEventHandler((sender, e) => { if (e.KeyCode == Keys.Escape) { this.Close(); } });
				this.Controls.Add(urlBox);
				this.ClientSize = new Size(ww, wh);
				
				this.FormClosing += new FormClosingEventHandler((sender, e) => 
				{
					if (this.result.Browser != null)
					{
						result.Browser.Open(result.Url);
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
		
		private static Configuration config = null;
		
		private static void InitializeSettings()
		{
			string configfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.xml");
			config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap {ExeConfigFilename = configfile}, ConfigurationUserLevel.None);
		}
		
		protected static String GetSettingDefault(String key, String def)
		{
			KeyValueConfigurationCollection settings = config.AppSettings.Settings; 
			if (settings[key] == null)  
			{  
				settings.Add(key, def);  
			}  
			return settings[key].Value;
		}
		
		private static void SaveSetting()
		{
			if (config != null)
				config.Save(ConfigurationSaveMode.Modified);
		}
		
		[STAThread]
		public static void Main(String[] args)
		{
			InitializeSettings();
			GetSettingDefault("URLScanAPI","");
			GetSettingDefault("URLScanPATH","");
			GetSettingDefault("VirustotalAPI","");
			GetSettingDefault("VirustotalPATH","");
			
			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new BrowserSelect(args));
			
			SaveSetting();
		}
	}
}
