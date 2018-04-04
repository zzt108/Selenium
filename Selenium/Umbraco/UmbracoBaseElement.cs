using OpenQA.Selenium;
using OpenQA.Selenium.Internal;

namespace Selenium.Umbraco
{
	public interface IUmbracoBaseElement
	{
		void Click();
		string InnerHtml { get; }
	}

	public class UmbracoBaseElement : IUmbracoBaseElement
	{
		protected IWebElement RootWebElement { get; set; }
		protected IWebDriver Driver => ((IWrapsDriver)RootWebElement).WrappedDriver;

		public UmbracoBaseElement(IWebElement rootWebElement)
		{
			RootWebElement = rootWebElement;
		}

		public void Click() => RootWebElement.Click();
		public string InnerHtml
		{
			get
			{
				string contents = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].innerHTML;", RootWebElement);
				return contents;
			}
		}
	}
}