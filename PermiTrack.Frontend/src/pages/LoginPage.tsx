import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, Layout, message, Form, Input, Button, Typography } from 'antd';
import { LockOutlined } from '@ant-design/icons';
import LoginForm from '../features/auth/components/LoginForm';
import { authService } from '../services/authService';
import type { LoginRequest, LoginResponse } from '../types/auth.types';
import type { ApiError } from '../types/api.types';
import { useAuthStore } from '../stores/authStore';

const { Content } = Layout;
const { Text, Title } = Typography;

const LoginPage = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [step, setStep] = useState<'credentials' | 'mfa'>('credentials');
  const [pendingResponse, setPendingResponse] = useState<LoginResponse | null>(null);
  
  const navigate = useNavigate();
  const setUser = useAuthStore((s) => s.setUser);

  const handleLogin = async (values: LoginRequest) => {
    setIsLoading(true);
    try {
      const response = await authService.login(values);
      setPendingResponse(response);
      setStep('mfa');
      message.info('Please enter the verification code sent to your device.');
    } catch (error) {
      const apiError = error as ApiError;
      message.error(apiError.message || 'An unknown error occurred.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleMfaSubmit = async (values: { code: string }) => {
    setIsLoading(true);
    try {
      const isValid = await authService.verify2FA(values.code);
      if (!isValid) {
        message.error('Invalid code. Try 123456');
        setIsLoading(false);
        return;
      }

      if (pendingResponse) {
        localStorage.setItem('accessToken', pendingResponse.accessToken);
        localStorage.setItem('refreshToken', pendingResponse.refreshToken);
        const userToStore = { ...pendingResponse.user, roles: pendingResponse.roles ?? pendingResponse.user.roles };
        localStorage.setItem('user', JSON.stringify(userToStore));
        setUser(userToStore);
        message.success('Login successful!');
        navigate('/', { replace: true });
      }
    } catch (error) {
      message.error('Verification failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Layout style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
        <Card style={{ width: 400, textAlign: 'center' }}>
          <div style={{ marginBottom: 24 }}>
            <img src="/vite.svg" alt="Logo" style={{ height: 40, marginBottom: 16 }} />
            <Title level={3}>PermiTrack</Title>
            <Text type="secondary">Secure Access Management</Text>
          </div>

          {step === 'credentials' ? (
            <>
              <LoginForm isLoading={isLoading} onSubmit={handleLogin} />
              <div style={{ marginTop: 12, textAlign: 'center' }}>
                <a href="/register">Don't have an account? Register</a>
              </div>
            </>
          ) : (
            <Form onFinish={handleMfaSubmit} layout="vertical">
              <div style={{ marginBottom: 24 }}>
                <LockOutlined style={{ fontSize: 48, color: '#1890ff' }} />
                <Title level={4} style={{ marginTop: 16 }}>Two-Factor Authentication</Title>
                <Text type="secondary">Enter the 6-digit code from your authenticator app.</Text>
              </div>
              
              <Form.Item
                name="code"
                rules={[
                  { required: true, message: 'Please enter the code' },
                  { len: 6, message: 'Code must be 6 digits' }
                ]}
              >
                <Input 
                  placeholder="123456" 
                  style={{ textAlign: 'center', fontSize: '24px', letterSpacing: '8px' }} 
                  maxLength={6}
                />
              </Form.Item>

              <Button type="primary" htmlType="submit" loading={isLoading} block size="large">
                Verify
              </Button>
              
              <Button type="link" onClick={() => setStep('credentials')} style={{ marginTop: 16 }}>
                Back to Login
              </Button>
            </Form>
          )}
        </Card>
      </Content>
    </Layout>
  );
};

export default LoginPage;
