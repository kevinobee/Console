﻿using System;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.PreprocessRequest;
using Sitecore.Text;
using Sitecore.Web;

namespace Cognifide.PowerShell.Core.Processors
{
    public class RewriteUrl : PreprocessRequestProcessor
    {
        public override void Process(PreprocessRequestArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            try
            {
                Assert.ArgumentNotNull(arguments.Context, "context");
                var url = arguments.Context.Request.Url;
                var localPath = url.LocalPath;
                if (localPath.StartsWith("/Console/", StringComparison.OrdinalIgnoreCase))
                {
                    // this bit is for compatibility of solutions integrating with SPE 2.x services in mind
                    WebUtil.RewriteUrl(
                        new UrlString
                        {
                            Path = localPath.ToLowerInvariant().Replace("/console/", "/sitecore modules/PowerShell/"),
                            Query = url.Query
                        }.ToString());
                }
                if (localPath.StartsWith("/-/script/v1", StringComparison.OrdinalIgnoreCase))
                {
                    var sourceArray = url.LocalPath.TrimStart('/').Split('/');
                    if (sourceArray.Length < 3)
                    {
                        return;
                    }
                    var length = sourceArray.Length - 3;
                    var destinationArray = new string[length];
                    Array.Copy(sourceArray, 3, destinationArray, 0, length);
                    var scriptPath = string.Format("/{0}", string.Join("/", destinationArray));
                    var query = url.Query.TrimStart('?');
                    query += string.Format("{0}script={1}&apiVersion=1", string.IsNullOrEmpty(query) ? "?" : "&",
                        scriptPath);
                    WebUtil.RewriteUrl(
                        new UrlString
                        {
                            Path = "/sitecore modules/PowerShell/Services/RemoteScriptCall.ashx",
                            Query = query
                        }.ToString());
                }
                if (localPath.StartsWith("/-/script/v2", StringComparison.OrdinalIgnoreCase))
                {
                    var sourceArray = url.LocalPath.TrimStart('/').Split('/');
                    if (sourceArray.Length < 4)
                    {
                        return;
                    }
                    var length = sourceArray.Length - 4;
                    var destinationArray = new string[length];
                    Array.Copy(sourceArray, 4, destinationArray, 0, length);
                    var scriptPath = string.Format("/{0}", string.Join("/", destinationArray));
                    var query = url.Query.TrimStart('?');
                    query += string.Format("{0}script={1}&sc_database={2}&scriptDb={2}&apiVersion=2",
                        string.IsNullOrEmpty(query) ? "" : "&", scriptPath, sourceArray[3]);
                    WebUtil.RewriteUrl(
                        new UrlString
                        {
                            Path = "/sitecore modules/PowerShell/Services/RemoteScriptCall.ashx",
                            Query = query
                        }.ToString());
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error during the SPE API call", exception);
            }
        }
    }
}