﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Resources;

namespace Microsoft.Templates.Core.PostActions.Catalog
{
    public class CreateSyncStepsInstructionsPostAction : PostAction<TempGenerationResult>
    {
        public CreateSyncStepsInstructionsPostAction(TempGenerationResult config) : base(config)
        {
        }

        public override void Execute()
        {
            var fileName = Path.Combine(GenContext.Current.OutputPath, StringRes.SyncInstructionsFileName);

            var sb = new StringBuilder();

            sb.AppendLine(StringRes.MarkdownHeader);
            sb.AppendLine();
            sb.AppendLine(StringRes.SyncInstructionsHeader);
            sb.AppendLine(StringRes.SyncInstructionsDescription);
            sb.AppendLine();
            sb.AppendLine(string.Format(StringRes.SyncInstructionsTempFolder, GenContext.Current.OutputPath));
            sb.AppendLine();

            if (_config.NewFiles.Any())
            {
                sb.AppendLine(StringRes.SyncInstructionsNewFiles);
                sb.AppendLine(StringRes.SyncInstructionsNewFilesDescription);
                foreach (var newFile in _config.NewFiles)
                {
                    sb.AppendLine(GetLinkToLocalFile(newFile));
                }
                sb.AppendLine();
            }

            if (GenContext.Current.MergeFilesFromProject.Any())
            {
                sb.AppendLine(StringRes.SyncInstructionsModifiedFiles);
                sb.AppendLine(StringRes.SyncInstructionsModifiedFilesDescription);
                sb.AppendLine();

                foreach (var mergeFile in GenContext.Current.MergeFilesFromProject)
                {
                    sb.AppendLine(string.Format(StringRes.SyncInstructionsMergeFile, mergeFile.Key));
                    foreach (var mergeInfo in mergeFile.Value)
                    {
                        sb.AppendLine($"```{mergeInfo.Format}");
                        sb.AppendLine(mergeInfo.PostActionCode);
                        sb.AppendLine("```");

                        sb.AppendLine();
                    }

                    if (!GenContext.Current.FailedMergePostActions.Any(w => w.FileName == mergeFile.Key))
                    {
                        sb.AppendLine(string.Format(StringRes.SyncInstructionsMergeFilePreview, mergeFile.Key));
                        sb.AppendLine();
                    }
                    else
                    {
                        var failedMergePostActions = GenContext.Current.FailedMergePostActions.Where(w => w.FileName == mergeFile.Key);

                        sb.AppendLine(StringRes.SyncInstructionsMergeFileError);
                        foreach (var failedMergePostAction in failedMergePostActions)
                        {
                            sb.AppendLine($"* {failedMergePostAction.Description}");
                            sb.AppendLine();
                        }
                    }
                }
            }

            if (_config.ConflictingFiles.Any())
            {
                sb.AppendLine(StringRes.SyncInstructionsConflictingFiles);
                sb.AppendLine();
                sb.AppendLine(StringRes.SyncInstructionsConflictingFilesDescription);
                sb.AppendLine();

                foreach (var conflictFile in _config.ConflictingFiles)
                {
                    sb.AppendLine(GetLinkToLocalFile(conflictFile));
                }
                sb.AppendLine();
            }

            if (_config.UnchangedFiles.Any())
            {
                sb.AppendLine(StringRes.SyncInstructionsUnchangedFiles);
                sb.AppendLine(StringRes.SyncInstructionsUnchangedFilesDescription);
                foreach (var unchangedFile in _config.UnchangedFiles)
                {
                    sb.AppendLine(GetLinkToLocalFile(unchangedFile));
                }
            }

            File.WriteAllText(fileName, sb.ToString());

            GenContext.Current.FilesToOpen.Add(fileName);

            GenContext.Current.FailedMergePostActions.Clear();
            GenContext.Current.MergeFilesFromProject.Clear();
        }

        private static string GetLinkToLocalFile(string fileName)
        {
            return $"* [{fileName}]({fileName})";
        }
    }
}
