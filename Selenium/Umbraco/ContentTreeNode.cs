using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Selenium.Umbraco
{
	public interface IContentTreeNode : IUmbracoBaseElement
	{
		string Title { get; }
		string RelativeUrl { get; }
		string ContentId { get; }
		IUmbracoBaseElement Options { get; }
		void Expand();
		void Collapse();
		IContentTree ContentTree { get; }
		ContentForm ContentForm { get; }
		IContentTreeNode Parent { get; }
	}


	public class ContentTreeNode : UmbracoBaseElement, IContentTreeNode
	{
		private readonly IContentTreeNode _parent;
		private const string CssVisible = "visible";
		private const string CssVisibility = "visibility";
		private const string CssHidden = "hidden";
		private const string IconNavigationRight = "icon-navigation-right";
		private const string IconNavigationDown = "icon-navigation-down";
		private const string AttributeClass = "class";
		private string _relativeUrl;
		private int _longWait = 5000;
		private IWebElement AElement => RootWebElement.FindElement(By.TagName("a"));
		private string Href => AElement.GetAttribute("href");

		public ContentTreeNode(IContentTreeNode parent, IWebElement rootWebElement) : base(rootWebElement)
		{
			_parent = parent;
			_relativeUrl = parent == null ? "/" : $"{parent.RelativeUrl}/{Title.Replace(" ", "-")}";
		}

		public string Title
		{
			get
			{
				var wait = new WebDriverWait(Driver, TimeSpan.FromMilliseconds(_longWait));
				return wait.Until(drv =>
				{
					var element = RootWebElement.FindElement(By.TagName("a"));
					if (string.IsNullOrEmpty(element.Text))
					{
						return null;
					}
					else { return element.Text; }
				});
			}
		}

		public string RelativeUrl => _relativeUrl;

		public string ContentId
		{
			get
			{
				var sa = Href.Split('/');
				return sa.ToArray().Last();
			}
		}
		public IUmbracoBaseElement Options => new UmbracoBaseElement(RootWebElement.FindElement(By.CssSelector("a.umb-options")));
		 
		public void Expand()
		{
			var toggle = Toggle;
			var cl = toggle.GetAttribute(AttributeClass);
			var css = toggle.GetCssValue(CssVisibility);
			if (cl.Contains(IconNavigationRight) && css == CssVisible)
			{
				toggle.Click();
				var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(1));
				wait.Until(driver => toggle.GetAttribute(AttributeClass).Contains(IconNavigationDown));
			}
		}

		public void Collapse()
		{
			var toggle = Toggle;
			var cl = toggle.GetAttribute(AttributeClass);
			var css = toggle.GetCssValue(CssVisibility);
			if (cl.Contains(IconNavigationDown) && css == CssVisible)
			{
				toggle.Click();
				var wait = new WebDriverWait(Driver,TimeSpan.FromSeconds(1));
				wait.Until(driver => toggle.GetAttribute(AttributeClass).Contains(IconNavigationRight));
			}
		}

		//This gets really slow if there are NO child elements
		public IContentTree ContentTree
		{
			get
			{
				var cssValue = Toggle.GetCssValue(CssVisibility);
				if (cssValue == CssHidden)
				{
					return new ContentTree(this, new List<IWebElement>());
				}
				else
				{
					Expand();
					return new ContentTree(this, WebElementExtensions.FindElements(RootWebElement, By.CssSelector("ul > li"),TimeSpan.Zero));
				}
			}
		}

		private IWebElement Toggle => RootWebElement.FindElement(By.CssSelector("ins"));

		public ContentForm ContentForm => new ContentForm(RootWebElement, "contentForm");
		public IContentTreeNode Parent => _parent;
	}

}