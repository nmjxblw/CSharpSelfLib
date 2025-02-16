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
	console.log(`${res.ToString()}`);
});

// ���������������Կͻ��ˣ�
app.post('/api/heartbeat', (req, res) => {
	const { message, flag } = req.body;
	const clientIP = flag.sender;
	console.log(message);
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
app.get('/api/self', (req, res) => {

	res.json({
		message: `[${new Date()}]heartbeat test web`,
		flag: {
			error: "None",
			sender: {
				ip: "127.0.0.1",
				port: 11011,
			}
		}
	});
});

// ����������
const PORT = 11011;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));