﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Signum.Utilities;
using System.Windows.Automation;
using System.Linq.Expressions;
using Signum.Utilities.ExpressionTrees;

namespace Signum.Windows.UIAutomation
{
    public static class BasicExtensions
    {
        public static void ComboSelectItem(this AutomationElement combo, Expression<Func<AutomationElement, bool>> itemCondition)
        {
            combo.Pattern<ExpandCollapsePattern>().Expand();

            var item = combo.Child(itemCondition);

            item.Pattern<SelectionItemPattern>().Select();
        }

        public static AutomationElement ComboGetSelectedItem(this AutomationElement combo)
        {
            return combo.Pattern<SelectionPattern>().Current.GetSelection().SingleOrDefault();
        }

        public static void ButtonInvoke(this AutomationElement button)
        {
            button.Pattern<InvokePattern>().Invoke();
        }

        public static void Value(this AutomationElement element, string value)
        {
            element.Pattern<ValuePattern>().SetValue(value);
        }

        public static string Value(this AutomationElement element)
        {
            return element.Pattern<ValuePattern>().Current.Value;
        }

        public static void SetCheck(this AutomationElement element, bool isChecked)
        {
            if (isChecked)
                element.Check();
            else
                element.UnCheck();
        }

        public static void Check(this AutomationElement element)
        {
            var  ck= element.Pattern<TogglePattern>();
            if(ck.Current.ToggleState != ToggleState.On)
                ck.Toggle();

            if (ck.Current.ToggleState != ToggleState.On)
                throw new InvalidOperationException("The checkbox {0} has not been checked".Formato(element.Current.AutomationId));
        }

        public static void UnCheck(this AutomationElement element)
        {
            var ck = element.Pattern<TogglePattern>();
            if (ck.Current.ToggleState != ToggleState.Off)
                ck.Toggle();

            if (ck.Current.ToggleState != ToggleState.Off)
                throw new InvalidOperationException("The checkbox {0} has not been unchecked".Formato(element.Current.AutomationId));
        }



        public static int WindowAfterTimeout = 5 * 1000;

        public static AutomationElement GetWindowAfter(this AutomationElement element, Action action, Func<string> actionDescription, int? timeOut = null)
        {
            var previous = AutomationElement.RootElement.Children(a => a.Current.ProcessId == element.Current.ProcessId).Select(a => a.GetRuntimeId().ToString(".")).ToHashSet();

            action();

            AutomationElement newWindow = null;

            element.Wait(() =>
            {
                newWindow = AutomationElement.RootElement
                    .Children(a => a.Current.ProcessId == element.Current.ProcessId)
                    .FirstOrDefault(a => !previous.Contains(a.GetRuntimeId().ToString(".")));

                return newWindow != null;
            }, actionDescription, timeOut ?? WindowAfterTimeout);
            return newWindow;
        }

        public static AutomationElement GetModalWindowAfter(this AutomationElement element, Action action, Func<string> actionDescription, int? timeOut = null)
        {
            TreeWalker walker = new TreeWalker(ConditionBuilder.ToCondition(a => a.Current.ControlType == ControlType.Window));

            var parentWindow = walker.Normalize(element);

            action();

            AutomationElement newWindow = null;

            element.Wait(() =>
            {
                newWindow = walker.GetFirstChild(parentWindow);

                return newWindow != null;
            }, actionDescription, timeOut ?? WindowAfterTimeout);
            return newWindow;
        }

        public static void SelectByName(this List<AutomationElement> list, string toString, Func<string> containerDescription)
        {
            var filtered = list.Where(a => a.Current.Name == toString).ToList();

            if (filtered.Count == 1)
            {
                filtered.SingleEx().Pattern<SelectionItemPattern>().Select();
            }
            else
            {
                filtered = list.Where(a => a.Current.Name.Contains(toString, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (filtered.Count == 0)
                    throw new InvalidOperationException("No element found on {0} with ToString '{1}'. Found: \r\n{2}".Formato(containerDescription(), toString, list.ToString(a => a.Current.Name, "\r\n")));

                if (filtered.Count > 1)
                    throw new InvalidOperationException("Ambiguous elements found on {0} with ToString '{1}'. Found: \r\n{2}".Formato(containerDescription(), toString, filtered.ToString(a => a.Current.Name, "\r\n")));

                filtered.Single().Pattern<SelectionItemPattern>().Select();
            }
        }
    }
}
