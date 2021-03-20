using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace Selenium.Interfaces
{
	public interface ISelenium
	{
        /// <summary>
        /// The base url to which all requested pages are relative
        /// </summary>
		string BaseUrl { get; }
		IWebDriver Driver { get; }
		void Quit();
	}
}