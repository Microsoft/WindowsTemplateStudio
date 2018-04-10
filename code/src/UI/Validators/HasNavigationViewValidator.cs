﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Validation;
using Microsoft.Templates.UI.Services;

namespace Microsoft.Templates.UI.Validators
{
    public class HasNavigationViewValidator : IBreakingChangeValidator
    {
        // This is last version with Hamburguer menu control in templates
        public Version BreakingVersion { get; } = new Version("2.0.18093.1");

        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            var projectType = ProjectMetadataService.GetProjectMetadata().ProjectType;

            if (projectType == "SplitView" && !HasNavigationView())
            {
                var message = string.Format(
                    Resources.StringRes.ValidatorHasNavigationViewMessage,
                    Core.Configuration.Current.GitHubDocsUrl);

                result.IsValid = false;
                result.ErrorMessages.Add(message);
            }

            return result;
        }

        private bool HasNavigationView()
        {
            var file = Path.Combine(GenContext.Current.ProjectPath, "Views", "ShellPage.xaml");
            var fileContent = GetFileContent(file);
            return fileContent.Contains("<NavigationView");
        }

        private string GetFileContent(string file)
        {
            if (!File.Exists(file))
            {
                return string.Empty;
            }

            try
            {
                return File.ReadAllText(file);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
