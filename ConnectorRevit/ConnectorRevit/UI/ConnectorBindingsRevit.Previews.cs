﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.ViewModels;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DesktopUI2.Models.Filters;
using DesktopUI2.Models.Settings;
using Speckle.Core.Transports;
using Speckle.Newtonsoft.Json;
using static DesktopUI2.ViewModels.MappingViewModel;
using ApplicationObject = Speckle.Core.Models.ApplicationObject;
using Avalonia.Threading;
using Autodesk.Revit.DB.DirectContext3D;
using Revit.Async;
using DynamicData;

namespace Speckle.ConnectorRevit.UI
{
  public partial class ConnectorBindingsRevit
  {
    public override bool CanPreviewReceive => true;
    private string SelectedReceiveCommit { get; set; }
    List<IDirectContext3DServer> m_servers = new List<IDirectContext3DServer>();

    public override async Task<StreamState> PreviewReceive(StreamState state, ProgressViewModel progress)
    {
      // first check if commit is the same and preview objects have already been generated
      Commit commit = await GetCommitFromState(state, progress);
      progress.Report = new ProgressReport();

      if (commit.id != SelectedReceiveCommit)
      {
        // check for converter 
        var converter = KitManager.GetDefaultKit().LoadConverter(ConnectorRevitUtils.RevitAppName);
        if (converter == null)
        {
          progress.Report.LogOperationError(new SpeckleException("Could not find any Kit!"));
          return null;
        }
        converter.SetContextDocument(CurrentDoc.Document);

        var settings = new Dictionary<string, string>();
        CurrentSettings = state.Settings;
        foreach (var setting in state.Settings)
          settings.Add(setting.Slug, setting.Selection);

        settings["preview"] = "true";
        converter.SetConverterSettings(settings);

        var commitObject = await GetCommit(commit, state, progress);
        if (commitObject == null)
        {
          progress.Report.LogOperationError(new Exception($"Could not retrieve commit {commit.id} from server"));
          progress.CancellationTokenSource.Cancel();
        }

        Preview.Clear();
        StoredObjects.Clear();

        Preview = FlattenCommitObject(commitObject, converter);
        foreach (var previewObj in Preview)
          progress.Report.Log(previewObj);

        List<ApplicationObject> applicationObjects = null;
        await RevitTask.RunAsync(app =>
        {
          using (var t = new Transaction(CurrentDoc.Document, $"Baking stream {state.StreamId}"))
          {
            t.Start();
            applicationObjects = ConvertReceivedObjects(converter, progress);
            t.Commit();
          }

          AddMultipleRevitElementServers(applicationObjects);
        });
      }
      else // just generate the log
      {
        foreach (var previewObj in Preview)
          progress.Report.Log(previewObj);
      }
      return null;
    }

    public override void ResetDocument()
    {
      UnregisterServers();
    }

    #region move these?
    // gets the state commit
    private async Task<Commit> GetCommitFromState(StreamState state, ProgressViewModel progress)
    {
      Commit commit = null;
      if (state.CommitId == "latest") //if "latest", always make sure we get the latest commit
      {
        var res = await state.Client.BranchGet(progress.CancellationTokenSource.Token, state.StreamId, state.BranchName, 1);
        commit = res.commits.items.FirstOrDefault();
      }
      else
      {
        var res = await state.Client.CommitGet(progress.CancellationTokenSource.Token, state.StreamId, state.CommitId);
        commit = res;
      }
      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
        return null;
      return commit;
    }
    private async Task<Base> GetCommit(Commit commit, StreamState state, ProgressViewModel progress)
    {
      var transport = new ServerTransport(state.Client.Account, state.StreamId);

      var commitObject = await Operations.Receive(
        commit.referencedObject,
        progress.CancellationTokenSource.Token,
        transport,
        onProgressAction: dict => progress.Update(dict),
        onErrorAction: (s, e) =>
        {
          progress.Report.LogOperationError(e);
          progress.CancellationTokenSource.Cancel();
        },
        onTotalChildrenCountKnown: (c) => progress.Max = c,
        disposeTransports: true
        );

      if (progress.Report.OperationErrorsCount != 0)
        return null;

      return commitObject;
    }
    #endregion
    public void AddMultipleRevitElementServers(List<ApplicationObject> applicationObjects)
    {
      ExternalService directContext3DService = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
      MultiServerService msDirectContext3DService = directContext3DService as MultiServerService;
      IList<Guid> serverIds = msDirectContext3DService.GetActiveServerIds();

      foreach (var appObj in applicationObjects)
      {
        if (!(appObj.Converted.FirstOrDefault() is IDirectContext3DServer server))
          continue;

        directContext3DService.AddServer(server);
        m_servers.Add(server);
        serverIds.Add(server.GetServerId());
        //RefreshView();
      }

      msDirectContext3DService.SetActiveServers(serverIds);

      //m_documents.Add(uidoc.Document);
      CurrentDoc.UpdateAllOpenViews();
    }

    public void UnregisterServers()
    {
      ExternalServiceId externalDrawerServiceId = ExternalServices.BuiltInExternalServices.DirectContext3DService;
      var externalDrawerService = ExternalServiceRegistry.GetService(externalDrawerServiceId) as MultiServerService;
      if (externalDrawerService == null)
        return;

      foreach (var registeredServerId in externalDrawerService.GetRegisteredServerIds())
      {
        var externalDrawServer = externalDrawerService.GetServer(registeredServerId) as IDirectContext3DServer;
        if (externalDrawServer == null)
          continue;
        //if (document != null && !document.Equals(externalDrawServer.Document))
        //  continue;
        externalDrawerService.RemoveServer(registeredServerId);
      }

      m_servers.Clear();
      CurrentDoc.UpdateAllOpenViews();
    }

    public override bool CanPreviewSend => true;

    public override void PreviewSend(StreamState state, ProgressViewModel progress)
    {
      var filterObjs = GetSelectionFilterObjects(state.Filter);
      foreach (var filterObj in filterObjs)
      {
        var converter = (ISpeckleConverter)Activator.CreateInstance(Converter.GetType());
        var descriptor = ConnectorRevitUtils.ObjectDescriptor(filterObj);
        var reportObj = new ApplicationObject(filterObj.UniqueId, descriptor);
        if (!converter.CanConvertToSpeckle(filterObj))
          reportObj.Update(status: ApplicationObject.State.Skipped, logItem: $"Sending this object type is not supported in Revit");
        else
          reportObj.Update(status: ApplicationObject.State.Created);
        progress.Report.Log(reportObj);
      }
      SelectClientObjects(filterObjs.Select(o => o.UniqueId).ToList());
    }
  }
}