using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium.Interfaces;

namespace Selenium.Umbraco
{
	public interface IBackOffice
	{
		ISelenium Selenium { get; }
		Sections Sections { get; }
		void Login(string userName, string password);
	}

	public class BackOffice : IUmbraco, IBackOffice
	{
		#region Privates

		private Sections _sections;

		#endregion
		double _longWait = 5;

		public ISelenium Selenium { get; }

		public BackOffice(string baseUrl, Selenium.BrowserTypeEnum browserType)
		{
			Selenium = new Selenium(baseUrl, browserType);
		}

		public BackOffice(string baseUrl, Selenium.BrowserTypeEnum browserType, TimeSpan implicitWait)
		{
			Selenium = new Selenium(baseUrl, browserType, implicitWait);
		}

		public void Login(string userName, string password)
		{
			Selenium.Driver.Navigate().GoToUrl($"{Selenium.BaseUrl}/umbraco");
			var wait = new WebDriverWait(Selenium.Driver, TimeSpan.FromSeconds(_longWait));
			wait.Until(driver => driver.Url.Contains("login"));
			//var loginForm = Selenium.Driver.FindElement(By.CssSelector(".form > div:nth-child(2) > form:nth-child(2)"));
			var loginForm = Selenium.Driver.FindElement(By.Name("loginForm"));
			var userNameControl = loginForm.FindElement(By.Name("username"));
			var userPasswordControl = loginForm.FindElement(By.Name("password"));
			var loginButton = loginForm.FindElement(By.CssSelector(".btn"));
			userNameControl.SendKeys(userName);
			userPasswordControl.SendKeys(password);
			loginButton.Click();
			wait.Until(driver => driver.Url.Contains("/umbraco#/umbraco"));
		}

		public Sections Sections
		{
			get
			{
				if (_sections == null)
				{
					_sections = new Sections();
					var section = Selenium.Driver.FindElement(By.CssSelector("#applications > ul"));
					foreach (var element in section.FindElements(By.CssSelector("li.ng-scope"), TimeSpan.Zero))
					{
						_sections.Add(new SectionItem(element));
					}
				}
				return _sections;
			}
		}
	}
}