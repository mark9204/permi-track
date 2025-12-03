import React, { useEffect } from 'react';
import { Card, Form, Input, Button, message, Spin, Divider, Typography } from 'antd';
import { UserOutlined, MailOutlined, LockOutlined, TeamOutlined } from '@ant-design/icons';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { authService } from '../../../services/authService';
import { useAuthStore } from '../../../stores/authStore';
import type { UpdateProfileRequest } from '../../../types/auth.types';

const { Title } = Typography;

const UserProfilePage: React.FC = () => {
  const [form] = Form.useForm();
  const { user, setUser } = useAuthStore();
  const queryClient = useQueryClient();

  // Fetch latest user data
  const { data: currentUser, isLoading } = useQuery({
    queryKey: ['currentUser'],
    queryFn: authService.getCurrentUser,
  });

  useEffect(() => {
    if (currentUser) {
      form.setFieldsValue({
        firstName: currentUser.firstName,
        lastName: currentUser.lastName,
        email: currentUser.email,
        department: currentUser.department,
      });
      // Update store if needed
      if (user && (user.firstName !== currentUser.firstName || user.lastName !== currentUser.lastName)) {
        setUser({ ...user, ...currentUser });
      }
    }
  }, [currentUser, form, setUser, user]);

  const updateMutation = useMutation({
    mutationFn: (data: UpdateProfileRequest) => authService.updateProfile(data),
    onSuccess: (updatedUser) => {
      message.success('Profile updated successfully');
      setUser({ ...user!, ...updatedUser });
      queryClient.invalidateQueries({ queryKey: ['currentUser'] });
      // Clear password fields
      form.setFieldsValue({
        password: '',
        currentPassword: '',
        confirmPassword: '',
      });
    },
    onError: (error: any) => {
      message.error(error?.response?.data?.message || 'Failed to update profile');
    },
  });

  const onFinish = (values: any) => {
    const { confirmPassword, ...data } = values;
    
    if (data.password && data.password !== confirmPassword) {
      message.error('Passwords do not match!');
      return;
    }

    // Remove empty password fields if not changing password
    if (!data.password) {
      delete data.password;
      delete data.confirmPassword;
    }

    updateMutation.mutate(data);
  };

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 800, margin: '0 auto' }}>
      <Card>
        <Title level={2} style={{ textAlign: 'center', marginBottom: 32 }}>Edit Profile</Title>
        
        <Form
          form={form}
          layout="vertical"
          onFinish={onFinish}
          initialValues={{
            firstName: user?.firstName,
            lastName: user?.lastName,
            email: user?.email,
            department: user?.department,
          }}
        >
          <Divider>Personal Information</Divider>
          
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="firstName"
              label="First Name"
              rules={[{ required: true, message: 'Please input your first name!' }]}
            >
              <Input prefix={<UserOutlined />} placeholder="First Name" />
            </Form.Item>

            <Form.Item
              name="lastName"
              label="Last Name"
              rules={[{ required: true, message: 'Please input your last name!' }]}
            >
              <Input prefix={<UserOutlined />} placeholder="Last Name" />
            </Form.Item>
          </div>

          <Form.Item
            name="email"
            label="Email"
            rules={[
              { required: true, message: 'Please input your email!' },
              { type: 'email', message: 'Please enter a valid email!' }
            ]}
          >
            <Input prefix={<MailOutlined />} placeholder="Email" />
          </Form.Item>

          <Form.Item
            name="department"
            label="Department"
          >
            <Input prefix={<TeamOutlined />} placeholder="Department (e.g. IT, HR, Sales)" />
          </Form.Item>

          <Divider>Security (Optional)</Divider>
          
          <Form.Item
            name="currentPassword"
            label="Current Password"
            help="Required only if changing password"
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Current Password" />
          </Form.Item>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="password"
              label="New Password"
              rules={[{ min: 6, message: 'Password must be at least 6 characters' }]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="New Password" />
            </Form.Item>

            <Form.Item
              name="confirmPassword"
              label="Confirm New Password"
              dependencies={['password']}
              rules={[
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue('password') === value) {
                      return Promise.resolve();
                    }
                    return Promise.reject(new Error('The two passwords that you entered do not match!'));
                  },
                }),
              ]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="Confirm New Password" />
            </Form.Item>
          </div>

          <Form.Item>
            <Button type="primary" htmlType="submit" loading={updateMutation.isPending} block size="large">
              Save Changes
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};

export default UserProfilePage;
