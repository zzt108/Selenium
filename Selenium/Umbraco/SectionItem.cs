using OpenQA.Selenium;

namespace Selenium.Umbraco
{
	public interface ISectionItem:IUmbracoBaseElement
	{
		string Title { get; }
		IContentTree ContentTree { get; }
	}

	public class SectionItem : UmbracoBaseElement, ISectionItem
	{
		public SectionItem(IWebElement rootWebElement) : base(rootWebElement)
		{
			RootWebElement = rootWebElement.FindElement(By.CssSelector("a"));
		}
		public string Title => RootWebElement.FindElement(By.CssSelector("span")).GetAttribute("innerHTML");
		private IWebElement Tree => Driver.FindElement(By.Id("tree"));
		private IWebElement Root => Tree.FindElement(By.ClassName("root"));
		public IContentTree ContentTree
		{
			get
			{
				var readOnlyCollection = Root.FindElements(By.CssSelector("ul > li"));
				var contentTree = new ContentTree(null, readOnlyCollection);
				return contentTree;
			}
		}
	}
}