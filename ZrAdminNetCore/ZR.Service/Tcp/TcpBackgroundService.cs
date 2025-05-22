using Infrastructure;
using Infrastructure.Attribute;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Service.Social.IService;

namespace ZR.Service.Tcp
{
    public class TcpBackgroundService : BackgroundService
    {
        private readonly TcpServer _tcpServer;
        private readonly ILogger<TcpBackgroundService> _loggerTcpBackground;


        public TcpBackgroundService(TcpServer tcpServer, ILogger<TcpBackgroundService> logger)
        {
            _tcpServer = tcpServer;
            _loggerTcpBackground = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _loggerTcpBackground.LogInformation("TCP 后台服务正在启动...");
            if (App.OptionsSetting.TcpServer.Open)
            {
                await _tcpServer.StartAsync();
            }
            
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _loggerTcpBackground.LogInformation("TCP 后台服务正在停止...");
            _tcpServer.Stop();
            await base.StopAsync(cancellationToken);
        }
    }
}
