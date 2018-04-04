using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Selenium.Umbraco
{
	public class ContentTab : UmbracoBaseElement
	{
		public ContentTab(IWebElement rootWebElement) : base(rootWebElement)
		{
		}

		public string Title
		{
			get { return InnerHtml; }
		}
	}

	public class ContentForm : UmbracoBaseElement
	{
		protected IWebElement FormElement;
		public List<ContentTab> Tabs = new List<ContentTab>();

		public ContentForm(IWebElement rootWebElement, string name) : base(rootWebElement)
		{
			rootWebElement.Click();
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
			FormElement = wait.Until(driver =>
			{
				try
				{
					return driver.FindElement(By.Name(name));
				}
				catch (Exception)
				{
					return null;
				}
			});
			var tabContainer = FormElement.FindElement(By.CssSelector("ul.umb-nav-tabs"));
			var tabs = tabContainer.FindElements(By.CssSelector("a.ng-binding"));
			foreach (var webElement in tabs)
			{
				Tabs.Add(new ContentTab(webElement));
			}
		}
		
	}
}