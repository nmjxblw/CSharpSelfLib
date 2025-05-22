using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure;

namespace ZR.Service.Tcp
{
    [AppService(ServiceType = typeof(TcpServer), ServiceLifetime = LifeTime.Singleton)]
    public class TcpServer : IDisposable
    {
        private readonly ILogger<TcpServer> _logger;
        private TcpListener _listener;
        private CancellationTokenSource _cts;

        public TcpServer(ILogger<TcpServer> logger)
        {
            _logger = logger;
            _listener = new TcpListener(IPAddress.Any, App.OptionsSetting.TcpServer.Port);
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _logger.LogInformation("TCP 服务端已启动，监听端口：{Port}", ((IPEndPoint)_listener.LocalEndpoint).Port);

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    _ = HandleClientAsync(client, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TCP 服务端已停止");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var buffer = new byte[1024];
                    _logger.LogInformation("客户端已连接：{Endpoint}", client.Client.RemoteEndPoint);

                    while (!token.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (bytesRead == 0) break;

                        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        _logger.LogInformation("收到消息：{Message}", message);



                        // 示例：回显消息
                        var response = Encoding.UTF8.GetBytes($"Server响应: {message}");
                        await stream.WriteAsync(response, 0, response.Length, token);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理客户端时发生异常");
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }

        public void Dispose()
        {
            Stop();
            _listener?.Dispose();
        }
    }

}
