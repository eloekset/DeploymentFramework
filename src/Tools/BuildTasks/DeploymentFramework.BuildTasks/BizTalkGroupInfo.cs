// Deployment Framework for BizTalk 5.0
// Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;

using Microsoft.Win32;

namespace DeploymentFramework.BuildTasks
{
	/// <summary>
	/// Summary description for BizTalkGroupInfo.
	/// </summary>
	public class BizTalkGroupInfo
	{
      public static string GroupDBServerName
      {
         get
         {
            using(RegistryKey rk = Registry.LocalMachine)
            {
               using(RegistryKey rk2 = rk.OpenSubKey(@"SOFTWARE\Microsoft\BizTalk Server\3.0\Administration"))
               {
                  string groupServerName = (string)rk2.GetValue("MgmtDBServer");
                  return groupServerName;
               }
            }
         }
      }

      public static string GroupMgmtDBName
      {
         get
         {
            using(RegistryKey rk = Registry.LocalMachine)
            {
               using(RegistryKey rk2 = rk.OpenSubKey(@"SOFTWARE\Microsoft\BizTalk Server\3.0\Administration"))
               {
                  string db = (string)rk2.GetValue("MgmtDBName");
                  return db;
               }
            }
         }
      }
	}
}