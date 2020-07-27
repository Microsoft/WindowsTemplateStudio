﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WindowsTestHelpers;
using Microsoft.Templates.Core;
using OpenQA.Selenium.Appium.Windows;
using Xunit;

namespace Microsoft.Templates.Test
{
    [Collection("GenerationCollection")]
    public class AccessibilityTests : VisualComparisonTests
    {
        public AccessibilityTests(GenerationFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        [Trait("ExecutionSet", "ManualOnly")]
        [Trait("Type", "WinAppDriver")]
        public async Task RunBasicAccessibilityChecksAgainstEachPageAsync()
        {
            // This test does not run against all combinations as other tests ensure output is the same for each combination.
            // In theory this means that there should be no need to repeat tests as they would find the same things.
            // As this is a slow test to run we do not want to increase overall execution time unnecessarily.

            ExecutionEnvironment.CheckRunningAsAdmin();
            WinAppDriverHelper.CheckIsInstalled();
            WinAppDriverHelper.StartIfNotRunning();

            var pagesFoundWithIssues = 0;

            var rootFolder = $"{Path.GetPathRoot(Environment.CurrentDirectory)}UIT\\ALLY\\{DateTime.Now:dd_HHmmss}\\";
            var reportFolder = Path.Combine(rootFolder, "Reports");

            var pageIdentities = AllTestablePages("CodeBehind");

            VisualComparisonTestDetails appDetails = null;

            async Task ForOpenedPage(string pageName, WindowsDriver<WindowsElement> session, string projectName)
            {
                if (pageName == "Map")
                {
                    // For location permission
                    if (await ClickYesOnPopUpAsync(session))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2)); // Allow page to load after accepting prompt
                    }
                }
                else if (pageName == "Camera")
                {
                    var cameraPermission = await ClickYesOnPopUpAsync(session); // For camera permission
                    var microphonePermission = await ClickYesOnPopUpAsync(session); // For microphone permission

                    if (cameraPermission && microphonePermission)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2)); // Allow page to load after accepting prompts
                    }
                }

                var processes = Process.GetProcessesByName(projectName);

                var config = Axe.Windows.Automation.Config.Builder.ForProcessId(processes[0].Id);

                config.WithOutputFileFormat(Axe.Windows.Automation.OutputFileFormat.A11yTest);
                config.WithOutputDirectory(reportFolder);

                var scanner = Axe.Windows.Automation.ScannerFactory.CreateScanner(config.Build());

                try
                {
                    var scanResults = scanner.Scan();

                    if (scanResults.ErrorCount > 0)
                    {
                        pagesFoundWithIssues++;
                    }
                }
                catch (Axe.Windows.Automation.AxeWindowsAutomationException exc)
                {
                    Assert.True(exc == null, exc.ToString());
                }
            }

            try
            {
                appDetails = await SetUpProjectForUiTestComparisonAsync(ProgrammingLanguages.CSharp, "SplitView", "CodeBehind", pageIdentities);

                using (var appSession = WinAppDriverHelper.LaunchAppx(appDetails.PackageFamilyName))
                {
                    appSession.Manage().Window.Maximize();

                    await Task.Delay(TimeSpan.FromSeconds(2));

                    var menuItems = appSession.FindElementsByClassName("Microsoft.UI.Xaml.Controls.NavigationViewItem");

                    foreach (var menuItem in menuItems)
                    {
                        menuItem.Click();

                        await Task.Delay(TimeSpan.FromMilliseconds(1500)); // Allow page to load and animations to complete

                        await ForOpenedPage(menuItem.Text, appSession, appDetails.ProjectName);
                    }
                }

                // Don't leave the app maximized in case we want to open the app again.
                // Some controls handle layout differently when the app is first opened maximized
                VirtualKeyboard.RestoreMaximizedWindow();
            }
            finally
            {
                if (appDetails != null)
                {
                    UninstallAppx(appDetails.PackageFullName);
                    RemoveCertificate(appDetails.CertificatePath);
                }

                WinAppDriverHelper.StopIfRunning();
            }

            Assert.True(0 == pagesFoundWithIssues, $"Accessibility issues found. For details, see reports in '{reportFolder}'.");
        }
    }
}