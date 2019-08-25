using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;

namespace Selenium
{
	public static class WebElementExtensions
	{
		public static void ClearAndSendKeys(this IWebElement e, string text)
		{
			e.Clear();
			e.SendKeys(text);
		}

		/// <summary>
		/// Find elements extension to override implicit wait issue
		/// </summary>
		/// <param name="element"></param>
		/// <param name="by"></param>
		/// <param name="timeOut"></param>
		/// <returns></returns>
		public static ReadOnlyCollection<IWebElement> FindElements(this IWebElement element, By by, TimeSpan timeOut)
		{
			var driver = ((IWrapsDriver)element).WrappedDriver;
			driver.Manage().Timeouts().ImplicitWait =  TimeSpan.FromMilliseconds(0);
			try
			{
				if (!timeOut.Equals(TimeSpan.Zero))
				{
					var wait = new WebDriverWait(driver, timeOut);
					return wait.Until(drv => element.FindElements(@by));
				}
				return element.FindElements(@by);

			}
			finally
			{
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(0);
			}
		}
	}
}