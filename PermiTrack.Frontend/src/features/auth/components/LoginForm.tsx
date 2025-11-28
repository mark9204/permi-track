import { Form, Input, Button, Checkbox } from 'antd';
import type { LoginRequest } from '../../../types/auth.types';

interface LoginFormProps {
  isLoading: boolean;
  onSubmit: (values: LoginRequest) => void;
}

const LoginForm = ({ isLoading, onSubmit }: LoginFormProps) => {
  return (
    <Form
      name="login"
      initialValues={{ remember: true }}
      onFinish={onSubmit}
      autoComplete="off"
      layout="vertical"
    >
      <Form.Item
        label="Username"
        name="username"
        rules={[{ required: true, message: 'Please input your username!' }]}
      >
        <Input />
      </Form.Item>

      <Form.Item
        label="Password"
        name="password"
        rules={[{ required: true, message: 'Please input your password!' }]}
      >
        <Input.Password />
      </Form.Item>

      <Form.Item name="remember" valuePropName="checked">
        <Checkbox>Remember me</Checkbox>
      </Form.Item>

      <Form.Item>
        <Button type="primary" htmlType="submit" loading={isLoading} block>
          Log in
        </Button>
      </Form.Item>
    </Form>
  );
};

export default LoginForm;
