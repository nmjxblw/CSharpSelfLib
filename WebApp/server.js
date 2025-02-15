const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const app = express();

app.use(cors());
app.use(bodyParser.json());

// 客户端状态存储
const clients = new Map(); // key: clientIP, value: { lastHeartbeat: Date, status: 'online'|'offline'|'error' }

// 接收控制命令（来自管理网页）
app.post('/api/control', (req, res) => {
    const { targetIP, command } = req.body;
    // 这里实际应通过网络发送到客户端，示例直接模拟
    console.log(`[Command] Send To ${targetIP}: ${command}`);
    res.json({ success: true });
});

// 接收心跳包（来自客户端）
app.post('/api/heartbeat', (req, res) => {
    const { message, flag, sender } = req.body;
    const clientIP = sender;

    // 更新状态
    clients.set(clientIP, {
        lastHeartbeat: new Date(),
        status: flag.error === 'none' ? 'online' : 'error'
    });

    res.json({ received: true });
});

// 获取所有客户端状态（供网页显示）
app.get('/api/status', (req, res) => {
    const now = new Date();
    // 检查超时
    clients.forEach((client, ip) => {
        if ((now - client.lastHeartbeat) > 5000) {
            client.status = 'offline';
        }
    });
    res.json(Array.from(clients.entries()));
});

// 网页界面
app.get('/', (req, res) => res.sendFile(__dirname + '/index.html'));

// 启动服务器
const PORT = 10001;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));