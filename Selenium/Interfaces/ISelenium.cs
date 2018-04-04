using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace Selenium.Interfaces
{
	public interface ISelenium
	{
		string BaseUrl { get; }
		IWebDriver Driver { get; }
		void Quit();
	}
}