using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Selenium.Umbraco;

namespace NUnit.Tests
{
#if !DEBUG
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Firefox)]
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Edge)]
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.InternetExplorer)]
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Chrome)]
#endif

	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Chrome)]
	public class BackofficeTest
	{
		#region Privates
		private readonly Selenium.Selenium.BrowserTypeEnum _browserType;
		private BackOffice _backOffice;
		private static readonly string WebSiteRoot;
		private static readonly string UserName;
		private static readonly string Password;
		#endregion

		static BackofficeTest()
		{
			WebSiteRoot = ConfigurationManager.AppSettings["WebSiteRoot"];
			UserName = ConfigurationManager.AppSettings["UserName"];
			Password = ConfigurationManager.AppSettings["Password"];
		}

		public BackofficeTest(Selenium.Selenium.BrowserTypeEnum browserType)
		{
			_browserType = browserType;
		}

		[SetUp]
		public void SetUp()
		{
			const string separator = "enabled";
			var appSettings = ConfigurationManager.AppSettings;
			if (appSettings.AllKeys.Where(s => s.ToLowerInvariant().Contains(separator)).Any(key => key.ToLowerInvariant().Contains(_browserType.ToString().ToLowerInvariant()) && (string.IsNullOrEmpty(appSettings[key]) || appSettings[key].ToLowerInvariant().Contains("false"))))
			{
				throw new InconclusiveException($"Browser {_browserType} disabled by configuration");
			}
		}

		[TearDown]
		public void Cleanup()
		{
			_backOffice?.Selenium.Quit();
		}

		[Test]
		public void CanLoginToUmbraco()
		{
			_backOffice = new BackOffice(WebSiteRoot, _browserType);
			_backOffice.Login(UserName, Password);
			//Assert.That(_backOffice.Selenium.Driver.Url,Is.EqualTo("https://dev.appstract.dk/umbraco#/umbraco"));
			Assert.Pass("Login passed");
		}

		[Test]
		public void CanGetSections()
		{
			_backOffice = new Selenium.Umbraco.BackOffice(WebSiteRoot, _browserType);
			_backOffice.Login(UserName, Password);
			foreach (var section in _backOffice.Sections)
			{
				Console.WriteLine($"Section:{section.Title}");
				section.Click();
				//Thread.Sleep(1000);
			}
		}

		[Test]
		public void CanGetContentTreeRoot()
		{
			_backOffice = new BackOffice(WebSiteRoot, _browserType);
			_backOffice.Login(UserName, Password);
			var content = _backOffice.Sections.Section(Sections.SectionEnum.Content);
			foreach (var treeNode in content.ContentTree)
			{
				Console.WriteLine($"Root:{treeNode.Title} - {treeNode.ContentId}");
			}
		}

		[Test]
		public void CanClickOnContentTreeOptions()
		{
			_backOffice = new BackOffice(WebSiteRoot, _browserType);
			_backOffice.Login(UserName, Password);
			var content = _backOffice.Sections.Section(Sections.SectionEnum.Content);
			foreach (var treeNode in content.ContentTree)
			{
				Console.WriteLine($"Root:{treeNode.Title} - {treeNode.ContentId}");
				treeNode.Click(); // tree node must be visible to be possible to click on its Options
				treeNode.Options.Click();
				Thread.Sleep(1000);
			}
		}

		[Test]
		public void CanGetContentTree()
		{
			_backOffice = new BackOffice(WebSiteRoot, _browserType);
			_backOffice.Login(UserName, Password);
			var contentMenu = _backOffice.Sections.Section(Sections.SectionEnum.Content);
			contentMenu.Click();
			PrintAllNodes(contentMenu.ContentTree.GetNodeByTitle("DK").ContentTree);
		}

		[Test]
		public void CanGetContentForm()
		{
			_backOffice = new BackOffice(WebSiteRoot, _browserType);
			_backOffice.Login(UserName, Password);
			var content = _backOffice.Sections.Section(Sections.SectionEnum.Content);
			content.Click();
			var form = content.ContentTree.GetNodeByTitle("DK").ContentForm;
			foreach (var tab in form.Tabs)
			{
				Console.WriteLine($"Tab:'{tab.Title}'");
			}
		}

		[Test]
		public void CanGetSiteMap()
		{
			var webSiteRoot = "http://lbf.dev.appstract.dk";
			var userName = "appstract";
			var password = "!QAZxsw2";
			_backOffice = new BackOffice(webSiteRoot, _browserType);
			_backOffice.Login(userName, Password);
			var content = _backOffice.Sections.Section(Sections.SectionEnum.Content);
			foreach (var child in content.ContentTree.Where(node => node.Title != "Recycle Bin"))
			{
				PrintAllUrls(child.ContentTree);
			}
		}

		private static void PrintAllNodes(IContentTree tree)
		{
			foreach (var node in tree)
			{
				Console.WriteLine($"Node:{node.Title} - {node.ContentId}");
				node.Expand();
				PrintAllNodes(node.ContentTree);
			}
		}
		private static void PrintAllUrls(IContentTree tree)
		{
			foreach (var node in tree)
			{
				Console.WriteLine($"{node.RelativeUrl}");
				node.Expand();
				PrintAllUrls(node.ContentTree);
			}
		}
	}
}
