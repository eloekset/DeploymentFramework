// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.BizTalk.ExplorerOM;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace DeploymentFramework.BuildTasks
{
    public class ControlBizTalkApp : Task
    {
        private string _applicationName;
        private string _startOption;
        private string _stopOption;
        private BtsCatalogExplorer _catalog = null;

        public ControlBizTalkApp()
        {
            // connect to the BizTalk configuration database that corresponds to our group membership.
            _catalog = new BtsCatalogExplorer();
            _catalog.ConnectionString = string.Format("Server={0};Initial Catalog={1};Integrated Security=SSPI;",
               BizTalkGroupInfo.GroupDBServerName,
               BizTalkGroupInfo.GroupMgmtDBName);
        }

        /// <summary>
        /// A member of ApplicationStartOption enumeration
        /// </summary>
        public string StartOption
        {
            get { return _startOption; }
            set { _startOption = value; }
        }

        /// <summary>
        /// A member of ApplicationStopOption enumeration
        /// </summary>
        public string StopOption
        {
            get { return _stopOption; }
            set { _stopOption = value; }
        }

        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool Execute()
        {
            int retryCount = 5;

            if ((string.IsNullOrEmpty(_startOption) && string.IsNullOrEmpty(_stopOption))
                || (!string.IsNullOrEmpty(_startOption) && !string.IsNullOrEmpty(_stopOption)))
            {
                this.Log.LogError("Please specify either StartOption or StopOption.");
                return false;
            }

            Application application = _catalog.Applications[_applicationName];
            if (application == null)
            {
                this.Log.LogError("Unable to find application '{0}' in catalog.", _applicationName);
                return false;
            }

            ApplicationStartOption startOption = 0;
            ApplicationStopOption stopOption = 0;

            if (!string.IsNullOrEmpty(_startOption))
            {
                startOption = ParseStartEnum(_startOption);
            }
            else
            {
                stopOption = ParseStopEnum(_stopOption);
            }

            for (int i = 0; i < retryCount; i++)
            {
                this.Log.LogMessage("(Retry count {0})", i);
                try
                {
                    if (startOption != 0)
                    {
                        this.Log.LogMessage("Starting {0} application...", _applicationName);
                        application.Start(startOption);
                    }
                    else
                    {
                        this.Log.LogMessage("Stopping {0} application...", _applicationName);
                        application.Stop(stopOption);
                    }

                    _catalog.SaveChanges();
                    break;
                }
                catch (Microsoft.BizTalk.ExplorerOM.BtsException ex)
                {
                    try
                    {
                        _catalog.DiscardChanges();
                    }
                    catch { }

                    if (!ex.Message.Contains("deadlocked"))
                    {
                        this.Log.LogErrorFromException(ex, false);
                        return false;
                    }
                }
            }

            return true;
        }

        private ApplicationStartOption ParseStartEnum(string enumComponents)
        {
            ApplicationStartOption result = 0;

            string[] enumComponentsSplit = enumComponents.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string enumComponent in enumComponentsSplit)
            {
                result |= (ApplicationStartOption)Enum.Parse(typeof(ApplicationStartOption), enumComponent);
            }

            return result;
        }

        private ApplicationStopOption ParseStopEnum(string enumComponents)
        {
            ApplicationStopOption result = 0;

            string[] enumComponentsSplit = enumComponents.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string enumComponent in enumComponentsSplit)
            {
                result |= (ApplicationStopOption)Enum.Parse(typeof(ApplicationStopOption), enumComponent);
            }

            return result;
        }
    }
}