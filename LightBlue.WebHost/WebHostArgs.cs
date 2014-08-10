﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using LightBlue.Infrastructure;

using NDesk.Options;

namespace LightBlue.WebHost
{
    public class WebHostArgs
    {
        public string SiteDirectory { get; private set; }
        public int Port { get; private set; }
        public string RoleName { get; private set; }
        public string ConfigurationPath { get; private set; }
        public RetryMode RetryMode { get; private set; }

        public static WebHostArgs ParseArgs(IEnumerable<string> args)
        {
            string siteDirectory = null;
            var port = 0;
            string roleName = null;
            string configurationPath = null;
            var retryMode = RetryMode.Infinite;

            var options = new OptionSet
            {
                {"d|siteDirectory=", "", v => siteDirectory = v},
                {"p|port=", "", v => Int32.TryParse(v, NumberStyles.None, CultureInfo.InvariantCulture, out port)},
                {"n|roleName=", "", v => roleName = v},
                {"c|configurationPath=", "", v => configurationPath = v},
                {"r|retryMode=", "", v => { retryMode = (RetryMode) Enum.Parse(typeof(RetryMode), v, true); }},
            };

            options.Parse(args);

            if (string.IsNullOrWhiteSpace(siteDirectory))
            {
                throw new ArgumentException("Host requires a site directory to run.");
            }
            if (port == 0)
            {
                throw new ArgumentException("Host requires a port to run the site on");
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role Name must be specified.");
            }
            if (string.IsNullOrWhiteSpace(configurationPath))
            {
                throw new ArgumentException("Configuration path must be specified.");
            }


            if (!Directory.Exists(siteDirectory))
            {
                throw new FileNotFoundException("The specified site directory does not exist.");
            }

            var configurationFile = LocateConfigurationFile(configurationPath);

            return new WebHostArgs
            {
                SiteDirectory = siteDirectory,
                Port = port,
                RoleName = roleName,
                ConfigurationPath = configurationFile,
                RetryMode = retryMode
            };
        }

        private static string LocateConfigurationFile(string configurationPath)
        {
            if (File.Exists(configurationPath))
            {
                return configurationPath;
            }

            if (!Directory.Exists(configurationPath))
            {
                throw new ArgumentException("The configuration path does not exist. Specify the specific configuration file or the directory in which ServiceConfiguration.Local.cscfg is located.");
            }

            var configurationFile = Path.Combine(configurationPath, "ServiceConfiguration.Local.cscfg");
            if (!File.Exists(configurationFile))
            {
                throw new ArgumentException("ServiceConfiguration.Local.cscfg cannot be located in the configuration path.");
            }

            return configurationFile;
        }
    }
}