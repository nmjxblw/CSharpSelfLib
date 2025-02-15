const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const app = express();

app.use(cors());
app.use(bodyParser.json());

// �ͻ���״̬�洢
const clients = new Map(); // key: clientIP, value: { lastHeartbeat: Date, status: 'online'|'offline'|'error' }

// ���տ���������Թ�����ҳ��
app.post('/api/control', (req, res) => {
    const { targetIP, command } = req.body;
    // ����ʵ��Ӧͨ�����緢�͵��ͻ��ˣ�ʾ��ֱ��ģ��
    console.log(`[Command] Send To ${targetIP}: ${command}`);
    res.json({ success: true });
});

// ���������������Կͻ��ˣ�
app.post('/api/heartbeat', (req, res) => {
    const { message, flag, sender } = req.body;
    const clientIP = sender;

    // ����״̬
    clients.set(clientIP, {
        lastHeartbeat: new Date(),
        status: flag.error === 'none' ? 'online' : 'error'
    });

    res.json({ received: true });
});

// ��ȡ���пͻ���״̬������ҳ��ʾ��
app.get('/api/status', (req, res) => {
    const now = new Date();
    // ��鳬ʱ
    clients.forEach((client, ip) => {
        if ((now - client.lastHeartbeat) > 5000) {
            client.status = 'offline';
        }
    });
    res.json(Array.from(clients.entries()));
});

// ��ҳ����
app.get('/', (req, res) => res.sendFile(__dirname + '/index.html'));

// ����������
const PORT = 10001;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));