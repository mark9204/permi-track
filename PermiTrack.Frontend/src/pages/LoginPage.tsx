import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, Layout, message } from 'antd';
import LoginForm from '../features/auth/components/LoginForm';
import { authService } from '../services/authService';
import type { LoginRequest } from '../types/auth.types';
import type { ApiError } from '../types/api.types';
import { useAuthStore } from '../stores/authStore';

const { Content } = Layout;

const LoginPage = () => {
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const setUser = useAuthStore((s) => s.setUser);

  const handleLogin = async (values: LoginRequest) => {
    setIsLoading(true);
    try {
      const response = await authService.login(values);
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);
      // Ensure roles returned separately are included on the stored user object
      const userToStore = { ...response.user, roles: response.roles ?? response.user.roles };
      localStorage.setItem('user', JSON.stringify(userToStore));
      // Update the zustand auth store so components react immediately
      setUser(userToStore);
      message.success('Login successful!');
      navigate('/', { replace: true });
    } catch (error) {
      const apiError = error as ApiError;
      message.error(apiError.message || 'An unknown error occurred.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Layout style={{ minHeight: '100vh', background: '#f0f2f5' }}>
      <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
        <Card title="Login" style={{ width: 400 }}>
          <LoginForm isLoading={isLoading} onSubmit={handleLogin} />
          <div style={{ marginTop: 12, textAlign: 'center' }}>
            <a href="/register">Don't have an account? Register</a>
          </div>
        </Card>
      </Content>
    </Layout>
  );
};

export default LoginPage;
