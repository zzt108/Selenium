using System;
using System.Configuration;
using System.Drawing;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
//using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using Selenium.Interfaces;
using static System.String;

namespace Selenium
{
	public class Selenium : ISelenium, IDisposable
	{
		public string BaseUrl { get; }
		public BrowserTypeEnum BrowserType { get; }
		public IWebDriver Driver { get; }
		public const string BrowserEnabledSuffix = "Enabled";

		public static bool CheckBrowserEnabled(BrowserTypeEnum browserTypeEnum)
		{
			var appSettings = ConfigurationManager.AppSettings;

			foreach (var s in appSettings.AllKeys)
			{
				if (s.ToLowerInvariant().Contains($"{browserTypeEnum}{BrowserEnabledSuffix}".ToLowerInvariant()) && (IsNullOrEmpty(appSettings[s]) || appSettings[s].ToLowerInvariant().Contains("true"))) return true;
			}
			return false;
		}

		public void SetBrowserWindowSize(BrowserTypeEnum browserTypeEnum)
		{
			var browserKey = browserTypeEnum + "WindowSize";
			var value = ConfigurationManager.AppSettings[browserKey];
			if (value != null && value.Contains('x'))
			{
				var size = value.Split('x');
				Driver.Manage().Window.Position = new Point(0,0);
				Driver.Manage().Window.Size = new Size(int.Parse(size[0]), int.Parse(size[1]));
			}
		}

		public enum BrowserTypeEnum
		{
			Firefox, Chrome, InternetExplorer, Edge, Opera, 
            //PhantomJS, deprecated
            //RemoteWebDriver, used PhantomJS
            Safari
		}

		public Selenium(string baseUrl, BrowserTypeEnum browserType, TimeSpan implicitWait) : this(baseUrl, browserType)
		{
			SetImplicitWait(implicitWait);
		}

		public Selenium(string baseUrl, BrowserTypeEnum browserType)
		{
			BaseUrl = baseUrl;
			BrowserType = browserType;
			switch (browserType)
			{
				case BrowserTypeEnum.Firefox:
					Driver = new FirefoxDriver();
					break;
				case BrowserTypeEnum.Chrome:
					Driver = new ChromeDriver();
					break;
				case BrowserTypeEnum.InternetExplorer:
					Driver = new InternetExplorerDriver();
					break;
				case BrowserTypeEnum.Edge:
					Driver = new EdgeDriver();
					break;
				case BrowserTypeEnum.Opera:
					Driver = new OperaDriver();
					break;
				//case BrowserTypeEnum.PhantomJS:
				//	Driver = new PhantomJSDriver();
				//	break;
				//case BrowserTypeEnum.RemoteWebDriver:
				//	ICapabilities capabilities = DesiredCapabilities.PhantomJS();
				//	Driver = new RemoteWebDriver(capabilities);
				//	break;
				case BrowserTypeEnum.Safari:
					Driver = new SafariDriver();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(browserType), browserType, null);
			}
			var implicitWait = ConfigurationManager.AppSettings["ImplicitWaitMilliseconds"];
			SetBrowserWindowSize(browserType);
			int result;
			var wait = TimeSpan.Zero;
			if (int.TryParse(implicitWait, out result))
			{
				wait = TimeSpan.FromMilliseconds(result);
			}
			SetImplicitWait(wait);
		}

		private void SetImplicitWait(TimeSpan implicitWait)
		{
			Driver.Manage().Timeouts().ImplicitWait = implicitWait;
		}

		public virtual void Quit()
		{
			Driver.Quit();
		}

		public IWebElement FindElement(By by, int timeoutInSeconds)
		{
			if (timeoutInSeconds > 0)
			{
				var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
				return wait.Until(drv => drv.FindElement(by));
			}
			return Driver.FindElement(by);
		}

		public void Dispose()
		{
			Quit();
		}
	}
}