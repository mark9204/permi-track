import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Card, Form, Input, Button, Row, Col, message, Layout, Typography } from 'antd';
import { authService } from '../services/authService';
import { useAuthStore } from '../stores/authStore';
import type { RegisterRequest } from '../types/auth.types';

const { Content } = Layout;
const { Text } = Typography;

const RegisterPage: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const setUser = useAuthStore((s) => s.setUser);

  const onFinish = async (values: any) => {
    const payload: RegisterRequest = {
      username: values.username,
      email: values.email,
      password: values.password,
      firstName: values.firstName,
      lastName: values.lastName,
    };

    setIsLoading(true);
    try {
      await authService.register(payload);
      message.success('Registration successful! Logging in...');
      
      // Auto-login after successful registration
      const loginResponse = await authService.login({
        username: values.username,
        password: values.password,
      });

      // Store tokens and user data
      localStorage.setItem('accessToken', loginResponse.accessToken);
      localStorage.setItem('refreshToken', loginResponse.refreshToken);

      const userToStore = { 
        ...loginResponse.user, 
        roles: loginResponse.roles ?? loginResponse.user.roles 
      };
      localStorage.setItem('user', JSON.stringify(userToStore));
      setUser(userToStore);

      message.success('Login successful!');
      navigate('/', { replace: true });
    } catch (err: any) {
      console.error('Registration/Login error', err);
      // Prefer server-provided message or full response data if available
      const serverData = err?.response?.data;
      const serverMsg = err?.message || serverData?.message || (serverData ? JSON.stringify(serverData) : undefined);
      message.error(serverMsg || 'Registration failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Layout style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
        <Card title="Create an Account" style={{ width: 520 }}>
          <Form layout="vertical" onFinish={onFinish}>
            <Form.Item name="username" label="Username" rules={[{ required: true, message: 'Please enter a username' }] }>
              <Input />
            </Form.Item>

            <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email', message: 'Please enter a valid email' }] }>
              <Input />
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item name="firstName" label="First Name" rules={[{ required: true, message: 'Please enter first name' }] }>
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="lastName" label="Last Name" rules={[{ required: true, message: 'Please enter last name' }] }>
                  <Input />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item name="password" label="Password" rules={[{ required: true, message: 'Please enter a password' }] }>
              <Input.Password />
            </Form.Item>

            <Form.Item
              name="confirmPassword"
              label="Confirm Password"
              dependencies={["password"]}
              rules={[
                { required: true, message: 'Please confirm your password' },
                ({ getFieldValue }) => ({
                  validator(_: any, value: string) {
                    if (!value || getFieldValue('password') === value) {
                      return Promise.resolve();
                    }
                    return Promise.reject(new Error('The two passwords do not match'));
                  },
                }),
              ]}
            >
              <Input.Password />
            </Form.Item>

            <Form.Item>
              <Button type="primary" htmlType="submit" block loading={isLoading}>
                Create Account
              </Button>
            </Form.Item>

            <div style={{ textAlign: 'center' }}>
              <Text>
                Already have an account? <Link to="/login">Login</Link>
              </Text>
            </div>
          </Form>
        </Card>
      </Content>
    </Layout>
  );
};

export default RegisterPage;
