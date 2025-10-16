import React, { useEffect, useState } from "react";
import { Form, Input, Button, Select, message, Card } from "antd";

const { Option } = Select;

const PermissionDemo = () => {
    const [permissions, setPermissions] = useState([]);
    const [loading, setLoading] = useState(false);
    const [lastRequest, setLastRequest] = useState(null); // elküldött adatokhoz

    useEffect(() => {
        fetch("https://localhost:7179/api/Permissions")
            .then((res) => res.json())
            .then((data) => setPermissions(data))
            .catch(() => message.error("Nem sikerült betölteni a jogosultságokat!"));
    }, []);

    const onFinish = (values) => {
        setLoading(true);
        const requestBody = {
            username: values.username,
            roleName: values.permission,
            reason: values.reason || "",
        };
        // API hívás demohoz kikommentezhető:
        // fetch("https://localhost:7179/api/AccessRequests", { ... })
        setTimeout(() => {
            setLoading(false);
            message.success("Jogosultság igénylés elküldve!");
            setLastRequest(requestBody); // elmentjük demohoz
        }, 500);
    };

    return (
        <div style={{ maxWidth: 400, margin: "50px auto" }}>
            <h2>Jogosultság igénylés</h2>
            <Form layout="vertical" onFinish={onFinish}>
                <Form.Item label="Név" name="username" rules={[{ required: true, message: "Kérlek, add meg a neved!" }]}>
                    <Input placeholder="Add meg a neved" />
                </Form.Item>
                <Form.Item label="Jogosultság" name="permission" rules={[{ required: true, message: "Válassz jogosultságot!" }]}>
                    <Select placeholder="Válassz jogosultságot">
                        {permissions.map((perm) => (
                            <Option key={perm.id} value={perm.name}>
                                {perm.name} - {perm.description}
                            </Option>
                        ))}
                    </Select>
                </Form.Item>
                <Form.Item label="Indoklás" name="reason">
                    <Input.TextArea placeholder="Opcionális indoklás" />
                </Form.Item>
                <Form.Item>
                    <Button type="primary" htmlType="submit" loading={loading} block>
                        Igénylés elküldése
                    </Button>
                </Form.Item>
            </Form>

            {/* DEMO: Elküldött adatok megjelenítése */}
            {lastRequest && (
                <Card style={{ marginTop: 24 }} title="Legutóbbi igénylés adatai">
                    <p><b>Név:</b> {lastRequest.username}</p>
                    <p><b>Jogosultság:</b> {lastRequest.roleName}</p>
                    <p><b>Indoklás:</b> {lastRequest.reason}</p>
                </Card>
            )}
        </div>
    );
};

export default PermissionDemo;
