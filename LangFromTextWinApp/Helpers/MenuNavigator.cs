using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LangFromTextWinApp.Helpers
{
    public class MenuNavigator
    {
        private Menu menu;
        ContentControl mainContent;
        List<UserControl> viewContainer = new List<UserControl> { new LangSettingsView(), new LangPhraseCheckView(), new LangStartPageView(), new LangDataView(), new LangAboutView() };
        private Random rnd = new Random();

        public MenuNavigator(Menu menu, ContentControl mainContent)
        {
            this.menu = menu;
            this.mainContent = mainContent;
        }

        public void ShowStartPage()
        {
            ShowPageByMenuTag(FindByTag(menu.Items, nameof(LangStartPageView)));
        }

        public void ShowDataPage()
        {
            ShowPageByMenuTag(FindByTag(menu.Items, nameof(LangDataView)));
        }

        public void ShowPhraseCheckPage()
        {
            ShowPageByMenuTag(FindByTag(menu.Items, nameof(LangPhraseCheckView)));
        }

        public void ShowSettingsPage()
        {
            ShowPageByMenuTag(FindByTag(menu.Items, nameof(LangSettingsView)));
        }

        public MenuItem FindByTag(ItemCollection menuItems, string tag)
        {
            foreach (MenuItem menuItem in menuItems)
            {
                if (menuItem.Tag?.ToString() == tag)
                    return menuItem;

                MenuItem subRes = FindByTag(menuItem.Items, tag);

                if (subRes != null)
                    return subRes;
            }

            return null;
        }

        public void ShowPageByViewName(string viewName)
        {
            ShowPageByMenuTag(FindByTag(menu.Items, viewName));
        }

        public void ShowPageByMenuTag(MenuItem selectedMenuItem)
        {
            if (selectedMenuItem?.Tag != null)
            {
                if (selectedMenuItem.Tag.ToString() == mainContent.Content?.GetType().Name)
                    return;

                MarkSelectedMenuItem(selectedMenuItem);

                try
                {
                    ShowControl(viewContainer.First(v => v.GetType().Name == selectedMenuItem.Tag.ToString()));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to show module by '{selectedMenuItem.Tag}' TAG. Please check TAG property on MenuItem in MainWindow.xaml.", ex);
                }
            }
        }

        public void ShowRandomLangModule()
        {
            LTModules.ILTModuleView langModuleControl = FEContext.LTModules[rnd.Next(FEContext.LTModules.Count)];
            ShowControl(langModuleControl);
        }

        public void ShowControl(object control)
        {
            mainContent.Content = null;
            mainContent.Content = control;
        }

        public void MarkSelectedMenuItem(MenuItem selectedItem)
        {
            if (selectedItem != null)
            {
                foreach (MenuItem item in menu.Items)
                {
                    item.FontWeight = FontWeights.Normal;

                    foreach (MenuItem subItem in item.Items)
                        subItem.FontWeight = FontWeights.Normal;
                }

                selectedItem.FontWeight = FontWeights.Bold;
                // selectedItem.CommandBindings[0].
            }
        }
    }
}
