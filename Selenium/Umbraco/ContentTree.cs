using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;

namespace Selenium.Umbraco
{
	public interface IContentTree:IEnumerable<IContentTreeNode>
	{
		IContentTreeNode GetNodeByTitle(string title);
		IContentTreeNode GetNodeByContentId(string contentId);
	}

	public class ContentTree:List<IContentTreeNode>, IContentTree
	{
		public ContentTree(IContentTreeNode parent, IEnumerable<IWebElement> elements)
		{
			AddRange(elements.Select(element => new ContentTreeNode(parent, element)));
		}

		public IContentTreeNode GetNodeByTitle(string title) => Find(node => node.Title == title);

		public IContentTreeNode GetNodeByContentId(string contentId) => Find(node => node.ContentId == contentId);
	}
}