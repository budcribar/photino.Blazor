﻿using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PeakSWC.RemoteWebView;
using PhotinoNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photino.Blazor
{
    public class RemotePhotinoBlazorApp
    {
        private IServiceProvider _services;

        /// <summary>
        /// Gets configuration for the root components in the window.
        /// </summary>
        public BlazorWindowRootComponents RootComponents { get; private set; }

        internal void Initialize(IServiceProvider services, RootComponentList rootComponents)
        {
            _services = services;

            MainWindow = new RemotePhotinoWindow();
            MainWindow.SetTitle("Photino.Blazor App");
            MainWindow.SetUseOsDefaultLocation(false);
            MainWindow.SetWidth(1000);
            MainWindow.SetHeight(900);
            MainWindow.SetLeft(450);
            MainWindow.SetTop(100);

            MainWindow.RegisterCustomSchemeHandler(PhotinoWebViewManager.BlazorAppScheme, HandleWebRequest);

            // We assume the host page is always in the root of the content directory, because it's
            // unclear there's any other use case. We can add more options later if so.
            string hostPage = "index.html";
            var contentRootDir = Path.GetDirectoryName(Path.GetFullPath(hostPage))!;
            var hostPageRelativePath = Path.GetRelativePath(contentRootDir, hostPage);
            var fileProvider = new PhysicalFileProvider(contentRootDir);

            var dispatcher = new PhotinoDispatcher(MainWindow);
            var jsComponents = new JSComponentConfigurationStore();
            WindowManager = new RemotePhotinoWebViewManager(MainWindow, services, dispatcher, new Uri(PhotinoWebViewManager.AppBaseUri), fileProvider, jsComponents, hostPageRelativePath);
            RootComponents = new BlazorWindowRootComponents(WindowManager, jsComponents);
            foreach (var component in rootComponents)
            {
                RootComponents.Add(component.Item1, component.Item2);
            }
        }

        public RemotePhotinoWindow MainWindow { get; private set; }

        public RemotePhotinoWebViewManager WindowManager { get; private set; }

        public void Run()
        {
            WindowManager.Navigate("/");
            MainWindow.WaitForClose();
        }

        public Stream HandleWebRequest(object sender, string scheme, string url, out string contentType)
                => WindowManager.HandleWebRequest(sender, scheme, url, out contentType!)!;

    }
}