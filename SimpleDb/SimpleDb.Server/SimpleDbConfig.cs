using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleDb.Server
{
    class SimpleDbConfig
    {
        private IConfigurationRoot configBuilder;
        private SimpleDbConfig()
        {           
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.json", optional: false, reloadOnChange: true);
            configBuilder = builder.Build();
        }

        /// <summary>
        /// 静态变量
        /// </summary>
        private static SimpleDbConfig instance = new SimpleDbConfig();

        public static SimpleDbConfig GetInstance()
        {
            return instance;
        }

        public string GetDbSetting()
        {
            var section =  configBuilder.GetSection("SimpleDbPath");
            if(section == null || string.IsNullOrEmpty(section.Value))
            {
                return "db001";
            }
            else
            {
                return section.Value;
            }
        }

    }
}
