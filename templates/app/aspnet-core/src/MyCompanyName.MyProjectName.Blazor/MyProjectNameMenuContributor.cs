﻿using System.Threading.Tasks;
using MyCompanyName.MyProjectName.Localization;
using Volo.Abp.UI.Navigation;

namespace MyCompanyName.MyProjectName.Blazor
{
    public class MyProjectNameMenuContributor : IMenuContributor
    {
        public Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            var l = context.GetLocalizer<MyProjectNameResource>();

            context.Menu.Items.Insert(
                0,
                new ApplicationMenuItem(
                    "MyProjectName.Home",
                    l["Menu:Home"],
                    "/",
                    icon: "fas fa-home"
                )
            );

            return Task.CompletedTask;
        }
    }
}
