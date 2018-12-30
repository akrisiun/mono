using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Mono.XLinq
{
    public class XEnumeratorElement : XEnumeratorNodeNext
    {
        public static XElement CloneRoot(XElement root)
        {
            return new XElement(root.Name, root.Attributes());
        }

        public static XElement Parse(XElement source, Func<XElement, bool> parse)
        {
            var clone = CloneRoot(source);
            var parser = new XEnumeratorElement(source);
            var result = parser.ChildrenInto(clone, parse);
            return result;
        }

        public XEnumeratorElement(XContainer source)
            : base(source)
        {
            isMoved = false;
            elBefore = null;
        }

        public XElement Root
        {
            get
            {
                return (base.nodes as XElement) ??
                       (base.nodes.Any() ? base.nodes.First<XNode>().Parent as XElement : null);
            }
        }

        public XElement ChildrenInto(XElement into, Func<XElement, bool> parse)
        {
            Reset();
            elBefore = null;
            var container = Current as XContainer;
            foreach (var child in DescendantsParse(container, parse))
            {
                Guard.Check(!child.Equals(elBefore));
                into.Add(child);
                elBefore = child;
            }
            elBefore = null;
            if (!into.HasElements 
                && parse != null)   // parse root node
                parse(into);

            return into;
        }

        #region Move Next

        public bool MoveAfter(XNode node)    // XNode abstract to XElement or XText
        {
            Guard.Check(node != null);
            if (MoveAfter(iterate, node))
            {
                if (iterate.Current != null) // && iterate.Current == next)
                    isMoved = true;
                return isMoved;
            }
            return false;
        }

        static bool MoveAfter(IEnumerator<XNode> iterate, XNode element)
        {
            var next = element.NextNode;
            if (element.Parent != null && next == element.Parent)
                next = element.Parent.NextNode;
            if (next == null)
            {
                int i = MaxIterations;
                while (iterate.MoveNext() && i > 0) i--;
                return false;
            }

            iterate.MoveNext();
            int iMax = MaxIterations / 10;
            while ((iMax--) > 0 && iterate.Current != null && iterate.Current != next)
                iterate.MoveNext();
            Guard.Check(iMax > 0);

            return (iterate.Current != null && iterate.Current == next);
        }

        XNode elBefore;
        bool isMoved;
        public override bool MoveNext()
        {
            if (isMoved)
            {
                isMoved = false;
                return Current != null;
            }
            return base.MoveNext();
        }
        
        #endregion

        public static int MaxIterations = 15000;
        private int iMax = 0;
        IEnumerable<XNode> DescendantsParse(XContainer node, Func<XElement, bool> parse)
        {
            if (this.nodes == null)
                yield break;
            Reset(node);
            var numNodes = nodes.GetEnumerator();
            numNodes.MoveNext();
            XNode root = numNodes.Current != null ? numNodes.Current.Parent : null;

            iMax = MaxIterations;
            XNode elBefore = null;

            while (iMax > 0 && MoveNext())
            {
                iMax--;
                XNode itemText = Current as XNode;

                var itemParsed = ParseItem(itemText, parse);
                if (itemParsed != null
                    && (elBefore == null || elBefore != itemParsed.Parent))
                    yield return itemParsed;

                elBefore = itemParsed;
                MoveAfter(itemText);
            }
        }

        public XElement ParseChildren(XElement item, IEnumerable<XNode> childrenNum, Func<XElement, bool> parse)
        {
            IEnumerator<XNode> children = childrenNum.GetEnumerator();

            XElement itemClone = new XElement(item.Name);
            if (item.HasAttributes)
                itemClone.Add(item.Attributes());

            XNode elBefore = null;
            XNode parent = null;

            while (iMax > 0 && children.MoveNext())
            {
                iMax--;
                XNode itemChText = children.Current;
                if (parent == null)
                    parent = itemChText.Parent;

                if (elBefore != null && elBefore == itemChText.Parent)
                    continue;
                if (itemChText.Parent != null && itemChText.Parent != parent)
                    continue;

                var itemParsed = ParseItem(itemChText, parse);
                if (itemParsed != null)
                {
                    if (itemParsed is XElement)
                    {
                        var itemEl = itemParsed as XElement;
                        if (!itemEl.HasElements)
                        {
                            if (itemEl.Name == "span" || itemEl.Name == "p" || itemEl.Name == "div")
                            {
                                // no br, hr, tr ..
                                var textEl = itemEl.Value;
                                if (string.IsNullOrWhiteSpace(textEl))
                                    itemParsed = textEl.Length > 0 ? new XText(textEl) : null;
                            }
                        }
                    }
                    if (itemParsed != null)
                    {
                        var ans = itemParsed.Ancestors().GetEnumerator();
                        if (elBefore != null)
                            while (ans.MoveNext() && ans.Current != elBefore.Parent)
                            {
                                var ansNode = ans.Current;
                                if (ansNode == elBefore)
                                {
                                    itemParsed = null;
                                    break;
                                }
                            }

                        if (itemParsed != null)
                            itemClone.Add(itemParsed);
                    }
                }
                elBefore = itemChText;
                // XEnumeratorElement.MoveAfter(children, itemChText);
            }

            return itemClone;
        }

        //else // todo
        //else
        //    using (var numerator = new XEnumeratorElement(itemCh))
        //        foreach (var el in numerator.DescendantsParse(item, parse)) // recursion
        //            if (!el.Equals(elBefore))
        //                yield return el;

        public XNode ParseItem(XNode Current, Func<XElement, bool> parse)
        {
            XNode itemText = Current;
#pragma warning disable IDE0019
            XElement item = itemText as XElement;

            if (item == null)
            {
                var root = Current.Document == null ? null : Current.Document.Root;
                if (itemText != null && itemText.Parent == root)
                    itemText = new XElement("span", itemText);
                if (itemText == null)
                    return null;

                var txtItem = itemText.ToString().Replace("\0A", "").Replace("\0D", "");
                if (string.IsNullOrWhiteSpace(txtItem))
                    return null;
                else
                    return itemText;
            }

            IEnumerable<XNode> children = null;
            bool accept = parse(item);
            if (accept)
            {
                if (!item.Equals(elBefore))
                {
                    if (!item.HasElements)
                        return item;
                }
            }
            if (item.HasElements)
                children = item.DescendantNodes();
            if (!accept)
                item = new XElement("span");

            if (children != null)
            {
                var childItem = ParseChildren(item, children, parse);
                if (childItem != null && !accept && !childItem.HasAttributes
                    && childItem.HasElements && childItem.FirstNode == childItem.LastNode)
                    return childItem.FirstNode;
                if (childItem != null)
                    return childItem;
            }

            return null;
        }

        public IList<XElement> ElementList
        {
            get
            {
                iterate = nodes.GetEnumerator();
                var list = new List<XElement>();
                while (iterate.MoveNext()) {
                    if (iterate.Current is XElement)
                        list.Add(iterate.Current as XElement);
                }
                return list;
            }
        }

    }

}
